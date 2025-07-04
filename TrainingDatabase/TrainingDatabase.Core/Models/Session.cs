namespace TrainingDatabase.Core.Models
{
    public class Session
    {
        public int SessionId { get; set; }
        public string? Path { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double DistanceM { get; set; }
        public double HeightDiff { get; set; }
        public int Calories { get; set; }
        public int AverageHr { get; set; }
        public int AveragePower { get; set; }
        public Dictionary<int, int>? PowerCurve { get; set; }
        public bool IsIndoor { get; set; }
        public List<Interval>? Intervals { get; set; }
        public Dictionary<Climb, Interval>? Climbs { get; set; }
    }
}