using CyclingTrainer.SessionAnalyzer.Core.Enums;

namespace CyclingTrainer.SessionAnalyzer.Core.Constants
{
    public static class IntervalTimes
    {
        public const ushort IntervalMinTime = 30;      // 30 seconds
        public const ushort IntervalMaxTime = 3600;    // 1 hour
        public const ushort MaxTimeBeforeEnd = 5;       // 5 seconds out of the requirements before it ends the interval

        internal const ushort MediumIntervalMinTime = 240;      // Less than 4 minutes is considered a short interval
        internal const ushort LongIntervalMinTime = 6000;       // Less than 10 minutes is considered a medium interval

        internal static readonly Dictionary<IntervalGroups, ushort> IntervalMinTimes = new Dictionary<IntervalGroups, ushort>
        {
            {IntervalGroups.Short, 4},       // Short intervals must be at least at Z4
            {IntervalGroups.Medium, 3},      // Medium intervals must be at least at Z3
            {IntervalGroups.Long, 2}         // Long intervals must be at least at Z2
        };
    }
}