using TrainingDatabase.Core.Models;
using TrainingDatabase.Core.Services;

namespace TrainingDatabase.Core.Repository
{
    public static class CyclistRepository
    {
        static Dictionary<int, Cyclist> _cyclists { get; }
        const string DatabasePath = @"Data Source=C:\FW_GIT\Pruebas\CyclingTrainerCore\TrainingDatabase\TrainingDatabase.Core\Resources\CyclistTraining.db";

        static CyclistRepository()
        {
            _cyclists = new Dictionary<int, Cyclist>();
            IEnumerable<Cyclist> list = DatabaseReaderService.GetCyclistsFromDb(DatabasePath);
            foreach (Cyclist c in list)
            {
                if (c == null) continue;
                IEnumerable<CyclistEvolution> evolutions = DatabaseReaderService.GetCyclistEvolution(DatabasePath, c.Id);
                if (evolutions.Count() == 0) continue;
                var curve = evolutions.First()?.MaxPowerCurveRaw;
                if (curve != null)
                {
                    evolutions.First().MaxPowerCurve = JsonService.LoadCurveFromJson(curve) ?? null;
                }
                c.Details = evolutions.FirstOrDefault();
                _cyclists.Add(c.Id, c);
            }
        }

        public static Dictionary<int, Cyclist> GetAll() => _cyclists;
        public static Cyclist? Get(int id) => _cyclists[id];

        public static int AddCyclist(Cyclist cyclist)
        {
            if (cyclist == null) throw new ArgumentNullException(nameof(cyclist));
            int id = DatabaseWriterService.AddCyclistToDb(DatabasePath, cyclist);
            cyclist.Id = id;
            _cyclists.Add(cyclist.Id, cyclist);
            if (cyclist.Details != null)
            {
                UpdateCyclist(id, cyclist.Details);
            }
            return id;
        }

        public static void UpdateCyclist(int id, CyclistEvolution evolution)
        {
            Cyclist? cyclist = Get(id);
            if (cyclist == null) throw new Exception("Cyclist ID not found!");
            ApplyCurrentStats(evolution, cyclist);
            if (evolution.MaxPowerCurve != null) evolution.MaxPowerCurveRaw = JsonService.GenerateJsonFromCurve(evolution.MaxPowerCurve);
            DatabaseWriterService.AddCyclistEvolutionToDb(DatabasePath, id, evolution);
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
