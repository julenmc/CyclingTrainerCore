using NLog;
using SessionAnalyzer.Core.Models;
using SessionAnalyzer.Core.Services;

namespace SessionAnalyzer.Core.Repository
{
    public static class RouteRepository
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static Route _route = new Route();

        public static Route AnalyzeRoute(IReader reader)
        {
            reader.Read();
            _route = new Route();
            _route.Name = reader.GetName();
            _route.Lenght = Math.Round(reader.GetLenght(), 2);
            _route.Elevation = Math.Round(reader.GetElevation(), 0);
            _route.Climbs = ClimbFinderService.GetClimbs(reader.GetAllSectors());
            Log.Info($"New route analyzed: {_route.Name}. Length = {_route.Lenght} km, Elevation = {_route.Elevation} m");

            return _route;
        }

        public static Route GetRoute() => _route;
    }
}
