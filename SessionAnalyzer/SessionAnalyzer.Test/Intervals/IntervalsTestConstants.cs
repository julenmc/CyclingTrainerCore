using CoreModels = CyclingTrainer.Core.Models;
using CyclingTrainer.SessionAnalyzer.Models;
using CyclingTrainer.SessionAnalyzer.Constants;
using CyclingTrainer.SessionAnalyzer.Enums;

namespace CyclingTrainer.SessionAnalyzer.Test.Intervals
{
    internal static class IntervalsTestConstants
    {
        internal readonly static IntervalValues NuleIntervalValues = new IntervalValues()
        {
            DefaultTime = 300,
            MaxPower = 180,
            MinPower = 0
        };
        internal readonly static IntervalValues ShortIntervalValues = new IntervalValues()
        {
            DefaultTime = 90,
            MaxPower = 320,
            MinPower = 300
        };
        internal readonly static IntervalValues MediumIntervalValues = new IntervalValues()
        {
            DefaultTime = 360,
            MaxPower = 270,
            MinPower = 250
        };
        internal readonly static IntervalValues LongIntervalValues = new IntervalValues()
        {
            DefaultTime = 900,
            MaxPower = 220,
            MinPower = 190
        };
        internal const float ShortAcpDelta = 0.25f;   // 25% time delta accepted for short intervals
        internal const float MediumAcpDelta = 0.2f; // 20% time delta accepted for medium intervals
        internal const float LongAcpDelta = 0.1f;   // 10% time delta accepted for long intervals

        internal static readonly List<CoreModels.Zone> PowerZones = new List<CoreModels.Zone>{
            new CoreModels.Zone { Id = 1, LowLimit = 0, HighLimit = 129},
            new CoreModels.Zone { Id = 2, LowLimit = 130, HighLimit = NuleIntervalValues.MaxPower - 1},
            new CoreModels.Zone { Id = 3, LowLimit = NuleIntervalValues.MaxPower, HighLimit = LongIntervalValues.MaxPower - 1},
            new CoreModels.Zone { Id = 4, LowLimit = LongIntervalValues.MaxPower, HighLimit = MediumIntervalValues.MaxPower - 1},
            new CoreModels.Zone { Id = 5, LowLimit = MediumIntervalValues.MaxPower, HighLimit = ShortIntervalValues.MaxPower - 1},
            new CoreModels.Zone { Id = 6, LowLimit = ShortIntervalValues.MaxPower, HighLimit = ShortIntervalValues.MaxPower + 49},
            new CoreModels.Zone { Id = 7, LowLimit = ShortIntervalValues.MaxPower + 50, HighLimit = 2000}
        };

        internal readonly static Thresholds ShortThresholds = new Thresholds(cvStart: 0.15f, cvFollow: 0.20f, range: 0.20f, maRel: 0.15f);
        internal readonly static Thresholds MediumThresholds = new Thresholds(cvStart: 0.30f, cvFollow: 0.30f, range: 0.50f, maRel: 0.20f);
        internal readonly static Thresholds LongThresholds = new Thresholds(cvStart: 0.40f, cvFollow: 0.40f, range: 1.00f, maRel: 0.30f);
    }
}