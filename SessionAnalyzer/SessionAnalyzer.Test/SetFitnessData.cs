using CyclingTrainer.SessionReader.Models;
using CyclingTrainer.SessionAnalyzer.Test.Constants;
using CyclingTrainer.SessionAnalyzer.Test.Models;

namespace CyclingTrainer.SessionAnalyzer.Test
{
    internal static class FitnessDataService
    {
        internal static List<FitnessData> SetData(List<FitnessSection> fitnessTestSections)
        {
            List<FitnessData> fitnessData = new List<FitnessData>();
            DateTime startDate = FitnessDataCreation.DefaultStartDate;
            foreach (FitnessSection section in fitnessTestSections)
            {
                // Check if session has stopped
                if (section.Time == 0)
                {
                    startDate = startDate.AddSeconds(section.Power);        // Workaround to add stopped time to a session, the time is given with the power
                    continue;
                }
                for (int i = 0; i < section.Time; i++)
                    {
                        fitnessData.Add(new FitnessData
                        {
                            Timestamp = new Dynastream.Fit.DateTime(startDate),
                            Stats = new PointStats
                            {
                                Power = section.Power,
                                HeartRate = section.HearRate,
                                Cadence = section.Cadence
                            }
                        });
                        startDate = startDate.AddSeconds(1);
                    }
            }

            return fitnessData;
        }
    }
}