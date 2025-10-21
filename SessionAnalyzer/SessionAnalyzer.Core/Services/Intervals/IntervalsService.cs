using CoreModels = CyclingTrainer.Core.Models;
using CyclingTrainer.SessionAnalyzer.Constants;
using CyclingTrainer.SessionAnalyzer.Models;
using CyclingTrainer.SessionReader.Models;
using NLog;
using CyclingTrainer.SessionAnalyzer.Enums;

namespace CyclingTrainer.SessionAnalyzer.Services.Intervals
{
    public class IntervalsService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private List<CoreModels.Zone> _powerZones;
        private Thresholds? _thresholds;

        public IntervalsService(List<CoreModels.Zone> powerZones, Thresholds? thresholds = null)
        {
            _powerZones = powerZones;
            _thresholds = thresholds;
        }

        public List<Interval> Search(List<FitnessData> activityPoints)
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
            CoreModels.Zone? sprint = _powerZones.Find(x => x.Id == 7);
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

            // Short intervals
            List<Interval> intervals = new List<Interval>();
            Log.Info($"Starting short interval search...");
            CoreModels.Zone zone = new CoreModels.Zone
            {
                HighLimit = _powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Short] + 2)?.HighLimit ?? 0,
                LowLimit = _powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Short])?.LowLimit ?? 0
            };
            List<Interval> tmp = Search(IntervalSearchValues.ShortIntervals.Default, AveragePowerCalculator.ShortWindowSize, zone);
            Log.Info($"Short intervals search done. {tmp.Count} intervals found");
            intervals.AddRange(tmp);

            // Medium intervals
            Log.Info($"Starting medium interval search...");
            zone = new CoreModels.Zone
            {
                HighLimit = _powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Medium] + 2)?.HighLimit ?? 0,
                LowLimit = _powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Medium])?.LowLimit ?? 0
            };
            tmp = Search(IntervalSearchValues.MediumIntervals.Default, AveragePowerCalculator.MediumWindowSize, zone);

            for (int i = 0; i < tmp.Count; i++)
            {
                if (IntervalAlreadyExists(tmp[i], intervals))
                {
                    Log.Debug($"Interval {tmp[i].StartTime.TimeOfDay}-{tmp[i].EndTime.TimeOfDay} at {tmp[i].AveragePower} already exists");
                    tmp.RemoveAt(i);
                }
            }
            // Delete existing intervals
            Log.Info($"Medium intervals search done. {tmp.Count} new intervals found");
            intervals.AddRange(tmp);

            // Long intervals
            Log.Info($"Starting long interval search...");
            zone = new CoreModels.Zone
            {
                HighLimit = _powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Long] + 2)?.HighLimit ?? 0,
                LowLimit = _powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Long])?.LowLimit ?? 0
            };
            tmp = Search(IntervalSearchValues.LongIntervals.Default, AveragePowerCalculator.LongWindowSize, zone);
            // Delete existing intervals
            for (int i = 0; i < tmp.Count; i++)
            {
                if (IntervalAlreadyExists(tmp[i], intervals))
                {
                    Log.Debug($"Interval {tmp[i].StartTime.TimeOfDay}-{tmp[i].StartTime.TimeOfDay} at {tmp[i].AveragePower} already exists");
                    tmp.RemoveAt(i);
                }
            }
            Log.Info($"Long intervals search done. {tmp.Count} new intervals found");
            intervals.AddRange(tmp);

            // Integrar intervalos
            IntegrateIntervals(intervals);

            Log.Info($"Interval search completed. Found {intervals.Count} main intervals");
            intervals.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
            return intervals;
        }

        private List<Interval> Search(Thresholds defaultThresholds, int windowSize, CoreModels.Zone powerZone)
        {
            // Inicializar con valores por defecto si no se proporcionan
            float cvStartThr = _thresholds != null ? _thresholds.CvStart : defaultThresholds.CvStart;
            float cvFollowThr = _thresholds != null ? _thresholds.CvFollow : defaultThresholds.CvFollow;
            float rangeThr = _thresholds != null ? _thresholds.Range : defaultThresholds.Range;
            float maRelThr = _thresholds != null ? _thresholds.MaRel : defaultThresholds.MaRel;

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
                Log.Debug($"New interval might start at: startDate={startTime.TimeOfDay}: CV={powerModels[i].CoefficientOfVariation}. Range={powerModels[i].RangePercent}");

                // Seguir el intervalo mientras se mantenga estable
                int unstableCount = 0;
                while (i < powerModels.Count)
                {
                    var current = powerModels[i];
                    //AveragePowerCalculator.CalculateDeviationFromReference(powerModels.Skip(startIndex).Take(i - startIndex + 1).ToList(), referenceAverage);
                    current.DeviationFromReference = Math.Abs(current.AvrgPower - referenceAverage) / referenceAverage;

                    if (!IsIntervalContinuation(current, cvFollowThr, maRelThr))
                    {
                        if (unstableCount == 0) Log.Debug($"Unstable point found at {powerModels[i].PointDate.TimeOfDay} ({i}): CV={current.CoefficientOfVariation}, Deviation={current.DeviationFromReference}");
                        unstableCount++;
                        if (unstableCount >= windowSize)
                            break;
                        pointCount++;
                        totalPower += (int)current.AvrgPower;
                    }
                    else
                    {
                        if (unstableCount != 0) Log.Debug($"Unstable point ends at {powerModels[i].PointDate.TimeOfDay} ({i}): CV={current.CoefficientOfVariation}, Deviation={current.DeviationFromReference}. Count: {unstableCount}");
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

                if (IsConsideredAnInterval(newInterval))
                {
                    Log.Info($"Saved interval: Time={newInterval.StartTime.TimeOfDay}-{newInterval.EndTime.TimeOfDay} ({newInterval.TimeDiff}s), avgPower={newInterval.AveragePower}W");
                    intervals.Add(newInterval);
                }
                else Log.Debug($"Interval not saved");
            }

            return intervals;
        }

        private bool IsIntervalStart(AveragePowerModel point, float cvThreshold, float rangeThreshold, float maxAvgPower)
        {
            //Log.Debug($"Checking interval start at {point.PointDate}: CV={point.CoefficientOfVariation}, Range={point.RangePercent}");

            return point.CoefficientOfVariation <= cvThreshold &&
                   point.RangePercent <= rangeThreshold &&
                   point.AvrgPower <= maxAvgPower;
        }

        private bool IsIntervalContinuation(AveragePowerModel point, float cvThreshold, float deviationThreshold)
        {
            //Log.Debug($"Checking interval continuation at {point.PointDate}: CV={point.CoefficientOfVariation}, Deviation={point.DeviationFromReference}");

            return point.CoefficientOfVariation <= cvThreshold &&
                   point.DeviationFromReference <= deviationThreshold;
        }

        private void RefineIntervalLimits(Interval interval, List<FitnessData> points, float maRelThr, int windowSize)
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

        private bool IsConsideredAnInterval(Interval interval) =>
            interval.TimeDiff switch
            {
                >= IntervalTimes.LongIntervalMinTime =>
                    interval.AveragePower > (_powerZones
                        .Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Long])?.LowLimit ?? 0),
                >= IntervalTimes.MediumIntervalMinTime =>
                    interval.AveragePower > (_powerZones
                        .Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Medium])?.LowLimit ?? 0),
                >= IntervalTimes.IntervalMinTime =>
                    interval.AveragePower > (_powerZones
                        .Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Short])?.LowLimit ?? 0),
                _ => false
            };

        private void IntegrateIntervals(List<Interval> intervals)
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

                    var result = CheckIntervalsIntegration(current, ref potential);
                    if (result)
                    {
                        intervals.RemoveAt(j);
                        j--;
                    }
                }
            }

            Log.Debug($"{intervals.Count} intervals after integration");

            foreach (Interval interval in intervals)
            {
                if (interval.Intervals != null && interval.Intervals.Count != 0)
                {
                    IntegrateIntervals(interval.Intervals);
                }
            }
        }

        private bool CheckIntervalsIntegration(Interval parent, ref Interval child)
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
                Log.Debug($"{info} between: child interval {child.StartTime.TimeOfDay}-{child.EndTime.TimeOfDay} at {child.AveragePower} W and parent interval {parent.StartTime.TimeOfDay}-{parent.EndTime.TimeOfDay} at {parent.AveragePower} W");
            }
            else if (DoIntervalsCollide(parent, child))
            {
                Log.Debug($"Collision between: child interval {child.StartTime.TimeOfDay}-{child.EndTime.TimeOfDay} at {child.AveragePower} W and parent interval {parent.StartTime.TimeOfDay}-{parent.EndTime.TimeOfDay} at {parent.AveragePower} W");

                if (child.StartTime < parent.StartTime)     // Collision at start
                {
                    Interval oldChild = new Interval
                    {
                        StartTime = child.StartTime,
                        EndTime = child.EndTime
                    };
                    GenerateNewIntervalBeforeCollitionInStart(parent, ref child);  // Pass reference before collision cuz might still be an interval

                    if (!IsConsideredAnInterval(child))
                    {
                        delete = true;
                        info = "Collision";
                        Log.Debug($"Interval will not be saved after modification");
                    }
                    else
                    {
                        Log.Debug($"Interval will be saved after modification");
                    }

                    Interval newChild = GenerateNewIntervalAfterCollitionInStart(parent, oldChild);       // After collision should be a new interval, no reference
                    if (IsConsideredAnInterval(newChild))   // TODO: check if child's power is lower than parent's
                    {
                        parent.Intervals?.Add(newChild);
                        Log.Debug($"New subInterval added: startTime {newChild.StartTime.TimeOfDay} with {newChild.AveragePower} W in {newChild.TimeDiff} s");
                    }
                }
                else        // Collision at end
                {
                    Interval oldChild = new Interval
                    {
                        StartTime = child.StartTime,
                        EndTime = child.EndTime
                    };
                    Interval newChild = GenerateNewIntervalBeforeCollitionInEnd(parent, oldChild);  // Before collision should be a new interval, no reference

                    if (IsConsideredAnInterval(newChild))   // TODO: check if child's power is lower than parent's
                    {
                        parent.Intervals?.Add(newChild);
                        Log.Debug($"New subInterval added: startTime {newChild.StartTime.TimeOfDay} with {newChild.AveragePower} W in {newChild.TimeDiff} s");
                    }

                    GenerateNewIntervalAfterCollitionInEnd(parent, ref child);       // Pass reference after collision cuz might still be an interval
                    if (!IsConsideredAnInterval(child))
                    {
                        delete = true;
                        info = "Collision";
                        Log.Debug($"Interval will not be saved after modification");
                    }
                    else
                    {
                        Log.Debug($"Interval will be saved after modification");
                    }
                }
            }

            return delete;
        }

        private static void GenerateNewIntervalBeforeCollitionInStart(Interval parent, ref Interval child)
        {
            var newEndTime = parent.StartTime.AddSeconds(-1);
            var startTime = child.StartTime;  // Guardar en variable local para usar en lambda

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

            Log.Debug($"Collision adjusted: interval now ends at {newEndTime.TimeOfDay}, new duration={child.TimeDiff}s, new avgPower={child.AveragePower}W.");
            return;
        }

        private static Interval GenerateNewIntervalAfterCollitionInStart(Interval parent, Interval child)
        {
            var endTime = child.EndTime;         // Save in variable to use in lambda
            var newStartTime = parent.StartTime;

            Interval ret = new Interval{
                StartTime = child.StartTime,
                EndTime = child.EndTime
            };

            var remainingPoints = IntervalRepository.GetRemainingFitnessData();
            var points = remainingPoints
                .Where(p =>
                {
                    var timestamp = p.Timestamp.GetDateTime();
                    return timestamp >= newStartTime && timestamp <= endTime;
                })
                .ToList();

            if (!points.Any())
            {
                ret.TimeDiff = 0;
                return ret;
            }

            ret.StartTime = newStartTime;
            ret.TimeDiff = (int)(endTime - newStartTime).TotalSeconds + 1;
            ret.AveragePower = (float)points.Average(p => p.Stats.Power ?? 0);

            Log.Debug($"New possible child interval starts at {newStartTime.TimeOfDay}, new duration={ret.TimeDiff}s, new avgPower={ret.AveragePower}W.");
            return ret;
        }

        private static Interval GenerateNewIntervalBeforeCollitionInEnd(Interval parent, Interval child)
        {
            var newEndTime = parent.EndTime;
            var startTime = child.StartTime;  // Guardar en variable local para usar en lambda

            Interval ret = new Interval{
                StartTime = child.StartTime,
                EndTime = child.EndTime
            };
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
                ret.TimeDiff = 0;
                return ret;
            }

            ret.EndTime = newEndTime;
            ret.TimeDiff = (int)(newEndTime - startTime).TotalSeconds + 1;
            ret.AveragePower = (float)points.Average(p => p.Stats.Power ?? 0);

            Log.Debug($"New possible child interval starts at {startTime.TimeOfDay}, new duration={ret.TimeDiff}s, new avgPower={ret.AveragePower}W.");
            return ret;
        }

        private static void GenerateNewIntervalAfterCollitionInEnd(Interval parent, ref Interval child)
        {
            var endTime = child.EndTime;         // Save in variable to use in lambda
            var newStartTime = parent.EndTime.AddSeconds(1);

            Interval ret = child;

            var remainingPoints = IntervalRepository.GetRemainingFitnessData();
            var points = remainingPoints
                .Where(p =>
                {
                    var timestamp = p.Timestamp.GetDateTime();
                    return timestamp >= newStartTime && timestamp <= endTime;
                })
                .ToList();

            if (!points.Any())
            {
                ret.TimeDiff = 0;
                return;
            }

            ret.StartTime = newStartTime;
            ret.TimeDiff = (int)(endTime - newStartTime).TotalSeconds + 1;
            ret.AveragePower = (float)points.Average(p => p.Stats.Power ?? 0);

            Log.Debug($"Collision adjusted: interval now starts at {newStartTime.TimeOfDay}, new duration={child.TimeDiff}s, new avgPower={child.AveragePower}W.");
        }

        private static bool IsSubInterval(Interval parent, Interval child)
        {
            return child.StartTime >= parent.StartTime &&
                   child.EndTime <= parent.EndTime &&
                   child != parent;
        }

        private static bool IsSameInterval(Interval parent, Interval child)
        {
            return (parent.StartTime == child.StartTime && parent.EndTime == child.EndTime);
        }

        private static bool DoIntervalsCollide(Interval parent, Interval child)
        {
            return (child.StartTime < parent.StartTime && child.EndTime > parent.StartTime) ||
                   (child.StartTime < parent.EndTime && child.EndTime > parent.EndTime);
        }

        private static bool IntervalAlreadyExists(Interval intervalToCheck, List<Interval> intervals)
        {
            return intervals.Any(x => IsSameInterval(x, intervalToCheck));
        }
    }
}