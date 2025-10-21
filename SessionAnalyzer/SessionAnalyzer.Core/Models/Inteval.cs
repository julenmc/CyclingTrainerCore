namespace CyclingTrainer.SessionAnalyzer.Models
{
    public class Interval
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TimeDiff { get; set; }
        public float AveragePower { get; set; }
        public List<Interval>? Intervals { get; set; }
    }
}