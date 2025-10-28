using CyclingTrainer.SessionAnalyzer.Enums;

namespace CyclingTrainer.SessionAnalyzer.Constants
{
    public static class IntervalZones
    {
        internal static readonly Dictionary<IntervalGroups, ushort> IntervalMinZones = new Dictionary<IntervalGroups, ushort>
        {
            {IntervalGroups.Short, 5},       // Short intervals must be at least at Z5
            {IntervalGroups.Medium, 4},      // Medium intervals must be at least at Z4
            {IntervalGroups.Long, 3}         // Long intervals must be at least at Z3
        };

        internal static readonly Dictionary<IntervalSeachGroups, ushort> SearchRequiredZones = new Dictionary<IntervalSeachGroups, ushort>
        {
            {IntervalSeachGroups.Short, 5},       
            {IntervalSeachGroups.Medium, 4},      
            {IntervalSeachGroups.Long, 3}         
        };
    }
}