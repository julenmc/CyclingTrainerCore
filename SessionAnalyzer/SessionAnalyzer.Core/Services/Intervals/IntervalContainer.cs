using CyclingTrainer.SessionAnalyzer.Models;

namespace CyclingTrainer.SessionAnalyzer.Services.Intervals
{
    internal class IntervalContainer
    {
        internal List<Interval> Sprints { get; set; } = new();
        internal List<Interval> Intervals { get; set; } = new();

        internal bool IsTheGapASprint(DateTime time)
        {
            return Sprints.Find(x => x.EndTime == time) != null;
        }
    }
}