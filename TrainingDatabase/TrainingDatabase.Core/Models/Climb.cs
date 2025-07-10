namespace TrainingDatabase.Core.Models
{
    public class Climb
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public double LongitudeInit { get; set; }
        public double LongitudeEnd { get; set; }
        public double LatitudeInit { get; set; }
        public double LatitudeEnd { get; set; }
        public double AltitudeInit { get; set; }
        public double AltitudeEnd { get; set; }
        public int Distance { get; set; }
        public double AverageSlope { get; set; }
        public double MaxSlope { get; set; }
        public int HeightDiff { get; set; }
    }
}