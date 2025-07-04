using TrainingDatabase.Core.Models;
using TrainingDatabase.Core.Services;

namespace TrainingDatabase.Core.Repository
{
    public static class CyclistRepository
    {
        static Dictionary<int, Cyclist> _cyclists { get; }
        const string DatabasePath = "Data Source=Resources/CyclistTraining.db";

        static CyclistRepository()
        {
            _cyclists = new Dictionary<int, Cyclist>();
            IEnumerable<Cyclist> list = DatabaseReaderService.GetCyclistsFromDb(DatabasePath);
            foreach (Cyclist c in list)
            {
                if (c == null) continue;
                IEnumerable<CyclistEvolution> evolutions = DatabaseReaderService.GetCyclistEvolution(DatabasePath, c.Id);
                c.Details = evolutions.FirstOrDefault();
                _cyclists.Add(c.Id, c);
            }
        }

        public static Dictionary<int, Cyclist> GetAll() => _cyclists;
        public static Cyclist? Get(int id) => _cyclists[id];
    }
}
