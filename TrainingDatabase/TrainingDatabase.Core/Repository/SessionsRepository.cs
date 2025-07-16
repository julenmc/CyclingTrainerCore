using CommonModels;
using TrainingDatabase.Core.Services;

namespace TrainingDatabase.Core.Repository
{
    public class SessionsRepository
    {
        const string DatabasePath = @"Data Source=C:\FW_GIT\Pruebas\CyclingTrainerCore\TrainingDatabase\TrainingDatabase.Core\Resources\CyclistTraining.db";

        Dictionary<int, Session> _sessions { get; set; }
        readonly int _cyclistId;

        public SessionsRepository(int cyclistId)
        {
            _sessions = new Dictionary<int, Session>();
            _cyclistId = cyclistId;
        }

        internal async Task<IEnumerable<Session>> LoadCyclistSessionsAsync()
        {
            IEnumerable<Session> list = await DatabaseReaderService.GetCyclistSessionsAsync(DatabasePath, _cyclistId);
            Task[] tasks = new Task[list.Count()];
            int index = 0;
            foreach (Session session in list)
            {
                tasks[index++] = LoadSingleSessionAsync(session);
            }
            await Task.WhenAll(tasks);
            _sessions = list.ToDictionary(s => s.Id, s => s);
            return list;
        }

        private async Task LoadSingleSessionAsync(Session session)
        {
            IEnumerable<Interval> interval = await DatabaseReaderService.GetSessionIntervalsAsync(DatabasePath, session.Id);
            session.AnalyzedData.Intervals = interval.ToList();
        }

        public Dictionary<int, Session> GetAll() => _sessions;
        public Session? Get(int id) => _sessions[id];

        public async Task<int> AddSessionAsync(Session session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            if (session.AnalyzedData.PowerCurve != null) session.AnalyzedData.PowerCurveRaw = JsonService.GenerateJsonFromCurve(session.AnalyzedData.PowerCurve);
            int sessionId = await DatabaseWriterService.AddSessionAsync(DatabasePath, _cyclistId, session);
            session.Id = sessionId;
            if (session.AnalyzedData.Intervals != null)
            {
                Task[] tasks = new Task[session.AnalyzedData.Intervals.Count()];
                int index = 0;
                foreach (Interval interval in session.AnalyzedData.Intervals)
                {
                    tasks[index++] = AddSingleInterval(sessionId, interval);
                }
                await Task.WhenAll(tasks);
            }
            if (session.AnalyzedData.Climbs != null)
            {
                Task[] tasks = new Task[session.AnalyzedData.Climbs.Count()];
                int index = 0;
                foreach (var kvp in session.AnalyzedData.Climbs)
                {
                    tasks[index++] = AddSingleClimbInterval(sessionId, kvp.Key.Id, kvp.Value);
                }
                await Task.WhenAll(tasks);
            }
            _sessions.Add(session.Id, session);
            return sessionId;
        }

        private async Task AddSingleInterval(int sessionId, Interval interval)
        {
            int intervalId = await DatabaseWriterService.AddIntervalAsync(DatabasePath, sessionId, interval);
            interval.IntervalId = intervalId;
        }

        private async Task AddSingleClimbInterval(int sessionId, int climbId, Interval interval)
        {
            int intervalId = await DatabaseWriterService.AddClimbIntervalAsync(DatabasePath, sessionId, climbId, interval);
            interval.IntervalId = intervalId;
        }
    }
}
