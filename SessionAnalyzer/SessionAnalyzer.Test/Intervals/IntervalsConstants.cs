namespace CyclingTrainer.SessionAnalyzer.Test.Intervals
{
    internal class IntervalsConstants
    {
        internal const int NuleMaxValue = 180;
        internal const int NuleMinValue = 0;
        internal const int NulePowerDefaultValue = (NuleMaxValue + NuleMinValue) / 2;
        internal const int ShortMaxValue = 320;
        internal const int ShortMinValue = 300;
        internal const int ShortPowerDefaultValue = (ShortMaxValue + ShortMinValue) / 2;
        internal const int MediumMaxValue = 270;
        internal const int MediumMinValue = 250;
        internal const int MediumPowerDefaultValue = (MediumMaxValue + MediumMinValue) / 2;
        internal const int LongMaxValue = 220;
        internal const int LongMinValue = 190;
        internal const int LongPowerDefaultValue = (LongMaxValue + LongMinValue) / 2;
        internal const float ShortAcpDelta = 0.2f;   // 20% time acceptance delta for short intervals
        internal const float MediumAcpDelta = 0.1f;  // 10% time acceptance delta for medium intervals
        internal const float LongAcpDelta = 0.05f;   // 5% time acceptance delta for long intervals
    }
}