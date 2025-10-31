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
        private Thresholds _thresholds;
        IntervalSeachGroups _searchGroup;
        private int _windowSize;
        private int _intervalStartMinPower;
        CoreModels.Zone _startThresholdPowerZone;

        internal IntervalsFinder(FitnessDataContainer fitnessDataContainer,
                                 IntervalContainer intervalContainer,
                                 List<CoreModels.Zone> powerZones,
                                 IntervalSeachGroups searchGroup,
                                 Thresholds? thresholds = null)
        {
            _fitnessDataContainer = fitnessDataContainer;
            _intervalContainer = intervalContainer;
            _powerZones = powerZones;
            _searchGroup = searchGroup;
            _windowSize = IntervalTimes.IntervalSearchWindows[_searchGroup];
            CoreModels.Zone? highZone = powerZones.Find(x => x.Id == IntervalZones.SearchRequiredZones[_searchGroup] + 2);
            CoreModels.Zone? lowZone = powerZones.Find(x => x.Id == IntervalZones.SearchRequiredZones[_searchGroup]);
            if (highZone == null || lowZone == null)
            {
                throw new Exception("Invalid power zones");
            }
            _startThresholdPowerZone = new CoreModels.Zone
            {
                HighLimit = highZone.HighLimit,
                LowLimit = lowZone.LowLimit
            };
            _intervalStartMinPower = (lowZone.HighLimit + lowZone.LowLimit) / 2;
            if (thresholds != null)
            {
                _thresholds = thresholds;
            }
            else
            {
                _thresholds = _searchGroup switch
                {
                    IntervalSeachGroups.Short => IntervalSearchValues.ShortIntervals.Default,
                    IntervalSeachGroups.Medium => IntervalSearchValues.MediumIntervals.Default,
                    _ => IntervalSearchValues.LongIntervals.Default,    // Long. If Long is writte, it forces to write "_" (default) case
                };
            }
        }

        internal int Search()
        {
            int intervalCount = 0;
            // Inicializar con valores por defecto si no se proporcionan
            float cvStartThr = _thresholds.CvStart;
            float cvFollowThr = _thresholds.CvFollow;
            float rangeThr = _thresholds.Range;
            float maRelThr = _thresholds.MaRel;

            int minStartPower = _intervalStartMinPower;
            int maxStartPower = _startThresholdPowerZone.HighLimit;

            Log.Debug($"Using thresholds: cvStart={cvStartThr}, cvFollow={cvFollowThr}, range={rangeThr}, maRel={maRelThr}. minStartPower={minStartPower} maxStartPower={maxStartPower}W");

            // Obtener los datos restantes después de eliminar sprints
            var remainingPoints = _fitnessDataContainer.FitnessData;

            // Calcular medias móviles para diferentes ventanas de tiempo
            Log.Info("Calculating moving averages...");
            var powerModels = PowerMetricsCalculator.CalculateMovingAverages(remainingPoints, _windowSize, _intervalContainer);
            Log.Debug($"Generated {powerModels.Count} power models");

            // Buscar intervalos
            int i = 0;
            while (i < powerModels.Count)
            {
                // Buscar inicio de intervalo potencial
                while (i < powerModels.Count && !IsIntervalStart(powerModels[i], cvStartThr, rangeThr, maxStartPower, minStartPower))
                    i++;

                if (i >= powerModels.Count)
                    break;

                var startTime = powerModels[i].PointDate;
                float referenceAverage = powerModels[i].AvrgPower;
                int totalPower = 0;
                int pointCount = 0;
                bool sessionStopped = false;
                int firstUnstablePointIndex = 0;
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
                        if (unstableCount == 0)
                        {
                            Log.Debug($"Unstable point found at {powerModels[i].PointDate.TimeOfDay} ({i}): CV={current.CoefficientOfVariation}, Deviation={current.DeviationFromReference}");
                            firstUnstablePointIndex = i;
                        }
                        unstableCount++;
                        if (unstableCount >= _windowSize)
                        {
                            break;
                        }
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
                i = (i < powerModels.Count && !sessionStopped) ? firstUnstablePointIndex + 1 : i;
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

                if (newInterval.TimeDiff >= IntervalTimes.IntervalMinTimes[_searchGroup] && IntervalsUtils.IsConsideredAnInterval(newInterval, _powerZones))
                {
                    if (_intervalContainer.AlreadyExists(newInterval))
                    {
                        Log.Debug($"Interval not saved. Already exists.");
                    }
                    else
                    {
                        Log.Info($"Saved interval: Time={newInterval.StartTime.TimeOfDay}-{newInterval.EndTime.TimeOfDay} ({newInterval.TimeDiff}s), avgPower={newInterval.AveragePower}W");
                        _intervalContainer.Intervals.Add(newInterval);
                        intervalCount++;
                    }
                }
                else Log.Debug($"Interval not saved. Not valid.");
            }
            return intervalCount;
        }

        private bool IsIntervalStart(PowerMetrics point, float cvThreshold, float rangeThreshold, float maxAvgPower, float minAvgPower)
        {
            //Log.Debug($"Checking interval start at {point.PointDate}: CV={point.CoefficientOfVariation}, Range={point.RangePercent}");

            return point.CoefficientOfVariation <= cvThreshold &&
                   point.RangePercent <= rangeThreshold &&
                   point.AvrgPower <= maxAvgPower &&
                   point.AvrgPower >= minAvgPower;
        }

        private bool IsIntervalContinuation(PowerMetrics point, float cvThreshold, float deviationThreshold)
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

            // Expandir el rango para incluir puntos contiguos
            int extraPoints = interval.TimeDiff switch
            {
                >= IntervalTimes.LongIntervalMinTime => Math.Max(IntervalTimes.LongWindowSize, _windowSize),// * 3,
                >= IntervalTimes.MediumIntervalMinTime => Math.Max(IntervalTimes.MediumWindowSize, _windowSize),// * 3,
                _ => Math.Max(IntervalTimes.ShortWindowSize, _windowSize),// * 3
            };
            int expandedStartIdx = Math.Max(0, intervalStartIdx - extraPoints);
            int expandedEndIdx = Math.Min(points.Count - 1, intervalEndIdx + extraPoints);

            var expandedEndPoints = points.GetRange(intervalEndIdx, expandedEndIdx - intervalEndIdx + 1);
            int auxIndex = 1;
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

            float targetPower = interval.AveragePower;
            float defaultMaRel = interval.TimeDiff switch
            {
                >= IntervalTimes.LongIntervalMinTime => IntervalSearchValues.LongIntervals.Default.MaRel,
                >= IntervalTimes.MediumIntervalMinTime => IntervalSearchValues.MediumIntervals.Default.MaRel,
                _ => IntervalSearchValues.ShortIntervals.Default.MaRel
            };
            float maRelThr = _thresholds.MaRel;
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
            while (/*startIndex < expandedPoints.Count - 1 &&*/ // I understand that it can't reach the end of the expandedPoint because it's inside the interval
                   Math.Abs((expandedPoints[startIndex].Stats.Power ?? 0) - targetPower) > allowedDeviation)
            {
                startIndex++;
            }

            // Refinar límite final - buscar hacia adelante desde el punto final
            var endIndex = expandedPoints.FindLastIndex(
                p => p.Timestamp.GetDateTime() <= interval.EndTime);

            // Buscar hacia adelante para encontrar el verdadero final
            // while (endIndex < expandedPoints.Count - 1)
            // {
            //     var nextPower = expandedPoints[endIndex + 1].Stats.Power ?? 0;
            //     if (Math.Abs(nextPower - targetPower) > allowedDeviation)
            //         break;
            //     endIndex++;
            // }

            // Buscar hacia atrás si es necesario
            while (/*endIndex > startIndex &&*/     // I understand that it can't reach the end of the expandedPoint because it's inside the interval
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
            }
        }
    }
}