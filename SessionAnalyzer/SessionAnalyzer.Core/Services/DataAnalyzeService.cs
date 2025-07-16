using CommonModels;
using NLog;
using SessionReader.Core.Models;
using SessionReader.Core.Repository;

namespace SessionAnalyzer.Core.Services
{
    public static class DataAnalyzeService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private const int MaxTimeDiff = 3000;

        public static Session AnalyzeData()
        {
            List<FitnessData> fitnessData = SessionRepository.GetFitnessData();
            double totalMiliSeconds = 0;
            double totalPower = 0;
            double totalHr = 0;
            double totalCadence = 0;
            for (int i = 0; i < fitnessData.Count - 1; i++)
            {
                double timeDiff = fitnessData[i+1].Timestamp.GetDateTime().Subtract(fitnessData[i].Timestamp.GetDateTime()).TotalMilliseconds;
                if (timeDiff > MaxTimeDiff)
                {
                    Log.Warn($"Time difference between records {i} and {i + 1} is too high: {timeDiff} ms. This may cause incorrect average values.");
                    continue;
                }
                totalMiliSeconds += timeDiff;
                totalPower += (double)(fitnessData[i].Stats.Power ?? 0) * timeDiff;
                totalHr += (double)(fitnessData[i].Stats.HeartRate ?? 0) * timeDiff;
                totalCadence += (double)(fitnessData[i].Stats.Cadence ?? 0) * timeDiff;
            }

            AnalyzedData data = new AnalyzedData();
            data.AveragePower = (int)Math.Round(totalPower / totalMiliSeconds);
            data.AverageHr = (int)Math.Round(totalHr / totalMiliSeconds);
            data.AverageCadence = (int)Math.Round(totalCadence / totalMiliSeconds);
            SessionRepository.UpdateAnalyzedData(data);

            return SessionRepository.GetSession();
        }
    }
}
