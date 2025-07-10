namespace TrainingDatabase.Core.Models
{
    public class Interval
    {
        public int IntervalId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public double TotalDistance { get; set; }
        public int AverageHeartRate { get; set; }
        public int AveragePower { get; set; }
        public int AverageCadence { get; set; }
    }
}