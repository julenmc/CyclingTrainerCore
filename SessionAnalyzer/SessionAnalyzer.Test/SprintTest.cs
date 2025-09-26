using CyclingTrainer.SessionAnalyzer.Core.Models;
using CyclingTrainer.SessionAnalyzer.Core.Services.Intervals;
using CyclingTrainer.SessionReader.Core.Models;

namespace CyclingTrainer.SessionAnalyzer.Test
{
    [TestClass]
    public sealed class SprintTest
    {
        List<FitnessData> fitnessData = default!;
        List<FitnessSection> fitnessTestSections = new List<FitnessSection>
        {
            new FitnessSection{ Time = 10, Power = 150, HearRate = 120, Cadence = 85},
            new FitnessSection{ Time = 5, Power = 600, HearRate = 150, Cadence = 90},
            new FitnessSection{ Time = 5, Power = 550, HearRate = 150, Cadence = 90},
            new FitnessSection{ Time = 10, Power = 150, HearRate = 180, Cadence = 95},
        };

        [TestInitialize]
        public void SetUp()
        {
            fitnessData = new List<FitnessData>();
            DateTime startDate = new DateTime(2025, 07, 14, 12, 00, 00);
            foreach (FitnessSection section in fitnessTestSections)
            {
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
        }

        [TestMethod]
        public void NormalSprint()
        {
            SprintService.SetConfiguration(5, 550, 500);
            SprintService.AnalyzeActivity(fitnessData);
            List<Sprint> sprints = IntervalRepository.GetSprints();
            Assert.AreEqual(1, sprints.Count);
            Assert.AreEqual(575, sprints.First().AveragePower);
            List<FitnessData> remainingPoints = IntervalRepository.GetRemainingFitnessData();
            Assert.AreEqual(20, remainingPoints.Count);
        }
    }
}