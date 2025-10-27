namespace CyclingTrainer.Core.Models
{
    public class AnalyzedData
    {
        public List<Interval>? Intervals { get; set; }
        public Dictionary<Climb, Interval>? Climbs { get; set; }
        public int Calories { get; set; }
        public int AverageHr { get; set; }
        public int AveragePower { get; set; }
        public int AverageCadence { get; set; }
        public string? PowerCurveRaw { get; set; }
        public Dictionary<int, PowerCurveData>? PowerCurve { get; set; }
        public Dictionary<Zone, int>? HrZones { get; set; }
        public Dictionary<Zone, int>? PowerZones { get; set; }
    }
}