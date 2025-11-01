using CyclingTrainer.SessionAnalyzer.Services.Intervals;

namespace CyclingTrainer.SessionAnalyzer.Models
{
    public class IntervalContainer
    {
        public List<Interval> Sprints { get; set; } = new();
        public List<Interval> Intervals { get; set; } = new();

        internal bool IsTheGapASprint(DateTime time)
        {
            return Sprints.Find(x => x.EndTime == time) != null;
        }

        internal bool AlreadyExists(Interval interval)
        {
            return Intervals.Any(x => IntervalsUtils.AreEqual(x, interval));
        }
    }
}