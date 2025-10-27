using CoreModels = CyclingTrainer.Core.Models;
using CyclingTrainer.SessionReader.Models;
using CyclingTrainer.SessionAnalyzer.Constants;
using CyclingTrainer.SessionAnalyzer.Models;
using NLog;

namespace CyclingTrainer.SessionAnalyzer.Services.Intervals
{
    internal class IntervalsRefiner
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private List<CoreModels.Zone> _powerZones;
        private FitnessDataContainer _container;
        private Thresholds? _thresholds;

        internal IntervalsRefiner(FitnessDataContainer container, List<CoreModels.Zone> powerZones, Thresholds? thresholds = null)
        {
            _container = container;
            _powerZones = powerZones;
            _thresholds = thresholds;
        }

        internal void Refine(List<Interval> intervals)
        {
            Log.Debug($"Checking relations of {intervals.Count} intervals...");

            intervals.Sort((a, b) => b.TimeDiff.CompareTo(a.TimeDiff));

            for (int i = 0; i < intervals.Count; i++)
            {
                var longInterval = intervals[i];
                longInterval.Intervals = new List<Interval>();
                for (int j = i + 1; j < intervals.Count;)
                {
                    var shortInterval = intervals[j];

                    var result = CheckTwoIntervalsRelation(ref longInterval, ref shortInterval);
                    intervals[i] = longInterval;
                    if (result)
                        intervals.RemoveAt(j);
                    else
                        j++;
                }
            }

            Log.Debug($"{intervals.Count} intervals after integration");

            foreach (Interval interval in intervals)
            {
                if (interval.Intervals != null && interval.Intervals.Count != 0)
                {
                    Log.Debug($"Integration of interval at {interval.StartTime.TimeOfDay} ({interval.TimeDiff}s) to be started...");
                    List<Interval> aux = interval.Intervals;    // IDK why
                    this.Refine(aux);
                    interval.Intervals = aux;
                }
            }

            intervals.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
        }

        private bool CheckTwoIntervalsRelation(ref Interval longInterval, ref Interval shortInterval)
        {
            bool delete = false;
            string info = "";

            if (IsSubInterval(longInterval, shortInterval))
            {
                delete = true;
                if (IntervalsUtils.AreEqual(longInterval, shortInterval))
                    info = "SameInterval";
                else if (shortInterval.AveragePower < longInterval.AveragePower)
                    info = "LessPower";
                else
                {
                    info = "SubInterval";
                    longInterval.Intervals?.Add(shortInterval);
                }
                Log.Debug($"{info} between: interval {longInterval.StartTime.TimeOfDay}-{longInterval.EndTime.TimeOfDay} ({longInterval.TimeDiff} s) at {longInterval.AveragePower} W and interval {shortInterval.StartTime.TimeOfDay}-{shortInterval.EndTime.TimeOfDay} ({shortInterval.TimeDiff} s) at {shortInterval.AveragePower} W");
            }
            else if (DoIntervalsCollide(longInterval, shortInterval))
            {
                Log.Debug($"Collision between: interval {longInterval.StartTime.TimeOfDay}-{longInterval.EndTime.TimeOfDay} ({longInterval.TimeDiff} s) at {longInterval.AveragePower} W and interval {shortInterval.StartTime.TimeOfDay}-{shortInterval.EndTime.TimeOfDay} ({shortInterval.TimeDiff} s) at {shortInterval.AveragePower} W");

                // Check if intervals can be merged
                Interval? merged = null;
                try
                {
                    merged = MergeIntervals(longInterval, shortInterval);
                }
                catch
                {
                    Log.Debug($"Intervals can't be merged because their average power delta is too high");
                }

                if (merged != null)
                {
                    longInterval = merged;
                    delete = true;
                    Log.Debug($"Interval has been extended to: {longInterval.StartTime.TimeOfDay}-{longInterval.EndTime.TimeOfDay} ({longInterval.TimeDiff} s) at {longInterval.AveragePower} W");
                }
                else if (shortInterval.StartTime < longInterval.StartTime)
                {
                    delete = HandleCollisionAtStart(longInterval, ref shortInterval);
                }
                else
                {
                    delete = HandleCollisionAtEnd(longInterval, ref shortInterval);
                }
            }

            return delete;
        }

