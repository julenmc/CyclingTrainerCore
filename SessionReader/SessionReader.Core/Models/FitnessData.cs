namespace CyclingTrainer.SessionReader.Models
{
    public class FitnessData
    {
        public Dynastream.Fit.DateTime Timestamp { get; set; } = default!;
        public int? Temperature { get; set; } = default!;
        public int? AccCalories { get; set; } = default!;
        public PointPosition Position { get; set; } = default!;
        public PointStats Stats { get; set; } = default!;
        public PointAdvStats Advanced { get; set; } = default!;
    }

    public class PointPosition
    {
        public double? Longitude { get; set; } = default!;
        public double? Latitude { get; set; } = default!;
        public float? Altitude { get; set; } = default!;
        public float? Distance { get; set; } = default!;
    }

    public class PointStats
    {
        public int? Power { get; set; } = default!;
        public float? Speed { get; set; } = default!;
        public int? HeartRate { get; set; } = default!;
        public int? Cadence { get; set; } = default!;
        public int? RespirationRate { get; set; } = default!;
    }

    public class PointAdvStats
    {
        public float? FractionalCadence { get; set; } = default!;
        public int? LeftPco { get; set; } = default!;
        public int? RightPco { get; set; } = default!;
        public float? LeftPowerPhase { get; set; } = default!;
        public float? RightPowerPhase { get; set; } = default!;
        public float? LeftPowerPhasePeak { get; set; } = default!;
        public float? RightPowerPhasePeak { get; set; } = default!;
    }
}
