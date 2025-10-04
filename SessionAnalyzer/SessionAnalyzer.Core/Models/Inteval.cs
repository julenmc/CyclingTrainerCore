namespace CyclingTrainer.SessionAnalyzer.Core.Models
{
    public class Interval
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TimeDiff { get; set; }
        public float AveragePower { get; set; }
    }
}