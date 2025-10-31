using CoreModels = CyclingTrainer.Core.Models;
using CyclingTrainer.SessionAnalyzer.Constants;
using CyclingTrainer.SessionAnalyzer.Models;
using CyclingTrainer.SessionReader.Models;
using NLog;
using CyclingTrainer.SessionAnalyzer.Enums;

namespace CyclingTrainer.SessionAnalyzer.Services.Intervals
{
    public static class IntervalsService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static List<Interval> Search(List<FitnessData> activityPoints, List<CoreModels.Zone> powerZones, Thresholds? thresholds = null)
        {
            Log.Info("Starting intervals search...");
            if (activityPoints == null || !activityPoints.Any())
            {
                Log.Warn("No activity points provided");
                return new List<Interval>();
            }

            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(activityPoints);
            IntervalContainer intervalContainer = new IntervalContainer();

            // Detectar y eliminar sprints primero
            CoreModels.Zone? sprint = powerZones.Find(x => x.Id == 7);
            if (sprint == null)
            {
                Log.Warn("No sprint power zone found");
                return new List<Interval>();
            }
            int sprintPower = sprint.LowLimit;
            Log.Info($"Starting sprint detection and removal at {sprintPower} W...");
            SprintService sprintService = new SprintService(fitnessDataContainer, IntervalTimes.SprintMinTime, sprintPower * 11 / 10, sprintPower);
            intervalContainer.Sprints.AddRange(sprintService.SearchSprints());
            Log.Info("Sprint detection completed");

            // Short intervals
            Log.Info($"Starting short interval search...");
            IntervalsFinder finder = new IntervalsFinder(fitnessDataContainer, intervalContainer, powerZones, IntervalSeachGroups.Short, thresholds);
            int saved = finder.Search();
            Log.Info($"Short intervals search done, {saved} saved");

            // Medium intervals
            Log.Info($"Starting medium interval search...");
            finder = new IntervalsFinder(fitnessDataContainer, intervalContainer, powerZones, IntervalSeachGroups.Medium, thresholds);
            saved = finder.Search();
            Log.Info($"Medium intervals search done, {saved} saved");

            // Long intervals
            Log.Info($"Starting long interval search...");
            finder = new IntervalsFinder(fitnessDataContainer, intervalContainer, powerZones, IntervalSeachGroups.Long, thresholds);
            saved = finder.Search();
            Log.Info($"Long intervals search done, {saved} saved");

            // Integrar intervalos
            IntervalsRefiner refiner = new IntervalsRefiner(intervalContainer, fitnessDataContainer);
            refiner.Refine();

            Log.Info($"Interval search completed. Found {intervalContainer.Intervals.Count} main intervals");
            return intervalContainer.Intervals;
        }

        private static bool IntervalAlreadyExists(Interval intervalToCheck, List<Interval> intervals)
        {
            return intervals.Any(x => IntervalsUtils.AreEqual(x, intervalToCheck));
        }
    }
}