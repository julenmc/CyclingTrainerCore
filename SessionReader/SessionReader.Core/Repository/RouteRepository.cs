using Dynastream.Fit;
using NLog;
using SessionReader.Core.Models;
using SessionReader.Core.Services;
using SessionReader.Core.Services.Fit;

namespace SessionReader.Core.Repository
{
    public static class RouteRepository
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static Route _route = new Route();

        internal static Route AnalyzeRoute(IReader reader)
        {
            return Analyze(reader);
        }

        public static Route AnalyzeRoute(string path)
        {
            switch (Path.GetExtension(path))
            {
                case ".fit":
                    FitReader fitReader = new FitReader(path);
                    return Analyze(fitReader);
                case ".gpx":
                    Gpx gpxReader = new Gpx(path);
                    return Analyze(gpxReader);
                default:
                    throw new ArgumentException("File not valid");
            }
        }

        private static Route Analyze(IReader reader)
        {
            reader.Read();
            _route = new Route();
            _route.Name = reader.GetName();
            _route.Lenght = Math.Round(reader.GetLenght(), 2);
            _route.Elevation = Math.Round(reader.GetElevation(), 0);
            _route.Sectors = reader.GetAllSectors();
            _route.Climbs = ClimbFinderService.GetClimbs(_route.Sectors);
            Log.Info($"New route analyzed: {_route.Name}. Length = {_route.Lenght} km, Elevation = {_route.Elevation} m");

            return _route;
        }

        public static Route GetRoute() => _route;
    }
}
