using CoreModels = CyclingTrainer.Core.Models;
using CyclingTrainer.SessionAnalyzer.Constants;
using CyclingTrainer.SessionAnalyzer.Models;
using CyclingTrainer.SessionAnalyzer.Enums;

namespace CyclingTrainer.SessionAnalyzer.Services.Intervals
{
    internal static class IntervalsUtils
    {
        internal static bool IsConsideredAnInterval(Interval interval, List<CoreModels.Zone> powerZones) =>
            interval.TimeDiff switch
            {
                >= IntervalTimes.LongIntervalMinTime =>
                    interval.AveragePower > (powerZones
                        .Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Long])?.LowLimit ?? 0),
                >= IntervalTimes.MediumIntervalMinTime =>
                    interval.AveragePower > (powerZones
                        .Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Medium])?.LowLimit ?? 0),
                >= IntervalTimes.IntervalMinTime =>
                    interval.AveragePower > (powerZones
                        .Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Short])?.LowLimit ?? 0),
                _ => false
            };

        internal static bool AreEqual(Interval parent, Interval child)
        {
            return (parent.StartTime == child.StartTime && parent.EndTime == child.EndTime);
        }
    }
}