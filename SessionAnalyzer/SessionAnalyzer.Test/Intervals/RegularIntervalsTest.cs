using CyclingTrainer.SessionReader.Core.Models;
using CyclingTrainer.SessionAnalyzer.Test.Models;
using CyclingTrainer.SessionAnalyzer.Core.Models;
using CyclingTrainer.SessionAnalyzer.Core.Services.Intervals;

namespace CyclingTrainer.SessionAnalyzer.Test.Intervals
{
    [TestClass]
    public sealed class RegularIntervalsTest
    {
        private const int SprintPowerValue = 600;

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
        public void NoIntervals()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>();
            for (int i = 0; i < 100; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 5, Power = 150, HearRate = 90, Cadence = 80 });
                fitnessTestSections.Add(new FitnessSection { Time = 5, Power = 250, HearRate = 90, Cadence = 80 });
            }
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, SprintPowerValue);
            Assert.AreEqual(0, intervals.Count);
        }

        [TestMethod]
        public void UniqueInterval()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = 360, Power = 150, HearRate = 120, Cadence = 85}
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, SprintPowerValue);
            Assert.AreEqual(1, intervals.Count);
            Assert.AreEqual(150, intervals.First().AveragePower);
            Assert.AreEqual(360, intervals.First().TimeDiff);
        }

        [TestMethod]
        public void NuleSimpleNule()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>();
            for (int i = 0; i < 50; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 5, Power = 150, HearRate = 90, Cadence = 80 });
                fitnessTestSections.Add(new FitnessSection { Time = 5, Power = 250, HearRate = 90, Cadence = 80 });
            }
            fitnessTestSections.Add(new FitnessSection { Time = 360, Power = 200, HearRate = 90, Cadence = 80 });
            for (int i = 0; i < 50; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 5, Power = 150, HearRate = 90, Cadence = 80 });
                fitnessTestSections.Add(new FitnessSection { Time = 5, Power = 250, HearRate = 90, Cadence = 80 });
            }
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, SprintPowerValue);
            Assert.AreEqual(1, intervals.Count);
            Assert.AreEqual(200, intervals.First().AveragePower);
            Assert.AreEqual(360, intervals.First().TimeDiff);
        }

        [TestMethod]
        public void ShortMediumShort()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = 60, Power = 300, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 300, Power = 200, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 60, Power = 300, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, SprintPowerValue);
            Assert.AreEqual(3, intervals.Count);
            Assert.AreEqual(300, intervals[0].AveragePower);
            Assert.AreEqual(60, intervals[0].TimeDiff);
            Assert.AreEqual(200, intervals[1].AveragePower);
            Assert.AreEqual(300, intervals[1].TimeDiff);
            Assert.AreEqual(300, intervals[2].AveragePower);
            Assert.AreEqual(60, intervals[2].TimeDiff);
        }

        [TestMethod]
        public void MediumLongMedium()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = 300, Power = 200, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 1200, Power = 240, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 300, Power = 200, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, SprintPowerValue);
            Assert.AreEqual(3, intervals.Count);
            Assert.AreEqual(200, intervals[0].AveragePower);
            Assert.AreEqual(300, intervals[0].TimeDiff);
            Assert.AreEqual(240, intervals[1].AveragePower);
            Assert.AreEqual(1200, intervals[1].TimeDiff);   
            Assert.AreEqual(200, intervals[2].AveragePower);
            Assert.AreEqual(300, intervals[2].TimeDiff);   
        }
    }
}