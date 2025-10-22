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

            // Limpiar el repositorio para el nuevo análisis
            IntervalRepository.Clear();
            IntervalRepository.SetFitnessData(activityPoints);

            // Detectar y eliminar sprints primero
            CoreModels.Zone? sprint = powerZones.Find(x => x.Id == 7);
            if (sprint == null)
            {
                Log.Warn("No sprint power zone found");
                return new List<Interval>();
            }
            int sprintPower = sprint.LowLimit;
            Log.Info($"Starting sprint detection and removal at {sprintPower} W...");
            SprintService.SetConfiguration(5, sprintPower * 11 / 10, sprintPower);
            SprintService.AnalyzeActivity(activityPoints);
            Log.Info("Sprint detection completed");

            // Short intervals
            List<Interval> intervals = new List<Interval>();
            Log.Info($"Starting short interval search...");
            CoreModels.Zone zone = new CoreModels.Zone
            {
                HighLimit = powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Short] + 2)?.HighLimit ?? 0,
                LowLimit = powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Short])?.LowLimit ?? 0
            };
            IntervalsFinder finder = new IntervalsFinder(powerZones, AveragePowerCalculator.ShortWindowSize, zone, thresholds);
            List<Interval> tmp = finder.Search();
            Log.Info($"Short intervals search done. {tmp.Count} intervals found");
            intervals.AddRange(tmp);

            // Medium intervals
            Log.Info($"Starting medium interval search...");
            zone = new CoreModels.Zone
            {
                HighLimit = powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Medium] + 2)?.HighLimit ?? 0,
                LowLimit = powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Medium])?.LowLimit ?? 0
            };
            finder = new IntervalsFinder(powerZones, AveragePowerCalculator.MediumWindowSize, zone, thresholds);
            tmp = finder.Search();

            for (int i = 0; i < tmp.Count; i++)
            {
                if (IntervalAlreadyExists(tmp[i], intervals))
                {
                    Log.Debug($"Interval {tmp[i].StartTime.TimeOfDay}-{tmp[i].EndTime.TimeOfDay} at {tmp[i].AveragePower} already exists");
                    tmp.RemoveAt(i);
                }
            }
            Log.Info($"Medium intervals search done. {tmp.Count} new intervals found");
            intervals.AddRange(tmp);

            // Long intervals
            Log.Info($"Starting long interval search...");
            zone = new CoreModels.Zone
            {
                HighLimit = powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Long] + 2)?.HighLimit ?? 0,
                LowLimit = powerZones.Find(x => x.Id == IntervalZones.IntervalMinZones[IntervalGroups.Long])?.LowLimit ?? 0
            };
            finder = new IntervalsFinder(powerZones, AveragePowerCalculator.LongWindowSize, zone, thresholds);
            tmp = finder.Search();
            for (int i = 0; i < tmp.Count; i++)
            {
                if (IntervalAlreadyExists(tmp[i], intervals))
                {
                    Log.Debug($"Interval {tmp[i].StartTime.TimeOfDay}-{tmp[i].StartTime.TimeOfDay} at {tmp[i].AveragePower} already exists");
                    tmp.RemoveAt(i);
                }
            }
            Log.Info($"Long intervals search done. {tmp.Count} new intervals found");
            intervals.AddRange(tmp);

            // Integrar intervalos
            IntervalsCleaner cleaner = new IntervalsCleaner(powerZones, thresholds);
            cleaner.Clean(ref intervals);

            Log.Info($"Interval search completed. Found {intervals.Count} main intervals");
            intervals.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
            return intervals;
        }

        private static bool IntervalAlreadyExists(Interval intervalToCheck, List<Interval> intervals)
        {
            return intervals.Any(x => IntervalsUtils.AreEqual(x, intervalToCheck));
        }
    }
}