using CoreModels = CyclingTrainer.Core.Models;
using CyclingTrainer.SessionReader.Repository;
using CyclingTrainer.SessionAnalyzer.Services;
using CyclingTrainer.SessionAnalyzer.Services.Intervals;
using Models = CyclingTrainer.SessionAnalyzer.Models;
using CyclingTrainer.Core.Models;

namespace CyclingTrainer.SessionAnalyzer.Console
{
    public static class Program
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Main()
        {
            Logger.Info("Start Session Analyzer Test");

            SessionRepository.ReadRoute(@"Resources/19622171318_ACTIVITY.fit"); //aranguren 19652409585_ACTIVITY 19622171318_ACTIVITY 
                                                                                // Session session = DataAnalyzeService.AnalyzeData();
            List<CoreModels.Zone> powerZones = new List<CoreModels.Zone>{
                new CoreModels.Zone { Id = 1, LowLimit = 0, HighLimit = 129},
                new CoreModels.Zone { Id = 2, LowLimit = 130, HighLimit = 189},
                new CoreModels.Zone { Id = 3, LowLimit = 190, HighLimit = 229},
                new CoreModels.Zone { Id = 4, LowLimit = 230, HighLimit = 264},
                new CoreModels.Zone { Id = 5, LowLimit = 265, HighLimit = 309},
                new CoreModels.Zone { Id = 6, LowLimit = 310, HighLimit = 379},
                new CoreModels.Zone { Id = 7, LowLimit = 380, HighLimit = 2000}
            };
            List<Models.Interval> intervals = IntervalsService.Search(SessionRepository.GetFitnessData(), powerZones);
            LogIntervals(intervals);

            // IntervalsService.DetectionThresholds thr = new IntervalsService.DetectionThresholds
            // {
            //     CvStartThreshold
            // };

            // Logger.Info($"Route {session.Name} lenght: {session.Distance}m. Elevation: {session.HeightDiff}m");
            // Logger.Info($"Analysis complete. AvrPower = {session.AnalyzedData.AveragePower}W. AvrHR = {session.AnalyzedData.AverageHr}bpm. AvrCadence = {session.AnalyzedData.AverageCadence}rpm");
            // Logger.Info($"Max power: {session.AnalyzedData.PowerCurve?[1].Power}W. 1 min max power: {session.AnalyzedData.PowerCurve?[60].Power}W. 5 min max power: {session.AnalyzedData.PowerCurve?[300].Power}W. 8 min max power: {session.AnalyzedData.PowerCurve?[480].Power}W. 20 min max power: {session.AnalyzedData.PowerCurve?[1200].Power}W");



            // string path = @"C:\Users\Embeblue\Documents\_temp\powers.csv";
            // using (CsvWriter writer = new CsvWriter(path))
            // {
            //     if (session.AnalyzedData.PowerCurve == null) return;
            //     foreach (var kvp in session.AnalyzedData.PowerCurve)
            //     {
            //         writer.WriteData(kvp.Key.ToString(), kvp.Value.Power.ToString());
            //     }
            // }
        }

        private static void LogIntervals(List<Models.Interval> intervals, int level = 0)
        {
            for (int i = 0; i < intervals.Count; i++)
            {
                Logger.Info($"{new string(' ', level * 2)}Interval {i}. Time: {intervals[i].StartTime.TimeOfDay}-{intervals[i].EndTime.TimeOfDay} ({intervals[i].TimeDiff} s), Power: {intervals[i].AveragePower} W");
                if (intervals[i].Intervals?.Count != 0)
                {
                    LogIntervals(intervals[i].Intervals, level + 1);
                }
            }
        }
    }
}