using NLog;
using CommonModels;
using SessionReader.Core.Models;
using SessionReader.Core.Services;
using SessionReader.Core.Services.Fit;
using SessionReader.Core.Services.Gpx;

namespace SessionReader.Core.Repository
{
    public static class SessionRepository
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static Session _session = new Session();
        private static RouteSections _routeData = new RouteSections();
        private static List<FitnessData> _fitnessData = new List<FitnessData>();

        internal static Session AnalyzeRoute(ISessionReader reader)
        {
            return Analyze(reader);
        }

        public static Session AnalyzeRoute(string path)
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

        private static Session Analyze(ISessionReader reader)
        {
            reader.Read();
            _session = new Session();
            _routeData = new RouteSections();
            _fitnessData = new List<FitnessData>();
            _session.Name = reader.GetName();
            _session.Distance = Math.Round(reader.GetLenght(), 2);
            _session.HeightDiff = Math.Round(reader.GetElevation(), 0);
            _routeData.Sectors = reader.GetSmoothedSectors();
            _routeData.Climbs = ClimbFinderService.GetClimbs(_routeData.Sectors);
            Log.Info($"New route analyzed: {_session.Name}. Length = {Math.Round(_session.Distance / 1000, 2)} km, Elevation = {_session.HeightDiff} m");

            _fitnessData = reader.GetFitnessData();
            if (_fitnessData.Count == 0)
            {
                Log.Warn("No fitness data found in the session. Please check the file.");
                return _session;
            }

            Log.Info($"Fitness data found: {_fitnessData.Count} records");
            SetClimbCoords();

            return _session;
        }

        private static void SetClimbCoords()
        {
            int index = 0;
            foreach (Climb climb in _routeData.Climbs)
            {
                while (index < _fitnessData.Count && climb.InitRouteDistance < _fitnessData[index].Position.Distance)
                {
                    index++;
                }
                if (index >= _fitnessData.Count) continue;

                climb.LongitudeInit = _fitnessData[index].Position.Longitude ?? 0;
                climb.LongitudeEnd = _fitnessData[index+1].Position.Longitude ?? 0;
                climb.LatitudeInit = _fitnessData[index].Position.Latitude ?? 0;
                climb.LatitudeEnd = _fitnessData[index + 1].Position.Latitude ?? 0;
            }
        }

        public static Session GetSession() => _session;
        public static RouteSections GetRouteData() => _routeData;
        public static List<FitnessData> GetFitnessData() => _fitnessData;
    }
}
