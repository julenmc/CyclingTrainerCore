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
            
            SessionData session = SessionRepository.AnalyzeRoute(@"Resources/19652409585_ACTIVITY.fit"); //aranguren 19652409585_ACTIVITY 19622171318_ACTIVITY 
            Logger.Info($"Route {session.Name} info: Lenght = {session.Route.Lenght}km. Elevation = {session.Route.Elevation}m"); 
            foreach (Climb climb in session.Route.Climbs)
            {
                Logger.Info($"Climb {climb.Id} info: Lenght = {climb.Lenght}m (starts at km {climb.InitKm}, ends at km {climb.InitKm + climb.Lenght/1000}). Elevation = {climb.Elevation} m (starts at {climb.InitAltitude} m, ends at {climb.MaxAltitude} m). Slope = {climb.Slope}% (max {climb.MaxSlope}%)");
            }

            int index = 100;
            Logger.Info($"Example. Power at distance {session.FitnessData[index].Position.Distance} m = {session.FitnessData[index].Stats.Power} W");
        }
    }
}