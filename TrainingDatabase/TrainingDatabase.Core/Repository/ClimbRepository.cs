using TrainingDatabase.Core.Models;
using TrainingDatabase.Core.Services;

namespace TrainingDatabase.Core.Repository
{
    public static class ClimbRepository
    {
        const string DatabasePath = @"Data Source=C:\FW_GIT\Pruebas\CyclingTrainerCore\TrainingDatabase\TrainingDatabase.Core\Resources\CyclistTraining.db";

        public static async Task<Climb> GetClimbAsync(int id)
        {
            IEnumerable<Climb> enumerable = await DatabaseReaderService.GetClimbWithIdAsync(DatabasePath, id);
            if (enumerable.Count() > 1)
            {
                throw new KeyNotFoundException($"More than one climb with ID {id}");
            }
            return enumerable.First();
        }

        public static async Task<List<Climb>> GetClimbsAsync(string name)
        {
            throw new NotImplementedException();
        }

        public static async Task<List<Climb>> GetClimbsAsync(double longitude, double latitude, double size)
        {
            double deltaLat = (size / 2.0) / 111.32;
            double deltaLon = (size / 2.0) / (111.32 * Math.Cos(latitude * Math.PI / 180.0));
            double minLat = latitude - deltaLat;
            double maxLat = latitude + deltaLat;
            double minLon = longitude - deltaLon;
            double maxLon = longitude + deltaLon;
            IEnumerable<Climb> enumerable = await DatabaseReaderService.GetClimbsWithCoordsFilterAsync(DatabasePath, minLon, maxLon, minLat, maxLat);
            return enumerable.ToList();
        }

        public static async Task<int> AddClimb(Climb climb)
        {
            if (climb == null) throw new ArgumentNullException(nameof(climb));
            int id = await DatabaseWriterService.AddClimbAsync(DatabasePath, climb);
            return id;
        }
    }
}
