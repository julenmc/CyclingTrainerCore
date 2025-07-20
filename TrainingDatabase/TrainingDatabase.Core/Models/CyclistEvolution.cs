namespace CyclingTrainer.TrainingDatabase.Core.Models
{
    public class CyclistEvolution
    {
        public DateTime UpdateDate { get; set; }
        public int Height { get; set; }
        public float Weight { get; set; }
        public float Vo2Max { get; set; }
        public string? MaxPowerCurveRaw { get; set; }
        public Dictionary<int, int>? MaxPowerCurve { get; set; }
    }
}