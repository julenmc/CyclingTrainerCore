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

        private IntervalContainer _intervalContainer;
        private FitnessDataContainer _fitnessDataContainer;

        internal class MergeExpection : Exception
        {
            internal MergeExpection(string message) : base(message) { }
        }

        internal IntervalsRefiner(IntervalContainer intervalContainer, FitnessDataContainer fitnessDataContainer)
        {
            _intervalContainer = intervalContainer;
            _fitnessDataContainer = fitnessDataContainer;
        }

        /// <summary>
        /// Refines the intervals from the given container <see cref="IntervalContainer"/>
        /// </summary>
        /// <remarks>
        /// This method will handle the collisions between the intervals of the container and will integrate 
        /// sub-intervals in the interval that they belong
        /// </remarks>
        internal void Refine()
        {
            HandleCollisions();
            HandleIntegrations(_intervalContainer.Intervals);
        }

        private void HandleCollisions()
        {
            // It's crucial to sort the intervals in choronological order for the DoIntervalsCollide method
            _intervalContainer.Intervals.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));

            for (int i = 0; i < _intervalContainer.Intervals.Count; i++)
            {
                _intervalContainer.Intervals[i].Intervals = new List<Interval>();
                for (int j = i + 1; j < _intervalContainer.Intervals.Count; j++)
                {
                    if (DoIntervalsCollide(_intervalContainer.Intervals[i], _intervalContainer.Intervals[j]))
                    {
                        Log.Debug($"Collision between:" +
                                  $"interval {_intervalContainer.Intervals[i].StartTime.TimeOfDay}-{_intervalContainer.Intervals[i].EndTime.TimeOfDay} ({_intervalContainer.Intervals[i].TimeDiff} s) at {_intervalContainer.Intervals[i].AveragePower} W " +
                                  $"and interval {_intervalContainer.Intervals[j].StartTime.TimeOfDay}-{_intervalContainer.Intervals[j].EndTime.TimeOfDay} ({_intervalContainer.Intervals[j].TimeDiff} s) at {_intervalContainer.Intervals[j].AveragePower} W");

                        try
                        {
                            Interval merged = MergeIntervals(_intervalContainer.Intervals[i], _intervalContainer.Intervals[j]);
                            // Remove the interval with lower power and replace it with the merged one
                            if (_intervalContainer.Intervals[i].AveragePower > _intervalContainer.Intervals[j].AveragePower) _intervalContainer.Intervals[j] = merged;
                            else _intervalContainer.Intervals[i] = merged;
                            // _intervalContainer.Intervals.Insert(j, merged); // Inserted before the second interval to mantain the chronological order
                            // j++;                                            // Merged interval is inserted before the second, so j must be incremented so it doesn't compare the same intervals 
                        }
                        catch (MergeExpection ex)
                        {
                            Log.Debug($"Intervals can't be merged because: {ex.Message}");
                            // Trim interval with lower priority
                            if (_intervalContainer.Intervals[i].AveragePower < _intervalContainer.Intervals[j].AveragePower)
                            {
                                DateTime newEndTime = _intervalContainer.Intervals[j].StartTime.AddSeconds(-1);
                                _intervalContainer.Intervals[i] = GenerateInterval(_intervalContainer.Intervals[i].StartTime, newEndTime);
                                Log.Debug($"Interval has been shortened to: {_intervalContainer.Intervals[i].StartTime.TimeOfDay}-{_intervalContainer.Intervals[i].EndTime.TimeOfDay} ({_intervalContainer.Intervals[i].TimeDiff} s) at {_intervalContainer.Intervals[i].AveragePower} W");
                            }
                            else
                            {
                                DateTime newStartTime = _intervalContainer.Intervals[i].EndTime.AddSeconds(1);
                                _intervalContainer.Intervals[j] = GenerateInterval(newStartTime, _intervalContainer.Intervals[j].EndTime);
                                Log.Debug($"Interval has been shortened to: {_intervalContainer.Intervals[j].StartTime.TimeOfDay}-{_intervalContainer.Intervals[j].EndTime.TimeOfDay} ({_intervalContainer.Intervals[j].TimeDiff} s) at {_intervalContainer.Intervals[j].AveragePower} W");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Merges two intervals
        /// </summary>
        /// <remarks>
        /// Attention: one interval can't be the others sub-interval. Intervals have to be in chronological order
        /// </remarks>
        private Interval MergeIntervals(Interval interval1, Interval interval2)
        {
            // Initial check if can be merged
            float maRelThr = interval1.TimeDiff switch
            {
                >= IntervalTimes.LongIntervalMinTime => IntervalSearchValues.LongIntervals.Default.MaRel,
                >= IntervalTimes.MediumIntervalMinTime => IntervalSearchValues.MediumIntervals.Default.MaRel,
                _ => IntervalSearchValues.ShortIntervals.Default.MaRel
            };
            float allowedDeviation = interval1.AveragePower * maRelThr;
            if (Math.Abs(interval1.AveragePower - interval2.AveragePower) > allowedDeviation)
            {
                throw new MergeExpection($"Average deviation is too high: {interval2.AveragePower} vs {interval1.AveragePower}, where allowed is {allowedDeviation}");
            }

            // Once checked proceed with merge
            DateTime startTime = interval1.StartTime;
            DateTime endTime = interval2.EndTime;
            Interval merged = GenerateInterval(startTime, endTime);
            Log.Debug($"Interval has been extended to: {merged.StartTime.TimeOfDay}-{merged.EndTime.TimeOfDay} ({merged.TimeDiff} s) at {merged.AveragePower} W");

            return merged;
        }

        private Interval GenerateInterval(DateTime startTime, DateTime endTime)
        {
            var remainingPoints = _fitnessDataContainer.FitnessData;
            var points = remainingPoints
                .Where(p =>
                {
                    var timestamp = p.Timestamp.GetDateTime();
                    return timestamp >= startTime && timestamp <= endTime;
                })
                .ToList();

            Interval interval = new Interval
            {
                StartTime = startTime,
                EndTime = endTime,
                TimeDiff = (int)(endTime - startTime).TotalSeconds + 1,
                AveragePower = (float)points.Average(p => p.Stats.Power ?? 0),
            };
            interval.Intervals = new List<Interval>();

            return interval;
        }

        // It's crucial to sort the intervals in choronological order so this method works
        private static bool DoIntervalsCollide(Interval interval1, Interval interval2)
        {
            if (interval1.StartTime == interval2.StartTime) return false;
            return interval2.StartTime < interval1.EndTime && interval2.EndTime > interval1.EndTime;
        }
        
        private static void HandleIntegrations(List<Interval> intervals)
        {
            void HandleSubInterval(Interval parent, Interval child)
            {
                string info = "";
                if (child.AveragePower < parent.AveragePower)
                    info = "LessPower";
                else
                {
                    info = "SubInterval";
                    parent.Intervals.Add(child);
                }
                Log.Debug($"{info} between: parent interval {parent.StartTime.TimeOfDay}-{parent.EndTime.TimeOfDay} ({parent.TimeDiff} s) at {parent.AveragePower} W " +
                            $"and child interval {child.StartTime.TimeOfDay}-{child.EndTime.TimeOfDay} ({child.TimeDiff} s) at {child.AveragePower} W");
            }
            
            for (int i = 0; i < intervals.Count; i++)
            {
                for (int j = i + 1; j < intervals.Count;)
                {
                    if (intervals[i].IsSubInterval(intervals[j]))
                    {
                        HandleSubInterval(intervals[i], intervals[j]);
                        intervals.RemoveAt(j);
                    }
                    else if (intervals[j].IsSubInterval(intervals[i]))
                    {
                        HandleSubInterval(intervals[j], intervals[i]);
                        intervals.RemoveAt(i);
                    }
                    else
                        j++;
                }
            }

            Log.Debug($"{intervals.Count} intervals after integration");

            foreach (Interval interval in intervals)
            {
                if (interval.Intervals.Count != 0)
                {
                    Log.Debug($"Integration of interval at {interval.StartTime.TimeOfDay} ({interval.TimeDiff}s) to be started...");
                    List<Interval> aux = interval.Intervals;    // IDK why
                    HandleIntegrations(aux);
                    interval.Intervals = aux;
                }
            }
        }
    }
}