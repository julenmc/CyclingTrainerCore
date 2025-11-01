using CoreModels = CyclingTrainer.Core.Models;
using CyclingTrainer.SessionAnalyzer.Constants;
using CyclingTrainer.SessionAnalyzer.Models;
using CyclingTrainer.SessionReader.Models;
using NLog;
using CyclingTrainer.SessionAnalyzer.Enums;

namespace CyclingTrainer.SessionAnalyzer.Services.Intervals
{
    public class IntervalsService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private List<FitnessData> _activityPoints;
        private List<CoreModels.Zone> _powerZones;
        private IntervalGroupThresholds? _thresholds;

        public IntervalsService(List<FitnessData> activityPoints, List<CoreModels.Zone> powerZones, IntervalGroupThresholds? thresholds = null)
        {
            // Check arguments
            if (activityPoints.Count == 0)
            {
                throw new ArgumentException("Invalid activity points.");
            }
            _activityPoints = activityPoints;
            _powerZones = powerZones;
            _thresholds = thresholds;
        }

        public IntervalContainer Search()
        {
            Log.Info("Starting intervals search...");
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(_activityPoints);
            IntervalContainer intervalContainer = new IntervalContainer();

            // Detectar y eliminar sprints primero
            CoreModels.Zone? sprint = _powerZones.Find(x => x.Id == 7);
            if (sprint == null)
            {
                throw new ArgumentException("No sprint power zone found");
            }
            int sprintPower = sprint.LowLimit;
            Log.Info($"Starting sprint detection and removal at {sprintPower} W...");
            SprintService sprintService = new SprintService(fitnessDataContainer, IntervalTimes.SprintMinTime, sprintPower * 11 / 10, sprintPower);
            intervalContainer.Sprints.AddRange(sprintService.SearchSprints());
            Log.Info("Sprint detection completed");

            // Short intervals
            Log.Info($"Starting short interval search...");
            IntervalsFinder finder = new IntervalsFinder(fitnessDataContainer, intervalContainer, _powerZones, IntervalSeachGroups.Short, _thresholds?.Short);
            int saved = finder.Search();
            Log.Info($"Short intervals search done, {saved} saved");

            // Medium intervals
            Log.Info($"Starting medium interval search...");
            finder = new IntervalsFinder(fitnessDataContainer, intervalContainer, _powerZones, IntervalSeachGroups.Medium, _thresholds?.Medium);
            saved = finder.Search();
            Log.Info($"Medium intervals search done, {saved} saved");

            // Long intervals
            Log.Info($"Starting long interval search...");
            finder = new IntervalsFinder(fitnessDataContainer, intervalContainer, _powerZones, IntervalSeachGroups.Long, _thresholds?.Long);
            saved = finder.Search();
            Log.Info($"Long intervals search done, {saved} saved");

            // Integrar intervalos
            IntervalsRefiner refiner = new IntervalsRefiner(intervalContainer, fitnessDataContainer);
            refiner.Refine();

            Log.Info($"Interval search completed. Found {intervalContainer.Intervals.Count} main intervals");
            return intervalContainer;
        }

        public void SetThresholds(IntervalGroupThresholds thresholds)
        {
            // Check if thresholds are inside the limits
            bool CheckThresholds(Thresholds thresholds, IntervalThresholdValues limits)
            {
                return thresholds.CvStart >= limits.Min.CvStart && thresholds.CvStart <= limits.Max.CvStart &&
                       thresholds.CvFollow >= limits.Min.CvFollow && thresholds.CvFollow <= limits.Max.CvFollow &&
                       thresholds.Range >= limits.Min.Range && thresholds.Range <= limits.Max.Range &&
                       thresholds.MaRel >= limits.Min.MaRel && thresholds.MaRel <= limits.Max.MaRel;
            }
            if (!CheckThresholds(thresholds.Short, IntervalSearchValues.ShortIntervals))
            {
                throw new ArgumentException($"Invalid short thresholds: CvStart: {thresholds.Short.CvStart}, CvFollow: {thresholds.Short.CvFollow}, Range: {thresholds.Short.Range}, MaRel: {thresholds.Short.MaRel}");
            }
            if (!CheckThresholds(thresholds.Medium, IntervalSearchValues.MediumIntervals))
            {
                throw new ArgumentException($"Invalid medium thresholds: CvStart: {thresholds.Medium.CvStart}, CvFollow: {thresholds.Medium.CvFollow}, Range: {thresholds.Medium.Range}, MaRel: {thresholds.Medium.MaRel}");
            }
            if (!CheckThresholds(thresholds.Long, IntervalSearchValues.LongIntervals))
            {
                throw new ArgumentException($"Invalid long thresholds: CvStart: {thresholds.Long.CvStart}, CvFollow: {thresholds.Long.CvFollow}, Range: {thresholds.Long.Range}, MaRel: {thresholds.Long.MaRel}");
            }
            _thresholds = thresholds;
            Log.Info("Thresholds changed!");
        }
    }
}