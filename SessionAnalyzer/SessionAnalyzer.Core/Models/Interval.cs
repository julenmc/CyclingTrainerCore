namespace CyclingTrainer.SessionAnalyzer.Models
{
    public class Interval
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int TimeDiff { get; set; }
        public float AveragePower { get; set; }
        public float MaxPower { get; set; }
        public List<Interval> Intervals { get; set; } = new();

        internal bool IsSubInterval(Interval potential)
        {
            return potential.StartTime >= this.StartTime &&
                   potential.EndTime <= this.EndTime &&
                   potential != this;
        }
    }
}