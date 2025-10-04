using CyclingTrainer.SessionReader.Core.Models;

namespace CyclingTrainer.SessionAnalyzer.Core.Services.Intervals
{
    internal static class AveragePowerCalculator
    {
        internal const int ShortWindowSize = 10;
        internal const int MediumWindowSize = 30;
        internal const int LongWindowSize = 60;

        internal static List<AveragePowerModel> CalculateMovingAverages(List<FitnessData> points, int windowSize)
        {
            if (points == null || !points.Any() || windowSize <= 0)
                return new List<AveragePowerModel>();

            var result = new List<AveragePowerModel>();
            var powerValues = new Queue<int>();
            float sum = 0;
            float sumSquares = 0;
            int min = int.MaxValue;
            int max = int.MinValue;

            // Initialize the first window
            for (int i = 0; i < Math.Min(windowSize, points.Count); i++)
            {
                var power = points[i].Stats.Power ?? 0;
                powerValues.Enqueue(power);
                sum += power;
                sumSquares += power * power;
                min = Math.Min(min, power);
                max = Math.Max(max, power);
            }

            // Calculate for the first window
            if (powerValues.Count > 0)
            {
                float avg = sum / powerValues.Count;
                float variance = (sumSquares / powerValues.Count) - (avg * avg);
                float stdDev = (float)Math.Sqrt(Math.Max(0, variance));

                result.Add(new AveragePowerModel(
                    points[windowSize - 1].Timestamp.GetDateTime(),
                    avg,
                    stdDev,
                    max - min
                ));
            }

            // Sliding window calculations
            for (int i = windowSize; i < points.Count; i++)
            {
                // Remove the oldest value
                var oldPower = powerValues.Dequeue();
                sum -= oldPower;
                sumSquares -= oldPower * oldPower;

                // Add the new value
                var newPower = points[i].Stats.Power ?? 0;
                powerValues.Enqueue(newPower);
                sum += newPower;
                sumSquares += newPower * newPower;

                // Recalculate min and max
                min = powerValues.Min();
                max = powerValues.Max();

                // Calculate statistics
                float avg = sum / windowSize;
                float variance = (sumSquares / windowSize) - (avg * avg);
                float stdDev = (float)Math.Sqrt(Math.Max(0, variance));

                result.Add(new AveragePowerModel(
                    points[i].Timestamp.GetDateTime(),
                    avg,
                    stdDev,
                    max - min
                ));
            }

            return result;
        }

        internal static void CalculateDeviationFromReference(List<AveragePowerModel> points, float referenceAverage)
        {
            if (referenceAverage <= 0)
                return;

            foreach (var point in points)
            {
                point.DeviationFromReference = Math.Abs(point.AvrgPower - referenceAverage) / referenceAverage;
            }
        }

        internal static bool IsStableInterval(
            IEnumerable<AveragePowerModel> windowPoints,
            float maxCoefficientOfVariation,
            float maxRangePercent,
            float maxDeviationFromReference = float.MaxValue)
        {
            if (!windowPoints.Any())
                return false;

            return windowPoints.All(p =>
                p.CoefficientOfVariation <= maxCoefficientOfVariation &&
                p.RangePercent <= maxRangePercent &&
                p.DeviationFromReference <= maxDeviationFromReference);
        }

        // Not used, maybe delete
        internal static List<AveragePowerModel> CalculateWindowAverages(
            List<FitnessData> points,
            int shortWindowSize = ShortWindowSize,  
            int mediumWindowSize = MediumWindowSize, 
            int longWindowSize = LongWindowSize)   
        {
            var shortAverages = CalculateMovingAverages(points, shortWindowSize);
            var mediumAverages = CalculateMovingAverages(points, mediumWindowSize);
            var longAverages = CalculateMovingAverages(points, longWindowSize);

            // Combine the results with timestamps from short window
            for (int i = 0; i < shortAverages.Count; i++)
            {
                var shortAvg = shortAverages[i];
                var mediumPoint = mediumAverages.FirstOrDefault(p => p.PointDate == shortAvg.PointDate);
                var longPoint = longAverages.FirstOrDefault(p => p.PointDate == shortAvg.PointDate);

                if (mediumPoint != null)
                {
                    shortAvg.DeviationFromReference = Math.Abs(shortAvg.AvrgPower - mediumPoint.AvrgPower) / mediumPoint.AvrgPower;
                }
                else if (longPoint != null)
                {
                    shortAvg.DeviationFromReference = Math.Abs(shortAvg.AvrgPower - longPoint.AvrgPower) / longPoint.AvrgPower;
                }
            }

            return shortAverages;
        }
    }
}