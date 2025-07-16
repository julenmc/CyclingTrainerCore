using CommonModels;
using SessionReader.Core.Models;
using SessionReader.Core.Repository;

namespace SessionReader.Console
{
    public static class Program
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Main()
        {
            Logger.Info("Start Session Reader Test");
            
            Session session = SessionRepository.AnalyzeRoute(@"Resources/19652409585_ACTIVITY.fit"); //aranguren 19652409585_ACTIVITY 19622171318_ACTIVITY 
            RouteSections routeData = SessionRepository.GetRouteData();
            List<FitnessData> fitnessData = SessionRepository.GetFitnessData();
            Logger.Info($"Route {session.Name} info: Lenght = {session.Distance}m. Elevation = {session.HeightDiff}m"); 
            foreach (Climb climb in routeData.Climbs)
            {
                Logger.Info($"Climb {climb.Id} info: Lenght = {climb.Distance}m (starts at {climb.InitRouteDistance}m; ends at {climb.EndRouteDistance}m). Elevation = {climb.HeightDiff} m (starts at {climb.AltitudeInit} m, ends at {climb.AltitudeEnd} m). Slope = {climb.AverageSlope}% (max {climb.MaxSlope}%)");
            }

            if (fitnessData.Count != 0)
            {
                int index = 100;
                Logger.Info($"Example. Power at distance {fitnessData[index].Position.Distance} m = {fitnessData[index].Stats.Power} W");
            }
        }
    }
}