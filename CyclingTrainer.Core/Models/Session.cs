namespace CyclingTrainer.Core.Models
{
    public class Session
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double Distance { get; set; }
        public double HeightDiff { get; set; }
        public bool IsIndoor { get; set; }
        public AnalyzedData AnalyzedData { get; set; } = default!;
}

    public class AnalyzedData
    {
        public List<Interval>? Intervals { get; set; }
        public Dictionary<Climb, Interval>? Climbs { get; set; }
        public int Calories { get; set; }
        public int AverageHr { get; set; }
        public int AveragePower { get; set; }
        public int AverageCadence { get; set; }
        public string? PowerCurveRaw { get; set; }
        public Dictionary<int, int>? PowerCurve { get; set; }
    }
}