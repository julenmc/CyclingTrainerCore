using CyclingTrainer.SessionAnalyzer.Enums;

namespace CyclingTrainer.SessionAnalyzer.Constants
{
    public static class IntervalTimes
    {
        public const ushort IntervalMinTime = 30;      // 30 seconds
        internal const ushort MediumIntervalMinTime = 240;      // Less than 4 minutes is considered a short interval
        internal const ushort LongIntervalMinTime = 600;        // Less than 10 minutes is considered a medium interval

        internal static readonly Dictionary<IntervalGroups, int> IntervalMinTimes = new Dictionary<IntervalGroups, int>
        {
            {IntervalGroups.Short, IntervalMinTime},            // Short intervals must be at least of 30 seconds
            {IntervalGroups.Medium, MediumIntervalMinTime},     // Medium intervals must be at least of 4 minutes
            {IntervalGroups.Long, LongIntervalMinTime}          // Long intervals must be at least of 10 minutes
        };
    }
}