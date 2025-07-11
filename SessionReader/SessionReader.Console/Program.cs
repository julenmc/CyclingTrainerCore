using SessionReader.Core.Models;
using SessionReader.Core.Repository;

namespace SessionReader.Console
{
    public static class Program
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Main()
        {
            Logger.Info("Start Session Analyzer Test");
            
            Route route = RouteRepository.AnalyzeRoute(@"Resources/aranguren.gpx"); //19652409585_ACTIVITY 19622171318_ACTIVITY 
            Logger.Info($"Route {route.Name} info: Lenght = {route.Lenght}km. Elevation = {route.Elevation}m"); 
            foreach (Climb climb in route.Climbs)
            {
                Logger.Info($"Climb {climb.Id} info: Lenght = {climb.Lenght}m (starts at km {climb.InitKm}, ends at km {climb.InitKm + climb.Lenght/1000}). Elevation = {climb.Elevation} m (starts at {climb.InitAltitude} m, ends at {climb.MaxAltitude} m). Slope = {climb.Slope}% (max {climb.MaxSlope}%)");
            }
            foreach (SectorInfo sector in route.Sectors)
            {
                if (sector.Slope > 10)
                {
                    Logger.Info($"Big slope ({sector.Slope}%) from {sector.StartPoint} to {sector.EndPoint}");
                }
            }
        }
    }
}