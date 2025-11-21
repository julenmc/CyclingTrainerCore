using CyclingTrainer.SessionAnalyzer.Enums;

namespace CyclingTrainer.SessionAnalyzer.Constants
{
    public static class IntervalZones
    {
        internal static readonly Dictionary<IntervalGroups, ushort> IntervalMinZones = new Dictionary<IntervalGroups, ushort>
        {
            {IntervalGroups.Short, 5},       // Short intervals must be at least at Z5
            {IntervalGroups.Medium, 3},      // Medium intervals must be at least at Z3
            {IntervalGroups.Long, 2}         // Long intervals must be at least at Z2
        };

        internal static readonly Dictionary<IntervalSeachGroups, ushort> SearchRequiredZones = new Dictionary<IntervalSeachGroups, ushort>
        {
            {IntervalSeachGroups.Short, 5},       
            {IntervalSeachGroups.Medium, 3},      
            {IntervalSeachGroups.Long, 2}         
        };
    }
}