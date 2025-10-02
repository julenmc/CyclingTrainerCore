using CyclingTrainer.SessionAnalyzer.Core.Constants;
using CyclingTrainer.SessionAnalyzer.Core.Models;
using CyclingTrainer.SessionReader.Core.Models;
using NLog;

namespace CyclingTrainer.SessionAnalyzer.Core.Services.Intervals
{
    internal static class IntervalsService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        internal static List<Interval> FindIntervals(
            List<FitnessData> activityPoints,
            float? cvStartThreshold = null,
            float? cvFollowThreshold = null,
            float? rangeThreshold = null,
            float? maRelThreshold = null)
        {
            Log.Info("Starting intervals search...");
            if (activityPoints == null || !activityPoints.Any())
            {
                Log.Warn("No activity points provided");
                return new List<Interval>();
            }

            // Inicializar con valores por defecto si no se proporcionan
            float cvStartThr = cvStartThreshold ?? IntervalSearchValues.CvStartThrDefaultValue;
            float cvFollowThr = cvFollowThreshold ?? IntervalSearchValues.CvFollowThrDefaultValue;
            float rangeThr = rangeThreshold ?? IntervalSearchValues.RangeThrDefaultValue;
            float maRelThr = maRelThreshold ?? IntervalSearchValues.MaRelThrDefaultValue;

            Log.Debug($"Using thresholds: cvStart={cvStartThr}, cvFollow={cvFollowThr}, range={rangeThr}, maRel={maRelThr}");

            // Validar los valores
            ValidateThresholds(ref cvStartThr, ref cvFollowThr, ref rangeThr, ref maRelThr);

            // Limpiar el repositorio para el nuevo análisis
            IntervalRepository.Clear();
            IntervalRepository.SetFitnessData(activityPoints);

            // Detectar y eliminar sprints primero
            Log.Info("Starting sprint detection and removal...");
            SprintService.SetConfiguration(30, 400, 300); // Valores ejemplo para sprint
            SprintService.AnalyzeActivity(activityPoints);
            Log.Info("Sprint detection completed");

            // Obtener los datos restantes después de eliminar sprints
            var remainingPoints = IntervalRepository.GetRemainingFitnessData();

            // Calcular medias móviles para diferentes ventanas de tiempo
            Log.Info("Calculating moving averages...");
            var powerModels = AveragePowerCalculator.CalculateWindowAverages(remainingPoints);
            Log.Debug($"Generated {powerModels.Count} power models");
            var intervals = new List<Interval>();

            // Buscar intervalos
            int i = 0;
            while (i < powerModels.Count)
            {
                // Buscar inicio de intervalo potencial
                while (i < powerModels.Count && !IsIntervalStart(powerModels[i], cvStartThr, rangeThr))
                    i++;

                if (i >= powerModels.Count)
                    break;

                var startIndex = i;
                var startTime = powerModels[i].PointDate;
                float referenceAverage = powerModels[i].AvrgPower;
                int totalPower = 0;
                int pointCount = 0;

                // Seguir el intervalo mientras se mantenga estable
                int unstableCount = 0;
                while (i < powerModels.Count)
                {
                    var current = powerModels[i];
                    AveragePowerCalculator.CalculateDeviationFromReference(powerModels.Skip(startIndex).Take(i - startIndex + 1).ToList(), referenceAverage);

                    if (!IsIntervalContinuation(current, cvFollowThr, maRelThr))
                    {
                        unstableCount++;
                        if (unstableCount >= IntervalTimes.MaxTimeBeforeEnd)
                            break;
                    }
                    else
                    {
                        unstableCount = 0;
                    }

                    totalPower += (int)current.AvrgPower;
                    pointCount++;
                    referenceAverage = (float)totalPower / pointCount;
                    i++;
                }

                // Si el intervalo es lo suficientemente largo, guardarlo
                var endTime = powerModels[Math.Max(0, i - IntervalTimes.MaxTimeBeforeEnd)].PointDate;
                var duration = (endTime - startTime).TotalSeconds + 1;

                if (duration >= IntervalTimes.IntervalMinTime && duration <= IntervalTimes.IntervalMaxTime)
                {
                    Log.Debug($"Found interval: duration={duration}s, avgPower={totalPower / pointCount}W");
                    var newInterval = new Interval
                    {
                        StartTime = startTime,
                        EndTime = endTime,
                        TimeDiff = (int)duration,
                        AveragePower = totalPower / pointCount,
                        Intervals = new List<Interval>() // Para futuros subintervalos
                    };

                    // Refinar los límites del intervalo
                    RefineIntervalLimits(newInterval, remainingPoints);
                    intervals.Add(newInterval);
                }
            }

            // Integrar intervalos
            Log.Info($"Starting interval integration with {intervals.Count} intervals...");
            IntegrateIntervals(intervals);
            Log.Info($"Interval integration completed. Final interval count: {intervals.Count}");

            // Refinar los límites de todos los intervalos integrados
            Log.Info("Starting final refinement of all intervals...");
            foreach (var interval in intervals)
            {
                RefineIntervalLimits(interval, remainingPoints);
                if (interval.Intervals != null)
                {
                    foreach (var subInterval in interval.Intervals)
                    {
                        RefineIntervalLimits(subInterval, remainingPoints);
                    }
                }
            }
            Log.Info("Final refinement completed");

            Log.Info($"Interval search completed. Found {intervals.Count} main intervals");
            return intervals;
        }

