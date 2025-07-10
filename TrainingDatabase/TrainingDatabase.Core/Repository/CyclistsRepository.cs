using TrainingDatabase.Core.Models;
using TrainingDatabase.Core.Services;

namespace TrainingDatabase.Core.Repository
{
    public static class CyclistsRepository
    {
        static Dictionary<int, Cyclist> _cyclists { get; }

        const string DatabasePath = @"Data Source=C:\FW_GIT\Pruebas\CyclingTrainerCore\TrainingDatabase\TrainingDatabase.Core\Resources\CyclistTraining.db";

        static CyclistsRepository()
        {
            _cyclists = new Dictionary<int, Cyclist>();
        }

        public static async Task InitializeRepositoryAsync()
        {
            IEnumerable<Cyclist> list = await DatabaseReaderService.GetCyclistsAsync(DatabasePath);
            foreach (Cyclist c in list)
            {
                if (c == null) continue;
                IEnumerable<CyclistEvolution> evolutions = await DatabaseReaderService.GetCyclistEvolutionAsync(DatabasePath, c.Id);
                if (evolutions.Count() == 0) continue;
                var curve = evolutions.First()?.MaxPowerCurveRaw;
                if (curve != null)
                {
                    evolutions.First().MaxPowerCurve = JsonService.LoadCurveFromJson(curve) ?? null;
                }
                c.Details = evolutions.First();
                _cyclists.Add(c.Id, c);
            }
        }

        public static Dictionary<int, Cyclist> GetAll() => _cyclists;
        public static Cyclist? Get(int id) => _cyclists[id];

        public static async Task<SessionsRepository> GetCyclistSessionsAsync(int id)
        {
            SessionsRepository repo = new SessionsRepository(id);
            await repo.LoadCyclistSessionsAsync();
            return repo;
        }

        public static async Task<int> AddCyclistAsync(Cyclist cyclist)
        {
            if (cyclist == null) throw new ArgumentNullException(nameof(cyclist));
            int id = await DatabaseWriterService.AddCyclistAsync(DatabasePath, cyclist);
            cyclist.Id = id;
            _cyclists.Add(cyclist.Id, cyclist);
            if (cyclist.Details != null)
            {
                await UpdateCyclist(id, cyclist.Details);
            }
            return id;
        }

        public static async Task UpdateCyclist(int id, CyclistEvolution evolution)
        {
            Cyclist? cyclist = Get(id);
            if (cyclist == null) throw new Exception("Cyclist ID not found!");
            ApplyCurrentStats(evolution, cyclist);
            if (evolution.MaxPowerCurve != null) evolution.MaxPowerCurveRaw = JsonService.GenerateJsonFromCurve(evolution.MaxPowerCurve);
            await DatabaseWriterService.AddCyclistEvolutionAsync(DatabasePath, id, evolution);
        }

        private static void ApplyCurrentStats(CyclistEvolution evolution, Cyclist cyclist)
        {
            if (evolution.Height == 0) evolution.Height = cyclist.Details.Height;
            if (evolution.Weight == 0) evolution.Weight = cyclist.Details.Weight;
            if (evolution.Vo2Max == 0) evolution.Vo2Max = cyclist.Details.Vo2Max;
            if (evolution.MaxPowerCurve == null) evolution.MaxPowerCurve = cyclist.Details.MaxPowerCurve;
        }
    }
}
