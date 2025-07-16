using CommonModels;
using SessionReader.Core.Repository;
using SessionAnalyzer.Core.Services;

namespace SessionAnalyzer.Console
{
    public static class Program
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Main()
        {
            Logger.Info("Start Session Analyzer Test");

            SessionRepository.AnalyzeRoute(@"Resources/19652409585_ACTIVITY.fit"); //aranguren 19652409585_ACTIVITY 19622171318_ACTIVITY 
            Session session = DataAnalyzeService.AnalyzeData();

            Logger.Info($"Analysis complete. AvrPower = {session.AnalyzedData.AveragePower}W. AvrHR = {session.AnalyzedData.AverageHr}bpm. AvrCadence = {session.AnalyzedData.AverageCadence}rpm");
        }
    }
}