        private static void ValidateThresholds(
            ref float cvStartThreshold,
            ref float cvFollowThreshold,
            ref float rangeThreshold,
            ref float maRelThreshold)
        {
            Log.Debug("Validating and adjusting thresholds...");

            cvStartThreshold = Math.Clamp(
                cvStartThreshold,
                IntervalSearchValues.CvStartThrMinValue,
                IntervalSearchValues.CvStartThrMaxValue);

            cvFollowThreshold = Math.Clamp(
                cvFollowThreshold,
                IntervalSearchValues.CvFollowThrMinValue,
                IntervalSearchValues.CvFollowThrMaxValue);

            rangeThreshold = Math.Clamp(
                rangeThreshold,
                IntervalSearchValues.RangeThrMinValue,
                IntervalSearchValues.RangeThrMaxValue);

            maRelThreshold = Math.Clamp(
                maRelThreshold,
                IntervalSearchValues.MaRelThrMinValue,
                IntervalSearchValues.MaRelThrMaxValue);
        }

        private static bool IsIntervalStart(AveragePowerModel point, float cvThreshold, float rangeThreshold)
        {
            //Log.Debug($"Checking interval start at {point.PointDate}: CV={point.CoefficientOfVariation}, Range={point.RangePercent}");

            return point.CoefficientOfVariation <= cvThreshold &&
                   point.RangePercent <= rangeThreshold;
        }

        private static bool IsIntervalContinuation(AveragePowerModel point, float cvThreshold, float deviationThreshold)
        {
            //Log.Debug($"Checking interval continuation at {point.PointDate}: CV={point.CoefficientOfVariation}, Deviation={point.DeviationFromReference}");

            return point.CoefficientOfVariation <= cvThreshold &&
                   point.DeviationFromReference <= deviationThreshold;
        }

        private static void IntegrateIntervals(List<Interval> intervals)
        {
            Log.Debug($"Starting interval integration with {intervals.Count} intervals...");

            intervals.Sort((a, b) => b.TimeDiff.CompareTo(a.TimeDiff));

            for (int i = 0; i < intervals.Count; i++)
            {
                var current = intervals[i];
                for (int j = i + 1; j < intervals.Count; j++)
                {
                    var potential = intervals[j];
                    if (IsSubInterval(current, potential))
                    {
                        current.Intervals ??= new List<Interval>();
                        current.Intervals.Add(potential);
                        intervals.RemoveAt(j);
                        j--;
                    }
                }
            }
        }

        private static bool IsSubInterval(Interval parent, Interval child)
        {
            return child.StartTime >= parent.StartTime &&
                   child.EndTime <= parent.EndTime &&
                   child != parent;
        }

