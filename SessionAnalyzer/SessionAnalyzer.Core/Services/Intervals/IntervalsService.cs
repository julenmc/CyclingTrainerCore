using CoreModels = CyclingTrainer.Core.Models;
using CyclingTrainer.SessionAnalyzer.Core.Constants;
using CyclingTrainer.SessionAnalyzer.Core.Models;
using CyclingTrainer.SessionReader.Core.Models;
using NLog;
using CyclingTrainer.SessionAnalyzer.Core.Enums;

namespace CyclingTrainer.SessionAnalyzer.Core.Services.Intervals
{
    public static class IntervalsService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public struct DetectionThresholds
        {
            public int CvStartThreshold { get; set; }
            public int CvFollowThreshold { get; set; }
            public int RangeThreshold { get; set; }
            public int MaRelThreshold { get; set; }
        }

        public static List<Interval> Search(
            List<FitnessData> activityPoints,
            List<CoreModels.Zone> powerZones,
            DetectionThresholds? thresholds = null)
        {
            Log.Info("Starting intervals search...");
            if (activityPoints == null || !activityPoints.Any())
            {
                Log.Warn("No activity points provided");
                return new List<Interval>();
            }

            // Limpiar el repositorio para el nuevo análisis
            IntervalRepository.Clear();
            IntervalRepository.SetFitnessData(activityPoints);

            // Detectar y eliminar sprints primero
            CoreModels.Zone? sprint = powerZones.Find(x => x.Id == 7);
            if (sprint == null)
            {
                Log.Warn("No sprint power zone found");
                return new List<Interval>();
            }
            int sprintPower = sprint.LowLimit;
            Log.Info($"Starting sprint detection and removal at {sprintPower} W...");
            SprintService.SetConfiguration(5, sprintPower * 11 / 10, sprintPower);
            SprintService.AnalyzeActivity(activityPoints);
            Log.Info("Sprint detection completed");

            List<Interval> intervals = new List<Interval>();
            intervals.AddRange(Search(IntervalSearchValues.ShortIntervals, thresholds, AveragePowerCalculator.ShortWindowSize, powerZones));
            Log.Debug("Short intervals search done");
            intervals.AddRange(Search(IntervalSearchValues.MediumIntervals, thresholds, AveragePowerCalculator.MediumWindowSize, powerZones));
            Log.Debug("Medium intervals search done");
            intervals.AddRange(Search(IntervalSearchValues.LongIntervals, thresholds, AveragePowerCalculator.LongWindowSize, powerZones));
            Log.Debug("Long intervals search done");

            // Integrar intervalos
            IntegrateIntervals(intervals);

            Log.Info($"Interval search completed. Found {intervals.Count} main intervals");
            intervals.Sort((a, b) => b.StartTime.CompareTo(a.StartTime));
            return intervals;
        }

        private static List<Interval> Search(Parameters parameters, DetectionThresholds? thresholds, int windowSize, List<CoreModels.Zone> powerZones)
        {
            float GetCustomValue(int p, Thresholds t)
            {
                return (t.Max - t.Min) * p + t.Min;
            }

            // Inicializar con valores por defecto si no se proporcionan
            float cvStartThr = thresholds != null ? GetCustomValue(thresholds.Value.CvStartThreshold, parameters.CvStart) : parameters.CvStart.Default;
            float cvFollowThr = thresholds != null ? GetCustomValue(thresholds.Value.CvFollowThreshold, parameters.CvFollow) : parameters.CvFollow.Default;
            float rangeThr = thresholds != null ? GetCustomValue(thresholds.Value.RangeThreshold, parameters.Range) : parameters.Range.Default;
            float maRelThr = thresholds != null ? GetCustomValue(thresholds.Value.MaRelThreshold, parameters.MaRel) : parameters.MaRel.Default;

            Log.Debug($"Using thresholds: cvStart={cvStartThr}, cvFollow={cvFollowThr}, range={rangeThr}, maRel={maRelThr}");

            // Obtener los datos restantes después de eliminar sprints
            var remainingPoints = IntervalRepository.GetRemainingFitnessData();

            // Calcular medias móviles para diferentes ventanas de tiempo
            Log.Info("Calculating moving averages...");
            var powerModels = AveragePowerCalculator.CalculateMovingAverages(remainingPoints, windowSize);
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
                    //AveragePowerCalculator.CalculateDeviationFromReference(powerModels.Skip(startIndex).Take(i - startIndex + 1).ToList(), referenceAverage);
                    current.DeviationFromReference = Math.Abs(current.AvrgPower - referenceAverage) / referenceAverage;

                    if (!IsIntervalContinuation(current, cvFollowThr, maRelThr))
                    {
                        unstableCount++;
                        Log.Debug($"Unstable point found at index {i}: CV={current.CoefficientOfVariation}, Range={current.RangePercent}, Deviation={current.DeviationFromReference}. Count: {unstableCount}");
                        if (unstableCount >= windowSize)
                            break;
                        pointCount++;
                        totalPower += (int)current.AvrgPower;
                    }
                    else
                    {
                        unstableCount = 0;
                        pointCount++;
                        totalPower += (int)current.AvrgPower;
                        referenceAverage = (float)totalPower / pointCount;
                    }

                    i++;
                }

