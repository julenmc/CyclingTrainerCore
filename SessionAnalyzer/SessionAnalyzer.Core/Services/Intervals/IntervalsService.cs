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

        public static List<Interval> Search(
            List<FitnessData> activityPoints,
            List<CoreModels.Zone> powerZones,
            Thresholds? thresholds = null)
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
            Log.Info($"Starting short interval search...");
            CoreModels.Zone zone = new CoreModels.Zone
            {
                HighLimit = powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Short] + 1)?.HighLimit ?? 0,
                LowLimit = powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Short])?.LowLimit ?? 0
            };
            List<Interval> tmp = Search(IntervalSearchValues.ShortIntervals.Default, thresholds, AveragePowerCalculator.ShortWindowSize,
                                        IntervalTimes.IntervalMinTimes[IntervalGroups.Short], zone);
            Log.Info($"Short intervals search done. {tmp.Count} intervals found");
            intervals.AddRange(tmp);

            Log.Info($"Starting medium interval search...");
            zone = new CoreModels.Zone
            {
                HighLimit = powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Short])?.HighLimit ?? 0,
                LowLimit = powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Medium])?.LowLimit ?? 0
            };
            tmp = Search(IntervalSearchValues.MediumIntervals.Default, thresholds, AveragePowerCalculator.MediumWindowSize,
                         IntervalTimes.IntervalMinTimes[IntervalGroups.Medium], zone);
            Log.Info($"Medium intervals search done. {tmp.Count} intervals found");
            intervals.AddRange(tmp);

            Log.Info($"Starting long interval search...");
            zone = new CoreModels.Zone
            {
                HighLimit = powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Medium])?.HighLimit ?? 0,
                LowLimit = powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Long])?.LowLimit ?? 0
            };
            tmp = Search(IntervalSearchValues.LongIntervals.Default, thresholds, AveragePowerCalculator.LongWindowSize,
                         IntervalTimes.IntervalMinTimes[IntervalGroups.Long], zone);
            Log.Info($"Long intervals search done. {tmp.Count} intervals found");
            intervals.AddRange(tmp);

            // Integrar intervalos
            IntegrateIntervals(intervals, powerZones);

            Log.Info($"Interval search completed. Found {intervals.Count} main intervals");
            intervals.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
            return intervals;
        }

        private static List<Interval> Search(Thresholds defaultThresholds, Thresholds? thresholds, int windowSize, int minTime, CoreModels.Zone powerZone)
        {
            // Inicializar con valores por defecto si no se proporcionan
            float cvStartThr = thresholds != null ? thresholds.CvStart : defaultThresholds.CvStart;
            float cvFollowThr = thresholds != null ? thresholds.CvFollow : defaultThresholds.CvFollow;
            float rangeThr = thresholds != null ? thresholds.Range : defaultThresholds.Range;
            float maRelThr = thresholds != null ? thresholds.MaRel : defaultThresholds.MaRel;

            Log.Debug($"Using thresholds: cvStart={cvStartThr}, cvFollow={cvFollowThr}, range={rangeThr}, maRel={maRelThr}. minPower={powerZone.LowLimit}W, maxPower={powerZone.HighLimit}W");

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
                while (i < powerModels.Count && !IsIntervalStart(powerModels[i], cvStartThr, rangeThr, powerZone.HighLimit))
                    i++;

                if (i >= powerModels.Count)
                    break;

                var startIndex = i;
                var startTime = powerModels[i].PointDate;
                float referenceAverage = powerModels[i].AvrgPower;
                int totalPower = 0;
                int pointCount = 0;
                Log.Debug($"New interval might start at: startDate={startTime.TimeOfDay}");

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
                        Log.Debug($"Unstable point found at {powerModels[i].PointDate.TimeOfDay} ({i}): CV={current.CoefficientOfVariation}, Deviation={current.DeviationFromReference}. Count: {unstableCount}");
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

                int auxIndex = i >= powerModels.Count ? i - 1 : i;
                var endTime = powerModels[Math.Max(0, auxIndex)].PointDate;
                var duration = (endTime - startTime).TotalSeconds + 1;

                var newInterval = new Interval
                {
                    StartTime = startTime,
                    EndTime = endTime,
                    TimeDiff = (int)duration,
                    AveragePower = referenceAverage,
                };

                // Refinar los límites del intervalo
                RefineIntervalLimits(newInterval, remainingPoints, maRelThr, windowSize);
                Log.Debug($"Found interval: Time={newInterval.StartTime.TimeOfDay}-{newInterval.EndTime.TimeOfDay} ({newInterval.TimeDiff}s), avgPower={newInterval.AveragePower}W");

                if (IsConsideredAnInterval(newInterval, minTime, powerZone.LowLimit))
                {
                    Log.Info($"Saved interval: Time={newInterval.StartTime.TimeOfDay}-{newInterval.EndTime.TimeOfDay} ({newInterval.TimeDiff}s), avgPower={newInterval.AveragePower}W");
                    intervals.Add(newInterval);
                }
                else Log.Debug($"Interval not saved. Min time: {minTime} s. Min power: {powerZone.LowLimit} W");
            }

            return intervals;
        }

        private static bool IsIntervalStart(AveragePowerModel point, float cvThreshold, float rangeThreshold, float maxAvgPower)
        {
            //Log.Debug($"Checking interval start at {point.PointDate}: CV={point.CoefficientOfVariation}, Range={point.RangePercent}");

            return point.CoefficientOfVariation <= cvThreshold &&
                   point.RangePercent <= rangeThreshold &&
                   point.AvrgPower <= maxAvgPower;
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
            int extraPoints = windowSize * 3;
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

        private static bool IsConsideredAnInterval(Interval interval, int minTime, int lowLimit)
        {
            try
            {
                return interval.TimeDiff >= minTime && interval.AveragePower >= lowLimit;
            }
            catch { return false; }
        }

        private static void IntegrateIntervals(List<Interval> intervals, List<CoreModels.Zone> powerZones)
        {
            Log.Debug($"Starting interval integration with {intervals.Count} intervals...");

            intervals.Sort((a, b) => b.TimeDiff.CompareTo(a.TimeDiff));

            for (int i = 0; i < intervals.Count; i++)
            {
                var current = intervals[i];
                current.Intervals = new List<Interval>();
                for (int j = i + 1; j < intervals.Count; j++)
                {
                    var potential = intervals[j];

                    var result = CheckIntervalsIntegration(current, ref potential, powerZones);
                    if (result.Item1)
                    {
                        intervals.RemoveAt(j);
                        j--;
                        Log.Debug($"{result.Item2} between: interval starting at {potential.StartTime.TimeOfDay} with {potential.AveragePower} W in {potential.TimeDiff} s and the interval starting at {current.StartTime.TimeOfDay} with {current.AveragePower} W in {current.TimeDiff} s");
                    }
                }
            }

            Log.Debug($"{intervals.Count} intervals after integration");

            foreach (Interval interval in intervals)
            {
                if (interval.Intervals != null && interval.Intervals.Count != 0)
                {
                    IntegrateIntervals(interval.Intervals, powerZones);
                }
            }
        }

        private static (bool, string) CheckIntervalsIntegration(Interval parent, ref Interval child, List<CoreModels.Zone> powerZones)
        {
            bool delete = false;
            string info = "";

            if (IsSubInterval(parent, child))
            {
                delete = true;
                if (IsSameInterval(parent, child))
                    info = "SameInterval";
                else if (child.AveragePower < parent.AveragePower)
                    info = "LessPower";
                else
                {
                    info = "SubInterval";
                    parent.Intervals?.Add(child);
                }
            }
            else if (DoIntervalsCollide(parent, child))
            {
                GenerateNewIntervalFromCollition(parent, ref child);    // TODO: divide by two the interval

                IntervalGroups group = GetGroup(child);
                if (group == IntervalGroups.Nule)
                {
                    delete = true;
                    info = "Collision";
                    return (delete, info);
                }
                int minPower = powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[group])?.LowLimit ?? 0;
                if (!IsConsideredAnInterval(child, IntervalTimes.IntervalMinTimes[group], minPower))
                {
                    delete = true;
                    info = "Collision";
                }
                else
                {
                    Log.Debug($"Interval modified to: startTime {child.StartTime} with {child.AveragePower} W in {child.TimeDiff} s");
                }
            }

            return (delete, info);
        }

        private static void GenerateNewIntervalFromCollition(Interval parent, ref Interval child)
        {
            var newEndTime = parent.StartTime.AddSeconds(-1);
            var startTime = child.StartTime;  // Guardar en variable local para usar en lambda
            
            Log.Debug($"Before adjustsment: child interval at {child.StartTime.TimeOfDay}-{child.EndTime.TimeOfDay} ({child.TimeDiff}s), avgPower={child.AveragePower}W.");

            var remainingPoints = IntervalRepository.GetRemainingFitnessData();
            var points = remainingPoints
                .Where(p => 
                {
                    var timestamp = p.Timestamp.GetDateTime();
                    return timestamp >= startTime && timestamp <= newEndTime;
                })
                .ToList();

            if (!points.Any())
            {
                child.TimeDiff = 0;
                return;
            }

            child.EndTime = newEndTime;
            child.TimeDiff = (int)(newEndTime - startTime).TotalSeconds + 1;
            child.AveragePower = (float)points.Average(p => p.Stats.Power ?? 0);
            
            Log.Debug($"Collision adjusted: child interval now ends at {newEndTime.TimeOfDay}, new duration={child.TimeDiff}s, new avgPower={child.AveragePower}W.");
        }
        
        private static IntervalGroups GetGroup(Interval interval) =>
            interval.TimeDiff switch
            {
                < IntervalTimes.IntervalMinTime => IntervalGroups.Nule,
                < IntervalTimes.MediumIntervalMinTime => IntervalGroups.Short,
                < IntervalTimes.LongIntervalMinTime => IntervalGroups.Medium,
                _ => IntervalGroups.Long
            };



        private static bool IsSubInterval(Interval parent, Interval child)
        {
            int timeExpand = parent.TimeDiff / 20;  // 5% time expansion
            return child.StartTime >= parent.StartTime.AddSeconds(-timeExpand) &&
                   child.EndTime <= parent.EndTime.AddSeconds(timeExpand) &&
                   child != parent;
        }

        private static bool IsSameInterval(Interval parent, Interval child)     // Always called after IsSubInterval
        {
            return (parent.StartTime == child.StartTime && parent.EndTime == child.EndTime) ||
                    ((float)child.TimeDiff / (float)parent.TimeDiff >= 0.8f);      // Childs time diff has to be at max 80% of parents, this solves small diffs in the search algorithm
        }

        private static bool DoIntervalsCollide(Interval parent, Interval child)
        {
            return (child.StartTime < parent.StartTime && child.EndTime > parent.StartTime);
        }
    }
}