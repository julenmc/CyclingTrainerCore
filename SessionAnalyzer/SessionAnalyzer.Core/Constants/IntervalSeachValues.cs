namespace CyclingTrainer.SessionAnalyzer.Core.Constants
{
    public static class IntervalSearchValues
    {
        public const float CvStartThrMinValue = 0.01f;      // STD/Avr relation threshold min value on interval start detection
        public const float CvStartThrMaxValue = 0.2f;
        public const float CvStartThrDefaultValue = 0.03f;
        public const float CvFollowThrMinValue = 0.01f;     // STD/Avr relation threshold min value on following interval
        public const float CvFollowThrMaxValue = 0.5f;
        public const float CvFollowThrDefaultValue = 0.05f;
        public const float RangeThrMinValue = 0.05f;        // Period max-min values dif threshold min value
        public const float RangeThrMaxValue = 1f;
        public const float RangeThrDefaultValue = 0.15f;
        public const float MaRelThrMinValue = 0.05f;        // AvrShort/AvrLong relation threshold min value
        public const float MaRelThrMaxValue = 0.5f;  
        public const float MaRelThrDefaultValue = 0.1f;  
    }
}