using CoreModels = CyclingTrainer.Core.Models;

namespace CyclingTrainer.SessionAnalyzer.Test.Intervals
{
    internal class IntervalsConstants
    {
        internal const int NuleMaxValue = 180;
        internal const int NuleMinValue = 0;
        internal const int NulePowerDefaultValue = (NuleMaxValue + NuleMinValue) / 2;
        internal const int ShortMaxValue = 320;
        internal const int ShortMinValue = 300;
        internal const int ShortDefaultPower = (ShortMaxValue + ShortMinValue) / 2;
        internal const int MediumMaxValue = 270;
        internal const int MediumMinValue = 250;
        internal const int MediumDefaultPower = (MediumMaxValue + MediumMinValue) / 2;
        internal const int LongMaxValue = 220;
        internal const int LongMinValue = 190;
        internal const int LongDefaultPower = (LongMaxValue + LongMinValue) / 2;
        internal const int ShortDefaultTime = 90;
        internal const int MediumDefaultTime = 400;
        internal const int LongDefaultTime = 1000;
        internal const float ShortAcpDelta = 0.2f;   // 20% time acceptance delta for short intervals
        internal const float MediumAcpDelta = 0.15f; // 15% time acceptance delta for medium intervals
        internal const float LongAcpDelta = 0.05f;   // 5% time acceptance delta for long intervals

        internal static readonly List<CoreModels.Zone> PowerZones = new List<CoreModels.Zone>{
            new CoreModels.Zone { Id = 1, LowLimit = 0, HighLimit = 129},
            new CoreModels.Zone { Id = 2, LowLimit = 130, HighLimit = NuleMaxValue - 1},
            new CoreModels.Zone { Id = 3, LowLimit = NuleMaxValue, HighLimit = LongMaxValue - 1},
            new CoreModels.Zone { Id = 4, LowLimit = LongMaxValue, HighLimit = MediumMaxValue - 1},
            new CoreModels.Zone { Id = 5, LowLimit = MediumMaxValue, HighLimit = ShortMaxValue - 1},
            new CoreModels.Zone { Id = 6, LowLimit = ShortMaxValue, HighLimit = ShortMaxValue + 49},
            new CoreModels.Zone { Id = 7, LowLimit = ShortMaxValue + 50, HighLimit = 2000}
        };
    }
}