        private Interval MergeIntervals(Interval longInterval, Interval shortInterval)
        {
            // Initial check if can be merged
            float defaultMaRel = longInterval.TimeDiff switch
            {
                >= IntervalTimes.LongIntervalMinTime => IntervalSearchValues.LongIntervals.Default.MaRel,
                >= IntervalTimes.MediumIntervalMinTime => IntervalSearchValues.MediumIntervals.Default.MaRel,
                _ => IntervalSearchValues.ShortIntervals.Default.MaRel
            };
            float maRelThr = _thresholds != null ? _thresholds.MaRel : defaultMaRel;
            float allowedDeviation = longInterval.AveragePower * maRelThr;
            if (Math.Abs(longInterval.AveragePower - shortInterval.AveragePower) > allowedDeviation)
            {
                throw new Exception("Deviation too high");
            }

            // Once check proceed with merge
            DateTime startTime = longInterval.StartTime;
            DateTime endTime = longInterval.EndTime;
            if (longInterval.StartTime > shortInterval.StartTime)
            {
                // Collision before start
                startTime = shortInterval.StartTime;
            }
            else
            {
                // Collision after end
                endTime = shortInterval.EndTime;
            }
            var remainingPoints = _container.FitnessData;
            var points = remainingPoints
                .Where(p =>
                {
                    var timestamp = p.Timestamp.GetDateTime();
                    return timestamp >= startTime && timestamp <= endTime;
                })
                .ToList();

            if (!points.Any())
            {
                return longInterval;    // Should not be here. If so, return the big interval and continue
            }

            Interval merged = new Interval
            {
                StartTime = startTime,
                EndTime = endTime,
                TimeDiff = (int)(endTime - startTime).TotalSeconds + 1,
                AveragePower = (float)points.Average(p => p.Stats.Power ?? 0),
                Intervals = longInterval.Intervals
            };

            // Add short interval as sub-interval
            merged.Intervals?.Add(shortInterval);
            return merged;
        }

        private bool HandleCollisionAtStart(Interval longInterval, ref Interval shortInterval)
        {
            bool delete = false;

            Interval oldChild = new Interval
            {
                StartTime = shortInterval.StartTime,
                EndTime = shortInterval.EndTime
            };
            GenerateNewIntervalBeforeCollitionInStart(longInterval, ref shortInterval);  // Pass reference before collision cuz might still be an interval

            if (!IntervalsUtils.IsConsideredAnInterval(shortInterval, _powerZones))
            {
                delete = true;
                Log.Debug($"Interval will not be saved after modification");
            }
            else
            {
                Log.Debug($"Interval will be saved after modification");
            }

            Interval newChild = GenerateNewIntervalAfterCollitionInStart(longInterval, oldChild);       // After collision should be a new interval, no reference
            if (IntervalsUtils.IsConsideredAnInterval(newChild, _powerZones))   // TODO: check if child's power is lower than parent's
            {
                longInterval.Intervals?.Add(newChild);
                Log.Debug($"New subInterval added: startTime {newChild.StartTime.TimeOfDay} with {newChild.AveragePower} W in {newChild.TimeDiff} s");
            }
            return delete;
        }

        private bool HandleCollisionAtEnd(Interval longInterval, ref Interval shortInterval)
        {
            bool delete = false;
            Interval oldChild = new Interval
            {
                StartTime = shortInterval.StartTime,
                EndTime = shortInterval.EndTime
            };
            Interval newChild = GenerateNewIntervalBeforeCollitionInEnd(longInterval, oldChild);  // Before collision should be a new interval, no reference

            if (IntervalsUtils.IsConsideredAnInterval(newChild, _powerZones))   // TODO: check if child's power is lower than parent's
            {
                longInterval.Intervals?.Add(newChild);
                Log.Debug($"New subInterval added: startTime {newChild.StartTime.TimeOfDay} with {newChild.AveragePower} W in {newChild.TimeDiff} s");
            }

            GenerateNewIntervalAfterCollitionInEnd(longInterval, ref shortInterval);       // Pass reference after collision cuz might still be an interval
            if (!IntervalsUtils.IsConsideredAnInterval(shortInterval, _powerZones))
            {
                delete = true;
                Log.Debug($"Interval will not be saved after modification");
            }
            else
            {
                Log.Debug($"Interval will be saved after modification");
            }
            return delete;
        }

        private void GenerateNewIntervalBeforeCollitionInStart(Interval parent, ref Interval child)
        {
            var newEndTime = parent.StartTime.AddSeconds(-1);
            var startTime = child.StartTime;  // Guardar en variable local para usar en lambda

            var remainingPoints = _container.FitnessData;
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

        private Interval GenerateNewIntervalAfterCollitionInStart(Interval parent, Interval child)
        {
            var endTime = child.EndTime;         // Save in variable to use in lambda
            var newStartTime = parent.StartTime;

            Interval ret = new Interval{
                StartTime = child.StartTime,
                EndTime = child.EndTime
            };

            var remainingPoints = _container.FitnessData;
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

        private Interval GenerateNewIntervalBeforeCollitionInEnd(Interval parent, Interval child)
        {
            var newEndTime = parent.EndTime;
            var startTime = child.StartTime;  // Guardar en variable local para usar en lambda

            Interval ret = new Interval{
                StartTime = child.StartTime,
                EndTime = child.EndTime
            };
            var remainingPoints = _container.FitnessData;
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

        private void GenerateNewIntervalAfterCollitionInEnd(Interval parent, ref Interval child)
        {
            var endTime = child.EndTime;         // Save in variable to use in lambda
            var newStartTime = parent.EndTime.AddSeconds(1);

            Interval ret = child;

            var remainingPoints = _container.FitnessData;
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

        private static bool DoIntervalsCollide(Interval parent, Interval child)
        {
            return (child.StartTime < parent.StartTime && child.EndTime > parent.StartTime) ||
                   (child.StartTime < parent.EndTime && child.EndTime > parent.EndTime);
        }
    }
}