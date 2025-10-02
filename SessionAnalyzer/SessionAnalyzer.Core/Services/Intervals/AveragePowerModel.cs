namespace CyclingTrainer.SessionAnalyzer.Core.Services.Intervals
{
    internal class AveragePowerModel
    {
        internal DateTime PointDate { get; set; }
        internal float AvrgPower { get; set; }
        internal float Deviation { get; set; }
        internal float CoefficientOfVariation => Deviation / AvrgPower;
        internal int MaxMinDelta { get; set; }
        internal float RangePercent => MaxMinDelta / AvrgPower;
        internal float DeviationFromReference { get; set; }

        internal AveragePowerModel(DateTime pointDate, float avgPower, float deviation, int maxMinDelta)
        {
            PointDate = pointDate;
            AvrgPower = avgPower;
            Deviation = deviation;
            MaxMinDelta = maxMinDelta;
            DeviationFromReference = 0;
        }
    }
}