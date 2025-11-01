using CyclingTrainer.SessionReader.Models;
using CyclingTrainer.SessionAnalyzer.Models;
using NLog;

namespace CyclingTrainer.SessionAnalyzer.Services.Intervals
{
    internal static class PowerMetricsCalculator
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        internal static List<PowerMetrics> CalculateMovingAverages(List<FitnessData> points, int windowSize, IntervalContainer container)
        {
            if (points.Count < windowSize)
                throw new ArgumentException(nameof(points), $"Point count ({points.Count}) is smaller than the window size {windowSize}");

            var result = new List<PowerMetrics>();
            var powerValues = new Queue<int>();
            int min = int.MaxValue;
            int max = int.MinValue;
            int index = 0;

        createWindow:
            powerValues.Clear();
            int firstPoint = index + windowSize;
            float sum = 0;
            float sumSquares = 0;
            // Initialize the first window
            while (index < firstPoint)
            {
                var power = points[index].Stats.Power ?? 0;
                powerValues.Enqueue(power);
                sum += power;
                sumSquares += power * power;
                min = Math.Min(min, power);
                max = Math.Max(max, power);
                index++;
            }

            // Calculate for the first window
            if (powerValues.Count > 0)
            {
                float avg = sum / powerValues.Count;
                float variance = (sumSquares / powerValues.Count) - (avg * avg);
                float stdDev = (float)Math.Sqrt(Math.Max(0, variance));

                result.Add(new PowerMetrics(
                    points[index - 1].Timestamp.GetDateTime(),
                    avg,
                    stdDev,
                    max - min
                ));
            }

            // Sliding window calculations
            while (index < points.Count)
            {
                // Check if the session has been stopped
                int timeDiff = (int)(points[index].Timestamp.GetDateTime() - points[index - 1].Timestamp.GetDateTime()).TotalSeconds;
                if (timeDiff > 1 && !container.IsTheGapASprint(points[index].Timestamp.GetDateTime()))
                {
                    Log.Debug($"Session stopped at {points[index - 1].Timestamp.GetDateTime().TimeOfDay} for {timeDiff-1} seconds");
                    goto createWindow;
                }

                // Remove the oldest value
                var oldPower = powerValues.Dequeue();
                sum -= oldPower;
                sumSquares -= oldPower * oldPower;

                // Add the new value
                var newPower = points[index].Stats.Power ?? 0;
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

                result.Add(new PowerMetrics(
                    points[index].Timestamp.GetDateTime(),
                    avg,
                    stdDev,
                    max - min
                ));
                index++;
            }

            return result;
        }
    }
}