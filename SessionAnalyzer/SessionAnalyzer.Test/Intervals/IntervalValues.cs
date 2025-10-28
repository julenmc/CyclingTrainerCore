namespace CyclingTrainer.SessionAnalyzer.Test.Intervals
{
    internal class IntervalValues
    {
        internal int DefaultTime { get; set; }
        internal int MaxPower { get; set; }
        internal int MinPower { get; set; }
        internal int DefaultPower { get => (MaxPower + MinPower) / 2; }
    }
}