                // Si el intervalo es lo suficientemente largo, guardarlo
                var endTime = powerModels[Math.Max(0, i - windowSize)].PointDate;
                var duration = (endTime - startTime).TotalSeconds + 1;

                if (duration >= IntervalTimes.IntervalMinTime && duration <= IntervalTimes.IntervalMaxTime)
                {
                    var newInterval = new Interval
                    {
                        StartTime = startTime,
                        EndTime = endTime,
                        TimeDiff = (int)duration,
                        AveragePower = referenceAverage,
                    };

                    // Refinar los límites del intervalo
                    RefineIntervalLimits(newInterval, remainingPoints, maRelThr, windowSize);
                    if (IsConsideredAnInterval(newInterval, powerZones))
                    {
                        Log.Debug($"Found interval: startDate={newInterval.StartTime}, duration={newInterval.TimeDiff}s, avgPower={newInterval.AveragePower}W");
                        intervals.Add(newInterval);
                    }
                }
            }

            return intervals;
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

        private static void RefineIntervalLimits(Interval interval, List<FitnessData> points, float maRelThr, int windowSize)
        {
            // Log.Debug($"Refining interval limits for interval at {interval.StartTime}");

            // Encontrar el índice del punto inicial y final del intervalo en la lista completa
            int intervalStartIdx = points.FindIndex(p => p.Timestamp.GetDateTime() >= interval.StartTime);
            int intervalEndIdx = points.FindLastIndex(p => p.Timestamp.GetDateTime() <= interval.EndTime);

            if (intervalStartIdx == -1 || intervalEndIdx == -1 || intervalStartIdx > intervalEndIdx)
            {
                Log.Warn("Invalid interval indices found during refinement");
                return;
            }

            // Expandir el rango para incluir puntos contiguos
            int extraPoints = windowSize * 2;     
            int expandedStartIdx = Math.Max(0, intervalStartIdx - extraPoints);
            int expandedEndIdx = Math.Min(points.Count - 1, intervalEndIdx + extraPoints);

            var expandedPoints = points.GetRange(expandedStartIdx, expandedEndIdx - expandedStartIdx + 1);

            if (!expandedPoints.Any())
            {
                Log.Warn("No points found for interval refinement");
                return;
            }

            float targetPower = interval.AveragePower;
            float allowedDeviation = targetPower * maRelThr;

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
                    // Log.Debug($"Refined interval limits: {interval.StartTime} -> {newStartTime}, {interval.EndTime} -> {newEndTime}");
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
                    // Log.Debug("Refined interval too short, keeping original limits");
                }
            }
            else
            {
                // Log.Debug("Could not find suitable refined limits, keeping original");
            }
        }

        private static bool IsConsideredAnInterval(Interval interval, List<CoreModels.Zone> powerZones)
        {
            try
            {
                IntervalGroups group = GetGroup(interval);
                ushort zoneId = IntervalTimes.IntervalMinTimes[group];
                CoreModels.Zone? zone = powerZones.Find(x => x.Id == zoneId);
                if (zone == null) return false;
                return interval.AveragePower >= zone.LowLimit;
            }
            catch { return false; }
        }

        private static IntervalGroups GetGroup(Interval interval) =>
            interval.TimeDiff switch
            {
                < IntervalTimes.IntervalMinTime       => IntervalGroups.Nule,
                < IntervalTimes.MediumIntervalMinTime => IntervalGroups.Short,
                < IntervalTimes.LongIntervalMinTime   => IntervalGroups.Medium,
                _                                     => IntervalGroups.Long
            };

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
                        if (!(current.StartTime == potential.StartTime && current.EndTime == potential.EndTime))    // Remove if they are the same
                        {
                            current.Intervals ??= new List<Interval>();
                            current.Intervals.Add(potential);
                        }
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
    }
}