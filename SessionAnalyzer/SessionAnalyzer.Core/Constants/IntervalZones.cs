using CyclingTrainer.SessionAnalyzer.Core.Enums;

namespace CyclingTrainer.SessionAnalyzer.Core.Constants
{
    public static class IntervalZones
    {
        internal static readonly Dictionary<IntervalGroups, ushort> IntervalMinZones = new Dictionary<IntervalGroups, ushort>
        {
            {IntervalGroups.Short, 5},       // Short intervals must be at least at Z5
            {IntervalGroups.Medium, 4},      // Medium intervals must be at least at Z4
            {IntervalGroups.Long, 3}         // Long intervals must be at least at Z3
        };
    }
}