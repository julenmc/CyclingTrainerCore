using CyclingTrainer.SessionReader.Core.Models;
using CyclingTrainer.SessionAnalyzer.Test.Models;
using CyclingTrainer.SessionAnalyzer.Core.Models;
using CyclingTrainer.SessionAnalyzer.Core.Services.Intervals;

namespace CyclingTrainer.SessionAnalyzer.Test.Intervals
{
    [TestClass]
    public sealed class IrregularIntervalsTest
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
        public void UniqueInterval()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>();
            for (int i = 0; i < 100; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 5, Power = 200, HearRate = 90, Cadence = 80 });
                fitnessTestSections.Add(new FitnessSection { Time = 5, Power = 210, HearRate = 90, Cadence = 80 });
            }
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, SprintPowerValue);
            Assert.AreEqual(1, intervals.Count);
            Assert.AreEqual(205, intervals.First().AveragePower);
            Assert.AreEqual(1000, intervals.First().TimeDiff);
        }

        [TestMethod]
        public void UniqueWithSprint()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = 200, Power = 300, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 10, Power = 700, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 200, Power = 300, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, SprintPowerValue);
            Assert.AreEqual(1, intervals.Count);
            Assert.AreEqual(300, intervals.First().AveragePower);
            Assert.AreEqual(410, intervals.First().TimeDiff);
        }

        [TestMethod]
        public void SmallDrop()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = 200, Power = 300, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 1, Power = 200, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 200, Power = 300, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, SprintPowerValue);
            Assert.AreEqual(1, intervals.Count);
            Assert.AreEqual(299.75, intervals.First().AveragePower, 0.01);
            Assert.AreEqual(401, intervals.First().TimeDiff);
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
            for (int i = 0; i < 100; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 5, Power = 200, HearRate = 90, Cadence = 80 });
                fitnessTestSections.Add(new FitnessSection { Time = 5, Power = 210, HearRate = 90, Cadence = 80 });
            }
            for (int i = 0; i < 50; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 5, Power = 150, HearRate = 90, Cadence = 80 });
                fitnessTestSections.Add(new FitnessSection { Time = 5, Power = 250, HearRate = 90, Cadence = 80 });
            }
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, SprintPowerValue);
            Assert.AreEqual(1, intervals.Count);
            Assert.AreEqual(205, intervals.First().AveragePower);
            Assert.AreEqual(1000, intervals.First().TimeDiff);
        }

        [TestMethod]
        public void ShortMediumShort()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>();
            // Add short
            for (int i = 0; i < 20; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 5, Power = 250, HearRate = 90, Cadence = 80 });
                fitnessTestSections.Add(new FitnessSection { Time = 5, Power = 260, HearRate = 90, Cadence = 80 });
            }
            // Add medium
            for (int i = 0; i < 50; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 5, Power = 200, HearRate = 90, Cadence = 80 });
                fitnessTestSections.Add(new FitnessSection { Time = 5, Power = 210, HearRate = 90, Cadence = 80 });
            }
            // Add short
            for (int i = 0; i < 20; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 5, Power = 250, HearRate = 90, Cadence = 80 });
                fitnessTestSections.Add(new FitnessSection { Time = 5, Power = 260, HearRate = 90, Cadence = 80 });
            }

            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, SprintPowerValue);
            Assert.AreEqual(3, intervals.Count);
            Assert.AreEqual(255, intervals[0].AveragePower);
            Assert.AreEqual(200, intervals[0].TimeDiff);
            Assert.AreEqual(205, intervals[1].AveragePower);
            Assert.AreEqual(500, intervals[1].TimeDiff);
            Assert.AreEqual(255, intervals[2].AveragePower);
            Assert.AreEqual(200, intervals[2].TimeDiff);
        }

        [TestMethod]
        public void MediumLongMedium()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>();
            // Add medium
            for (int i = 0; i < 50; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 5, Power = 250, HearRate = 90, Cadence = 80 });
                fitnessTestSections.Add(new FitnessSection { Time = 5, Power = 260, HearRate = 90, Cadence = 80 });
            }
            // Add medium
            for (int i = 0; i < 150; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 5, Power = 200, HearRate = 90, Cadence = 80 });
                fitnessTestSections.Add(new FitnessSection { Time = 5, Power = 210, HearRate = 90, Cadence = 80 });
            }
            // Add medium
            for (int i = 0; i < 50; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 5, Power = 250, HearRate = 90, Cadence = 80 });
                fitnessTestSections.Add(new FitnessSection { Time = 5, Power = 260, HearRate = 90, Cadence = 80 });
            }
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, SprintPowerValue);
            Assert.AreEqual(3, intervals.Count);
            Assert.AreEqual(255, intervals[0].AveragePower);
            Assert.AreEqual(500, intervals[0].TimeDiff);
            Assert.AreEqual(205, intervals[1].AveragePower);
            Assert.AreEqual(1500, intervals[1].TimeDiff);
            Assert.AreEqual(255, intervals[2].AveragePower);
            Assert.AreEqual(500, intervals[2].TimeDiff);  
        }
    }
}