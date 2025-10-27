namespace CyclingTrainer.SessionAnalyzer.Models
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