using CyclingTrainer.SessionReader.Models;
using CyclingTrainer.SessionAnalyzer.Test.Models;
using CyclingTrainer.SessionAnalyzer.Services.Intervals;

namespace CyclingTrainer.SessionAnalyzer.Test.Intervals
{
    [TestClass]
    public sealed class AveragesTest
    {
        [TestInitialize]
        public void SetUp()
        {
            Monitor.Enter(LockClass.LockObject);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Monitor.Exit(LockClass.LockObject);
        }

        [TestMethod]
        public void AverageTest()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = 120, Power = 150, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 120, Power = 250, HearRate = 150, Cadence = 90},
                new FitnessSection{ Time = 120, Power = 250, HearRate = 150, Cadence = 90},
                new FitnessSection{ Time = 120, Power = 150, HearRate = 180, Cadence = 95},
            };
            List<FitnessData> _fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<AveragePowerModel> averages = AveragePowerCalculator.CalculateMovingAverages(_fitnessData, 10, new IntervalContainer());
            Assert.AreEqual(120 * 4 - 9, averages.Count);       // -9 because the 10th already has 10 seconds for the calculation
            Assert.AreEqual(150, averages.First().AvrgPower);
            Assert.AreEqual(0, averages.First().Deviation);
            Assert.AreEqual(0, averages.First().MaxMinDelta);
        }

        [TestMethod]
        public void WithPauses()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = 20, Power = 150, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 0, Power = 20, HearRate = 0, Cadence = 0},       // 20 second session stop
                new FitnessSection{ Time = 20, Power = 250, HearRate = 150, Cadence = 90},
            };
            List<FitnessData> _fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<AveragePowerModel> averages = AveragePowerCalculator.CalculateMovingAverages(_fitnessData, 10, new IntervalContainer());
            Assert.AreEqual((20 - 9) * 2, averages.Count);       // Two blocks, the first 20 seconds and the last 2. The first 10 seconds of the second block should not be included
            Assert.AreEqual(new DateTime(2025, 07, 14, 12, 00, 49), averages[11].PointDate);
            Assert.AreEqual(250, averages[11].AvrgPower);
        }
    }
}