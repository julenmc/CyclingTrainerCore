using CyclingTrainer.Core.Models;
using NLog;
using CyclingTrainer.TrainingDatabase.Core.Repository;

namespace CyclingTrainer.SessionAnalyzer.Core.Services
{
    internal static class ClimbSearchService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const double ClimbFilterSize = 0.2f;    // Init point cant be more than 200m away from the climb start point
        private const double MaxError = 1f;             // Max error in percentage for the climb search

        internal static async Task<Climb?> SearchClimb(Climb climb)
        {
            List<Climb> foundClimbs = await ClimbRepository.GetClimbsAsync(climb.LongitudeInit, climb.LatitudeInit, ClimbFilterSize);
            if (foundClimbs.Count == 0)
            {
                Log.Info("No climbs found in the area");
                return null;
            }
            else if (foundClimbs.Count == 1)
            {
                double min = Math.Min(foundClimbs.First().Distance, climb.Distance);
                double max = Math.Max(foundClimbs.First().Distance, climb.Distance);
                double diff = (max - min) / min * 100;
                if (diff < MaxError) return foundClimbs.First();
                else return null;
            }
            else
            {
                Log.Warn($"{foundClimbs.Count} climbs found!");
                return null;
            }
        }
    }
}