        private static void RefineIntervalLimits(Interval interval, List<FitnessData> points)
        {
            Log.Debug($"Refining interval limits for interval at {interval.StartTime}");
            
            // Encontrar el índice del punto inicial y final del intervalo en la lista completa
            int intervalStartIdx = points.FindIndex(p => p.Timestamp.GetDateTime() >= interval.StartTime);
            int intervalEndIdx = points.FindLastIndex(p => p.Timestamp.GetDateTime() <= interval.EndTime);

            if (intervalStartIdx == -1 || intervalEndIdx == -1 || intervalStartIdx > intervalEndIdx)
            {
                Log.Warn("Invalid interval indices found during refinement");
                return;
            }

            // Expandir el rango para incluir puntos contiguos
            const int extraPoints = 30; // Buscar hasta 30 segundos extra en cada dirección
            int expandedStartIdx = Math.Max(0, intervalStartIdx - extraPoints);
            int expandedEndIdx = Math.Min(points.Count - 1, intervalEndIdx + extraPoints);

            var expandedPoints = points.GetRange(expandedStartIdx, expandedEndIdx - expandedStartIdx + 1);
            
            if (!expandedPoints.Any())
            {
                Log.Warn("No points found for interval refinement");
                return;
            }

            float targetPower = interval.AveragePower;
            float allowedDeviation = targetPower * IntervalSearchValues.MaRelThrDefaultValue;

            // Refinar límite inicial - buscar hacia atrás desde el punto inicial
            var startIndex = expandedPoints.FindIndex(
                p => p.Timestamp.GetDateTime() >= interval.StartTime);
            
            // Buscar hacia atrás para encontrar el verdadero inicio
            while (startIndex > 0)
            {
                var prevPower = expandedPoints[startIndex - 1].Stats.Power ?? 0;
                if (Math.Abs(prevPower - targetPower) > allowedDeviation)
                    break;
                startIndex--;
            }

            // Buscar hacia adelante si es necesario
            while (startIndex < expandedPoints.Count - 1 && 
                   Math.Abs((expandedPoints[startIndex].Stats.Power ?? 0) - targetPower) > allowedDeviation)
            {
                startIndex++;
            }

            // Refinar límite final - buscar hacia adelante desde el punto final
            var endIndex = expandedPoints.FindLastIndex(
                p => p.Timestamp.GetDateTime() <= interval.EndTime);
            
            // Buscar hacia adelante para encontrar el verdadero final
            while (endIndex < expandedPoints.Count - 1)
            {
                var nextPower = expandedPoints[endIndex + 1].Stats.Power ?? 0;
                if (Math.Abs(nextPower - targetPower) > allowedDeviation)
                    break;
                endIndex++;
            }

            // Buscar hacia atrás si es necesario
            while (endIndex > startIndex && 
                   Math.Abs((expandedPoints[endIndex].Stats.Power ?? 0) - targetPower) > allowedDeviation)
            {
                endIndex--;
            }

            // Actualizar los límites si se encontró un rango válido
            if (startIndex < endIndex)
            {
                var newStartTime = expandedPoints[startIndex].Timestamp.GetDateTime();
                var newEndTime = expandedPoints[endIndex].Timestamp.GetDateTime();
                var duration = (newEndTime - newStartTime).TotalSeconds + 1;

                // Solo actualizar si el nuevo intervalo cumple con el tiempo mínimo
                if (duration >= IntervalTimes.IntervalMinTime)
                {
                    Log.Debug($"Refined interval limits: {interval.StartTime} -> {newStartTime}, {interval.EndTime} -> {newEndTime}");
                    interval.StartTime = newStartTime;
                    interval.EndTime = newEndTime;
                    interval.TimeDiff = (int)duration;

                    // Recalcular la potencia media con los nuevos límites
                    var powers = expandedPoints
                        .Skip(startIndex)
                        .Take(endIndex - startIndex + 1)
                        .Select(p => p.Stats.Power ?? 0);
                    interval.AveragePower = (float)powers.Average();
                }
                else
                {
                    Log.Debug("Refined interval too short, keeping original limits");
                }
            }
            else
            {
                Log.Debug("Could not find suitable refined limits, keeping original");
            }
        }
    }
}