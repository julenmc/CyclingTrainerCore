using CommonModels;
using TrainingDatabase.Core.Repository;

namespace SessionAnalyzer.Core.Services
{
    internal static class ClimbSearchService
    {
        private const double ClimbFilterSize = 0.2f;    // Init point cant be more than 200m away from the climb start point
        internal static async void SearchClimb(Climb climb)
        {
            List<Climb> climbs = await ClimbRepository.GetClimbsAsync(climb.LongitudeInit, climb.LatitudeInit, ClimbFilterSize);
        }
    }
}
