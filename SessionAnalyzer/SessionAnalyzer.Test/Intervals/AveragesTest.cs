using CyclingTrainer.SessionReader.Core.Models;
using CyclingTrainer.SessionAnalyzer.Test.Models;
using CyclingTrainer.SessionAnalyzer.Services.Intervals;

namespace CyclingTrainer.SessionAnalyzer.Test.Intervals
{
    [TestClass]
    public sealed class AveragesTest
    {
        List<FitnessData> _fitnessData = default!;

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
            _fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<AveragePowerModel> averages = AveragePowerCalculator.CalculateMovingAverages(_fitnessData, 10);
            Assert.AreEqual(120 * 4 - 9, averages.Count);       // -9 because the 10th already has 10 seconds for the calculation
            Assert.AreEqual(150, averages.First().AvrgPower);
            Assert.AreEqual(0, averages.First().Deviation);
            Assert.AreEqual(0, averages.First().MaxMinDelta);
        }
    }
}