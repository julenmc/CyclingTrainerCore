using CyclingTrainer.Core.Models;
using CyclingTrainer.SessionReader.Models;
using CyclingTrainer.SessionReader.Services;

namespace CyclingTrainer.SessionReader.Console
{
    public static class Program
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Main()
        {
            Logger.Info("Start Session Reader Test");

            SessionContainer sessionContainer = SessionReaderService.ReadRoute(@"Resources/19652409585_ACTIVITY.fit"); //aranguren 19652409585_ACTIVITY 19622171318_ACTIVITY 
            Logger.Info($"Route {sessionContainer.Session.Name} info: Lenght = {sessionContainer.Session.Distance}m. Elevation = {sessionContainer.Session.HeightDiff}m"); 
            foreach (Climb climb in sessionContainer.RouteSections.Climbs)
            {
                Logger.Info($"Climb {climb.Id} info: Lenght = {climb.Distance}m (starts at {climb.InitRouteDistance}m; ends at {climb.EndRouteDistance}m). Elevation = {climb.HeightDiff} m (starts at {climb.AltitudeInit} m, ends at {climb.AltitudeEnd} m). Slope = {climb.AverageSlope}% (max {climb.MaxSlope}%)");
            }

            if (sessionContainer.FitnessDataContainer.FitnessData.Count != 0)
            {
                int index = 100;
                Logger.Info($"Example. Power at distance {sessionContainer.FitnessDataContainer.FitnessData[index].Position.Distance} m = {sessionContainer.FitnessDataContainer.FitnessData[index].Stats.Power} W");
            }
        }
    }
}