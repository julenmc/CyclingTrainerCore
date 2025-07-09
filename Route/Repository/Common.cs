using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Route.Repository
{
    public class SectorInfo
    {
        public double StartPoint { get; }
        public double EndPoint { get; }
        public double StartAlt { get; }
        public double EndAlt { get; }
        public double Slope { private set; get; }

        public SectorInfo(double sp, double ep, double sa, double ea, double slope)
        {
            StartPoint = sp;
            EndPoint = ep;
            StartAlt = sa;
            EndAlt = ea;
            Slope = slope;
        }
    }

    public class PointPosition
    {
        public int? Longitude { get; set; } = default!;
        public int? Latitude { get; set; } = default!;
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

    public class PointInfo
    {
        public Dynastream.Fit.DateTime Timestamp { get; set; } = default!;
        public int? Temperature { get; set; } = default!;
        public int? AccCalories { get; set; } = default!;
        public PointPosition Position { get; set; } = default!;
        public PointStats Stats { get; set; } = default!;
        public PointAdvStats Advanced { get; set; } = default!;
    }
}
