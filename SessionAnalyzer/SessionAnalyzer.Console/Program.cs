using CyclingTrainer.Core.Models;
using CyclingTrainer.SessionReader.Core.Repository;
using CyclingTrainer.SessionAnalyzer.Core.Services;

namespace CyclingTrainer.SessionAnalyzer.Console
{
    public static class Program
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Main()
        {
            Logger.Info("Start Session Analyzer Test");

            SessionRepository.AnalyzeRoute(@"Resources/19622171318_ACTIVITY.fit"); //aranguren 19652409585_ACTIVITY 19622171318_ACTIVITY 
            Session session = DataAnalyzeService.AnalyzeData();

            Logger.Info($"Route {session.Name} lenght: {session.Distance}m. Elevation: {session.HeightDiff}m");
            Logger.Info($"Analysis complete. AvrPower = {session.AnalyzedData.AveragePower}W. AvrHR = {session.AnalyzedData.AverageHr}bpm. AvrCadence = {session.AnalyzedData.AverageCadence}rpm");
            Logger.Info($"Max power: {session.AnalyzedData.PowerCurve?[1].Power}W. 1 min max power: {session.AnalyzedData.PowerCurve?[60].Power}W. 5 min max power: {session.AnalyzedData.PowerCurve?[300].Power}W. 8 min max power: {session.AnalyzedData.PowerCurve?[480].Power}W. 20 min max power: {session.AnalyzedData.PowerCurve?[1200].Power}W");

            string path = @"C:\Users\Embeblue\Documents\_temp\powers.csv";
            using (CsvWriter writer = new CsvWriter(path))
            {
                if (session.AnalyzedData.PowerCurve == null) return;
                foreach (var kvp in session.AnalyzedData.PowerCurve)
                {
                    writer.WriteData(kvp.Key.ToString(), kvp.Value.Power.ToString());
                }
            }
        }
    }
}