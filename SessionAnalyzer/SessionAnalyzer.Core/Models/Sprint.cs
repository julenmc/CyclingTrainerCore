namespace CyclingTrainer.SessionAnalyzer.Core.Models
{
    public class Sprint
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public float AveragePower { get; set; }
        public float MaxPower { get; set; }
    }
}