using CoreModels = CyclingTrainer.Core.Models;
using CyclingTrainer.SessionAnalyzer.Constants;
using CyclingTrainer.SessionAnalyzer.Enums;
using CyclingTrainer.SessionAnalyzer.Models;
using CyclingTrainer.SessionReader.Models;
using NLog;

namespace CyclingTrainer.SessionAnalyzer.Services.Intervals
{
    internal class IntervalsFinder
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private List<CoreModels.Zone> _powerZones;
        private FitnessDataContainer _fitnessDataContainer;
        private IntervalContainer _intervalContainer;
        private Thresholds? _thresholds;
        private int _windowSize;
        CoreModels.Zone _allowedPowerZone;

        internal IntervalsFinder(FitnessDataContainer fitnessDataContainer,
                                 IntervalContainer intervalContainer,
                                 List<CoreModels.Zone> powerZones,
                                 int windowSize,
                                 CoreModels.Zone allowedPowerZone,
                                 Thresholds? thresholds = null)
        {
            _fitnessDataContainer = fitnessDataContainer;
            _intervalContainer = intervalContainer;
            _powerZones = powerZones;
            _thresholds = thresholds;
            _windowSize = windowSize;
            _allowedPowerZone = allowedPowerZone;
        }

        internal List<Interval> Search()
        {
            Thresholds defaultThresholds = _windowSize switch
            {
                IntervalTimes.ShortWindowSize => IntervalSearchValues.ShortIntervals.Default,
                IntervalTimes.MediumWindowSize => IntervalSearchValues.MediumIntervals.Default,
                IntervalTimes.LongWindowSize => IntervalSearchValues.LongIntervals.Default,
                _ => throw new Exception("Window size not accepted"),
            };
            // Inicializar con valores por defecto si no se proporcionan
            float cvStartThr = _thresholds != null ? _thresholds.CvStart : defaultThresholds.CvStart;
            float cvFollowThr = _thresholds != null ? _thresholds.CvFollow : defaultThresholds.CvFollow;
            float rangeThr = _thresholds != null ? _thresholds.Range : defaultThresholds.Range;
            float maRelThr = _thresholds != null ? _thresholds.MaRel : defaultThresholds.MaRel;

            Log.Debug($"Using thresholds: cvStart={cvStartThr}, cvFollow={cvFollowThr}, range={rangeThr}, maRel={maRelThr}. minPower={_allowedPowerZone.LowLimit}W, maxPower={_allowedPowerZone.HighLimit}W");

            // Obtener los datos restantes después de eliminar sprints
            var remainingPoints = _fitnessDataContainer.FitnessData;

            // Calcular medias móviles para diferentes ventanas de tiempo
            Log.Info("Calculating moving averages...");
            var powerModels = AveragePowerCalculator.CalculateMovingAverages(remainingPoints, _windowSize, _intervalContainer);
            Log.Debug($"Generated {powerModels.Count} power models");
            var intervals = new List<Interval>();

            // Buscar intervalos
            int i = 0;
            while (i < powerModels.Count)
            {
                // Buscar inicio de intervalo potencial
                while (i < powerModels.Count && !IsIntervalStart(powerModels[i], cvStartThr, rangeThr, _allowedPowerZone.HighLimit))
                    i++;

                if (i >= powerModels.Count)
                    break;

                var startTime = powerModels[i].PointDate;
                float referenceAverage = powerModels[i].AvrgPower;
                int totalPower = 0;
                int pointCount = 0;
                bool sessionStopped = false;
                Log.Debug($"New interval might start at: startDate={startTime.TimeOfDay}: CV={powerModels[i].CoefficientOfVariation}. Range={powerModels[i].RangePercent}");

                // Seguir el intervalo mientras se mantenga estable
                int unstableCount = 0;
                while (i < powerModels.Count)
                {
                    int timeDiff = (i > 0) ? (int)(powerModels[i].PointDate - powerModels[i - 1].PointDate).TotalSeconds : 1;
                    if (timeDiff > 1 && !_intervalContainer.IsTheGapASprint(powerModels[i].PointDate))
                    {
                        Log.Debug($"Session stopped at {powerModels[i - 1].PointDate.TimeOfDay} for {timeDiff - _windowSize} seconds. Finishing interval");
                        sessionStopped = true;
                        break;
                    }
                    var current = powerModels[i];
                    current.DeviationFromReference = Math.Abs(current.AvrgPower - referenceAverage) / referenceAverage;

                    if (!IsIntervalContinuation(current, cvFollowThr, maRelThr))
                    {
                        if (unstableCount == 0) Log.Debug($"Unstable point found at {powerModels[i].PointDate.TimeOfDay} ({i}): CV={current.CoefficientOfVariation}, Deviation={current.DeviationFromReference}");
                        unstableCount++;
                        if (unstableCount >= _windowSize)
                            break;
                        pointCount++;
                        totalPower += (int)current.AvrgPower;
                    }
                    else
                    {
                        if (unstableCount != 0)
                        {
                            Log.Debug($"Unstable point ends at {powerModels[i].PointDate.TimeOfDay} ({i}): CV={current.CoefficientOfVariation}, Deviation={current.DeviationFromReference}. Count: {unstableCount}");
                            unstableCount = 0;
                        }
                        pointCount++;
                        totalPower += (int)current.AvrgPower;
                        referenceAverage = (float)totalPower / pointCount;
                    }

                    i++;
                }

                int auxIndex = i >= powerModels.Count ? i - 1 : i;
                if (sessionStopped)
                {
                    auxIndex--;
                    i++;
                }
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
                RefineIntervalLimits(newInterval, remainingPoints);
                Log.Debug($"Found interval: Time={newInterval.StartTime.TimeOfDay}-{newInterval.EndTime.TimeOfDay} ({newInterval.TimeDiff}s), avgPower={newInterval.AveragePower}W");

                IntervalGroups group = _windowSize switch
                {
                    IntervalTimes.ShortWindowSize => IntervalGroups.Short,
                    IntervalTimes.MediumWindowSize => IntervalGroups.Medium,
                    IntervalTimes.LongWindowSize => IntervalGroups.Long,
                    _ => throw new Exception($"Unknown window size ({_windowSize})")
                };
                if (newInterval.TimeDiff >= IntervalTimes.IntervalMinTimes[group] && IntervalsUtils.IsConsideredAnInterval(newInterval, _powerZones))
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

        private void RefineIntervalLimits(Interval interval, List<FitnessData> points)
        {
            // Encontrar el índice del punto inicial y final del intervalo en la lista completa
            int intervalStartIdx = points.FindIndex(p => p.Timestamp.GetDateTime() >= interval.StartTime);
            int intervalEndIdx = points.FindLastIndex(p => p.Timestamp.GetDateTime() <= interval.EndTime);

            if (intervalStartIdx == -1 || intervalEndIdx == -1 || intervalStartIdx > intervalEndIdx)
            {
                Log.Warn("Invalid interval indices found during refinement");
                return;
            }

            // Expandir el rango para incluir puntos contiguos
            int extraPoints = interval.TimeDiff switch
            {
                >= IntervalTimes.LongIntervalMinTime => Math.Max(IntervalTimes.LongWindowSize, _windowSize) * 3,
                >= IntervalTimes.MediumIntervalMinTime => Math.Max(IntervalTimes.MediumWindowSize, _windowSize) * 3,
                _ => Math.Max(IntervalTimes.ShortWindowSize, _windowSize) * 3
            };
            int expandedStartIdx = Math.Max(0, intervalStartIdx - extraPoints);
            int expandedEndIdx = Math.Min(points.Count - 1, intervalEndIdx + extraPoints);

            // Check if session has been stopped during the expansion
            var expandedStartPoints = points.GetRange(expandedStartIdx, intervalStartIdx - expandedStartIdx + 1);
            int auxIndex = 1;
            while (auxIndex < expandedStartPoints.Count)
            {
                if ((int)(expandedStartPoints[auxIndex].Timestamp.GetDateTime() - expandedStartPoints[auxIndex - 1].Timestamp.GetDateTime()).TotalSeconds > 1 && 
                    !_intervalContainer.IsTheGapASprint(expandedStartPoints[auxIndex].Timestamp.GetDateTime()))
                {
                    expandedStartIdx = intervalStartIdx - (expandedStartPoints.Count - auxIndex) + 1;
                    expandedStartPoints = points.GetRange(expandedStartIdx, intervalStartIdx - expandedStartIdx + 1);
                    auxIndex = 1;
                }
                else
                {
                    auxIndex++;
                }
            }

            var expandedEndPoints = points.GetRange(intervalEndIdx, expandedEndIdx - intervalEndIdx + 1);
            auxIndex = 1;
            while (auxIndex < expandedEndPoints.Count)
            {
                if ((int)(expandedEndPoints[auxIndex].Timestamp.GetDateTime() - expandedEndPoints[auxIndex - 1].Timestamp.GetDateTime()).TotalSeconds > 1 && 
                    !_intervalContainer.IsTheGapASprint(expandedEndPoints[auxIndex].Timestamp.GetDateTime()))
                {
                    expandedEndIdx = auxIndex + intervalEndIdx - 1;
                    expandedEndPoints = points.GetRange(intervalEndIdx, expandedEndIdx - intervalEndIdx + 1);
                    auxIndex = 1;
                }
                else
                    auxIndex++;
            }

            var expandedPoints = points.GetRange(expandedStartIdx, expandedEndIdx - expandedStartIdx + 1);

            if (!expandedPoints.Any())
            {
                Log.Warn("No points found for interval refinement");
                return;
            }

            float targetPower = interval.AveragePower;
            float defaultMaRel = interval.TimeDiff switch
            {
                >= IntervalTimes.LongIntervalMinTime => IntervalSearchValues.LongIntervals.Default.MaRel,
                >= IntervalTimes.MediumIntervalMinTime => IntervalSearchValues.MediumIntervals.Default.MaRel,
                _ => IntervalSearchValues.ShortIntervals.Default.MaRel
            };
            float maRelThr = _thresholds != null ? _thresholds.MaRel : defaultMaRel;
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
    }
}