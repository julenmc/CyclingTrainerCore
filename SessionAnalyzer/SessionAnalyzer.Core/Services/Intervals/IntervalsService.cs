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
            CoreModels.Zone zone = new CoreModels.Zone
            {
                HighLimit = powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Short] + 2)?.HighLimit ?? 0,
                LowLimit = powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Short])?.LowLimit ?? 0
            };
            IntervalsFinder finder = new IntervalsFinder(fitnessDataContainer, intervalContainer, powerZones, IntervalTimes.ShortWindowSize, zone, thresholds);
            List<Interval> tmp = finder.Search();
            Log.Info($"Short intervals search done. {tmp.Count} intervals found");
            intervalContainer.Intervals.AddRange(tmp);

            // Medium intervals
            Log.Info($"Starting medium interval search...");
            zone = new CoreModels.Zone
            {
                HighLimit = powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Medium] + 2)?.HighLimit ?? 0,
                LowLimit = powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Medium])?.LowLimit ?? 0
            };
            finder = new IntervalsFinder(fitnessDataContainer, intervalContainer, powerZones, IntervalTimes.MediumWindowSize, zone, thresholds);
            tmp = finder.Search();

            for (int i = 0; i < tmp.Count; i++)
            {
                if (IntervalAlreadyExists(tmp[i], intervalContainer.Intervals))
                {
                    Log.Debug($"Interval {tmp[i].StartTime.TimeOfDay}-{tmp[i].EndTime.TimeOfDay} at {tmp[i].AveragePower} already exists");
                    tmp.RemoveAt(i);
                }
            }
            Log.Info($"Medium intervals search done. {tmp.Count} new intervals found");
            intervalContainer.Intervals.AddRange(tmp);

            // Long intervals
            Log.Info($"Starting long interval search...");
            zone = new CoreModels.Zone
            {
                HighLimit = powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Long] + 2)?.HighLimit ?? 0,
                LowLimit = powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Long])?.LowLimit ?? 0
            };
            finder = new IntervalsFinder(fitnessDataContainer, intervalContainer, powerZones, IntervalTimes.LongWindowSize, zone, thresholds);
            tmp = finder.Search();
            for (int i = 0; i < tmp.Count; i++)
            {
                if (IntervalAlreadyExists(tmp[i], intervalContainer.Intervals))
                {
                    Log.Debug($"Interval {tmp[i].StartTime.TimeOfDay}-{tmp[i].StartTime.TimeOfDay} at {tmp[i].AveragePower} already exists");
                    tmp.RemoveAt(i);
                }
            }
            Log.Info($"Long intervals search done. {tmp.Count} new intervals found");
            intervalContainer.Intervals.AddRange(tmp);

            // Integrar intervalos
            IntervalsRefiner refiner = new IntervalsRefiner(intervalContainer, fitnessDataContainer, powerZones, thresholds);
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