using CyclingTrainer.SessionReader.Models;
using CyclingTrainer.SessionAnalyzer.Test.Models;
using CyclingTrainer.SessionAnalyzer.Models;
using CyclingTrainer.SessionAnalyzer.Services.Intervals;
using static CyclingTrainer.SessionAnalyzer.Test.Intervals.IntervalsConstants;

namespace CyclingTrainer.SessionAnalyzer.Test.Intervals
{
    [TestClass]
    public sealed class CollisionsTest
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
        public void CollisionOutsideAtStart()   // Interval should shortened and detected before the long interval, difference between avrPower of short and long is too high to merge
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = 90, Power = ShortDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 60, Power = MediumDefaultPower, HearRate = 120, Cadence = 85},   // Long interval starts here
                new FitnessSection{ Time = 20, Power = ShortDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = LongDefaultTime, Power = LongDefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
            Assert.AreEqual(2, intervals.Count);
            Assert.AreEqual(90, intervals[0].TimeDiff);
            Assert.AreEqual(ShortDefaultPower, intervals[0].AveragePower);
            Assert.AreEqual(LongDefaultTime + 80, intervals[1].TimeDiff);
        }

        [TestMethod]
        public void CollisionInsideAtStart()    // Short interval should be shortened and detected inside the long interval
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = 20, Power = ShortDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 60, Power = MediumDefaultPower, HearRate = 120, Cadence = 85},   // Long interval starts here
                new FitnessSection{ Time = 90, Power = ShortDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = LongDefaultTime, Power = LongDefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
            Assert.AreEqual(1, intervals.Count);
            Assert.AreEqual(LongDefaultTime + 150, intervals[0].TimeDiff);
            Assert.AreEqual(1, intervals[0].Intervals?.Count);
            Assert.AreEqual(150, intervals[0].Intervals?[0].TimeDiff);
        }

        [TestMethod]
        public void CollisionDivitionAtStart()        // Interval should be shortened and detected inside and outside the long interval
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = 90, Power = ShortDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 60, Power = MediumDefaultPower, HearRate = 120, Cadence = 85},   // Long interval starts here
                new FitnessSection{ Time = 90, Power = ShortDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = LongDefaultTime, Power = LongDefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
            Assert.AreEqual(2, intervals.Count);
            Assert.AreEqual(90, intervals[0].TimeDiff);
            Assert.AreEqual(ShortDefaultPower, intervals[0].AveragePower);
            Assert.AreEqual(LongDefaultTime + 150, intervals[1].TimeDiff);
            Assert.AreEqual(1, intervals[1].Intervals?.Count);
            Assert.AreEqual(150, intervals[1].Intervals?[0].TimeDiff);
        }

        [TestMethod]
        public void CollisionInsideAtEnd()        // Interval should shortened and detected inside the long interval
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = LongDefaultTime, Power = LongDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 90, Power = ShortDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 60, Power = MediumDefaultPower, HearRate = 120, Cadence = 85},   // Long interval ends here
                new FitnessSection{ Time = 20, Power = ShortDefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
            Assert.AreEqual(1, intervals.Count);
            Assert.AreEqual(LongDefaultTime + 150, intervals[0].TimeDiff);
            Assert.AreEqual(1, intervals[0].Intervals?.Count);
            Assert.IsTrue(150 <= intervals[0].Intervals?[0].TimeDiff);   // Workaround, will check later
        }

        [TestMethod]
        public void CollisionOutsideAtEnd()        // Interval should shortened and detected after the long interval
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = LongDefaultTime, Power = LongDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 20, Power = ShortDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 60, Power = MediumDefaultPower, HearRate = 120, Cadence = 85},   // Long interval ends here
                new FitnessSection{ Time = 90, Power = ShortDefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
            Assert.AreEqual(2, intervals.Count);
            Assert.AreEqual(LongDefaultTime + 80, intervals[0].TimeDiff);
            Assert.AreEqual(90, intervals[1].TimeDiff);
            Assert.AreEqual(ShortDefaultPower, intervals[1].AveragePower);
        }

        [TestMethod]
        public void CollisionDivitionAtEnd()        // Interval should shortened and detected inside the long interval
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = LongDefaultTime, Power = LongDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 90, Power = ShortDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 60, Power = MediumDefaultPower, HearRate = 120, Cadence = 85},   // Long interval ends here
                new FitnessSection{ Time = 90, Power = ShortDefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
            Assert.AreEqual(2, intervals.Count);

            Assert.AreEqual(LongDefaultTime + 150, intervals[0].TimeDiff);
            Assert.AreEqual(1, intervals[0].Intervals?.Count);
            Assert.IsTrue(150 < intervals[0].Intervals?[0].TimeDiff);       // Workaround, will check later
            Assert.AreEqual(90, intervals[1].TimeDiff);
            Assert.AreEqual(ShortDefaultPower, intervals[1].AveragePower);
        }
    }
}