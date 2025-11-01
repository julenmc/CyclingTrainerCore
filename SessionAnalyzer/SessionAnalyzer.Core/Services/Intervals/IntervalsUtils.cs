using CoreModels = CyclingTrainer.Core.Models;
using CyclingTrainer.SessionAnalyzer.Constants;
using CyclingTrainer.SessionAnalyzer.Models;
using CyclingTrainer.SessionAnalyzer.Enums;

namespace CyclingTrainer.SessionAnalyzer.Services.Intervals
{
    internal static class IntervalsUtils
    {
        internal static bool IsConsideredAnInterval(Interval interval, List<CoreModels.Zone> powerZones)
        {
            // Obtain expected minimum zones
            var shortZone = powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Short]);
            if (shortZone == null)
                throw new ArgumentException($"Couldn't find minimum zone for '{IntervalGroups.Short}' (Id = {IntervalZones.IntervalMinZones[IntervalGroups.Short]}).");

            var mediumZone = powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Medium]);
            if (mediumZone == null)
                throw new ArgumentException($"Couldn't find minimum zone for '{IntervalGroups.Medium}' (Id = {IntervalZones.IntervalMinZones[IntervalGroups.Medium]}).");

            var longZone = powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Long]);
            if (longZone == null)
                throw new ArgumentException($"Couldn't find minimum zone for '{IntervalGroups.Long}' (Id = {IntervalZones.IntervalMinZones[IntervalGroups.Long]}).");

            double shortLimit = shortZone.LowLimit;
            double mediumLimit = mediumZone.LowLimit;
            double longLimit = longZone.LowLimit;

            return interval.TimeDiff switch
            {
                >= IntervalTimes.LongIntervalMinTime => interval.AveragePower >= longLimit,
                >= IntervalTimes.MediumIntervalMinTime => interval.AveragePower >= mediumLimit,
                >= IntervalTimes.IntervalMinTime => interval.AveragePower >= shortLimit,
                _ => false
            };
        }

        internal static bool AreEqual(Interval interval1, Interval interval2)
        {
            return (interval1.StartTime == interval2.StartTime && interval1.EndTime == interval2.EndTime);
        }
    }
}