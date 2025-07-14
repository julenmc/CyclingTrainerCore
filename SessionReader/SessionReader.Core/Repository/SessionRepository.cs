using NLog;
using SessionReader.Core.Models;
using SessionReader.Core.Services;
using SessionReader.Core.Services.Fit;
using SessionReader.Core.Services.Gpx;

namespace SessionReader.Core.Repository
{
    public static class SessionRepository
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static SessionData _sessionData = new SessionData();

        internal static SessionData AnalyzeRoute(ISessionReader reader)
        {
            return Analyze(reader);
        }

        public static SessionData AnalyzeRoute(string path)
        {
            switch (Path.GetExtension(path))
            {
                case ".fit":
                    FitReader fitReader = new FitReader(path);
                    return Analyze(fitReader);
                case ".gpx":
                    GpxReader gpxReader = new GpxReader(path);
                    return Analyze(gpxReader);
                default:
                    throw new ArgumentException("File not valid");
            }
        }

        private static SessionData Analyze(ISessionReader reader)
        {
            reader.Read();
            _sessionData = new SessionData();
            _sessionData.Name = reader.GetName();
            _sessionData.Route.Lenght = Math.Round(reader.GetLenght(), 2);
            _sessionData.Route.Elevation = Math.Round(reader.GetElevation(), 0);
            _sessionData.Route.Sectors = reader.GetSmoothedSectors();
            _sessionData.Route.Climbs = ClimbFinderService.GetClimbs(_sessionData.Route.Sectors);
            Log.Info($"New route analyzed: {_sessionData.Name}. Length = {_sessionData.Route.Lenght} km, Elevation = {_sessionData.Route.Elevation} m");

            _sessionData.FitnessData = reader.GetFitnessData();
            Log.Info($"Fitness data found: {_sessionData.FitnessData.Count} records");

            return _sessionData;
        }

        public static SessionData GetRoute() => _sessionData;
        public static List<FitnessData> GetFitnessData() => _sessionData.FitnessData;
    }
}
