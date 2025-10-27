using CyclingTrainer.SessionReader.Models;
using CyclingTrainer.SessionAnalyzer.Test.Models;
using CyclingTrainer.SessionAnalyzer.Models;
using CyclingTrainer.SessionAnalyzer.Services.Intervals;
using static CyclingTrainer.SessionAnalyzer.Test.Constants.FitnessDataCreation;
using static CyclingTrainer.SessionAnalyzer.Test.Intervals.IntervalsConstants;

namespace CyclingTrainer.SessionAnalyzer.Test.Intervals
{
    /// <summary>
    /// Contains the unit test of the <see cref="IntervalsRefiner"/> class.
    /// </summary>
    /// <remarks>
    /// This class verifies different scenarios of collision:
    /// Interval merges, divitions and integrations
    /// 
    /// The test follow the convention:
    /// <c>Scenario_ExpectedResult</c>.
    /// 
    /// The repository <see cref="FitnessDataContainer"/> is locked with <see cref="SetUp"/> before 
    /// each test and unlocked with <see cref="TestCleanup"/> after each test.
    /// </remarks>
    [TestClass]
    public sealed class RefinerUnitTests
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

        /// <summary>
        /// Verifies that two intervals that collide at start can be merged.
        /// </summary>
        /// <remarks>
        /// A default medium size interval and a defaut long size interval collide at start. Medium interval must 
        /// be merged into the long interval and detected as a sub-interval
        /// </remarks>
        [TestMethod]
        public void CollisionAtStart_Merged()
        {
            int intervalDelay = 60;
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = intervalDelay, Power = MediumDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = MediumDefaultTime - intervalDelay, Power = MediumDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = LongDefaultTime, Power = LongDefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer container = new FitnessDataContainer(fitnessData);

            DateTime longStart = DefaultStartDate.AddSeconds(intervalDelay);
            List<Interval> intervals = new List<Interval>()
            {
                new Interval()
                {
                    StartTime = DefaultStartDate,
                    EndTime = DefaultStartDate.AddSeconds(MediumDefaultTime - 1),
                    TimeDiff = MediumDefaultTime,
                    AveragePower = MediumDefaultPower
                },
                new Interval()
                {
                    StartTime = longStart,
                    EndTime = longStart.AddSeconds(LongDefaultTime + (MediumDefaultTime - intervalDelay) - 1),
                    TimeDiff = LongDefaultTime + (MediumDefaultTime - intervalDelay),
                    AveragePower = LongDefaultPower
                },
            };

            IntervalsRefiner refiner = new IntervalsRefiner(container, PowerZones);
            refiner.Refine(intervals);

            Assert.AreEqual(1, intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervals[0].StartTime);
            Assert.AreEqual(LongDefaultTime + MediumDefaultTime, intervals[0].TimeDiff);
            Assert.AreEqual(1, intervals[0].Intervals?.Count);
            Assert.AreEqual(DefaultStartDate, intervals[0].Intervals?[0].StartTime);
            Assert.AreEqual(MediumDefaultTime, intervals[0].Intervals?[0].TimeDiff);
        }

        /// <summary>
        /// Verifies that two intervals aren't always merged at start when they collide.
        /// </summary>
        /// <remarks>
        /// A default short size interval and a default long size interval collide at start. Short interval mustn't 
        /// be merged into the long interval; long interval should shortened
        /// </remarks>
        [TestMethod]
        public void CollisionAtStart_NotMerged()
        {
            int intervalDelay = 60;
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = intervalDelay, Power = ShortDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = ShortDefaultTime - intervalDelay, Power = ShortDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = LongDefaultTime, Power = LongDefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer container = new FitnessDataContainer(fitnessData);

            DateTime longStart = DefaultStartDate.AddSeconds(intervalDelay);
            List<Interval> intervals = new List<Interval>()
            {
                new Interval()
                {
                    StartTime = DefaultStartDate,
                    EndTime = DefaultStartDate.AddSeconds(ShortDefaultTime - 1),
                    TimeDiff = ShortDefaultTime,
                    AveragePower = ShortDefaultPower
                },
                new Interval()
                {
                    StartTime = longStart,
                    EndTime = longStart.AddSeconds(LongDefaultTime + (ShortDefaultTime - intervalDelay) - 1),
                    TimeDiff = LongDefaultTime + (ShortDefaultTime - intervalDelay),
                    AveragePower = LongDefaultPower
                },
            };

            IntervalsRefiner refiner = new IntervalsRefiner(container, PowerZones);
            refiner.Refine(intervals);

            Assert.AreEqual(2, intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervals[0].StartTime);
            Assert.AreEqual(ShortDefaultTime, intervals[0].TimeDiff);
            Assert.AreEqual(DefaultStartDate.AddSeconds(ShortDefaultTime), intervals[1].StartTime);
            Assert.AreEqual(LongDefaultTime - ShortDefaultTime, intervals[1].TimeDiff);
        }

        /// <summary>
        /// Verifies that two intervals that collide at the end can be merged.
        /// </summary>
        /// <remarks>
        /// A default medium size interval and a defaut long size interval collide at the end. Medium interval must 
        /// be merged into the long interval and detected as a sub-interval.
        /// </remarks>
        [TestMethod]
        public void CollisionAtEnd_Merged()
        {
            int intervalDelay = 60;
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = LongDefaultTime, Power = LongDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = MediumDefaultTime - intervalDelay, Power = MediumDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = MediumDefaultTime, Power = MediumDefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer container = new FitnessDataContainer(fitnessData);

            DateTime mediumStart = DefaultStartDate.AddSeconds(LongDefaultTime);
            List<Interval> intervals = new List<Interval>()
            {
                new Interval()
                {
                    StartTime = DefaultStartDate,
                    EndTime = DefaultStartDate.AddSeconds(LongDefaultTime + (MediumDefaultTime - intervalDelay) - 1),
                    TimeDiff = LongDefaultTime + (MediumDefaultTime - intervalDelay),
                    AveragePower = LongDefaultPower
                },
                new Interval()
                {
                    StartTime = mediumStart,
                    EndTime = mediumStart.AddSeconds(MediumDefaultTime - 1),
                    TimeDiff = MediumDefaultTime,
                    AveragePower = MediumDefaultPower
                },
            };

            IntervalsRefiner refiner = new IntervalsRefiner(container, PowerZones);
            refiner.Refine(intervals);

            Assert.AreEqual(1, intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervals[0].StartTime);
            Assert.AreEqual(LongDefaultTime + MediumDefaultTime, intervals[0].TimeDiff);
            Assert.AreEqual(1, intervals[0].Intervals?.Count);
            Assert.AreEqual(DefaultStartDate.AddSeconds(LongDefaultTime), intervals[0].Intervals?[0].StartTime);
            Assert.AreEqual(MediumDefaultTime, intervals[0].Intervals?[0].TimeDiff);
        }

        /// <summary>
        /// Verifies that two intervals aren't always merged when they collide at the end.
        /// </summary>
        /// <remarks>
        /// A default short size interval and a default long size interval collide at the end. Short interval mustn't 
        /// be merged into the long interval; long interval should shortened.
        /// </remarks>
        [TestMethod]
        public void CollisionAtEnd_NotMerged()
        {
            int intervalDelay = 60;
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = LongDefaultTime, Power = LongDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = ShortDefaultTime - intervalDelay, Power = ShortDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = ShortDefaultTime, Power = ShortDefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer container = new FitnessDataContainer(fitnessData);

            DateTime mediumStart = DefaultStartDate.AddSeconds(LongDefaultTime);
            List<Interval> intervals = new List<Interval>()
            {
                new Interval()
                {
                    StartTime = DefaultStartDate,
                    EndTime = DefaultStartDate.AddSeconds(LongDefaultTime + (ShortDefaultTime - intervalDelay) - 1),
                    TimeDiff = LongDefaultTime + (ShortDefaultTime - intervalDelay),
                    AveragePower = LongDefaultPower
                },
                new Interval()
                {
                    StartTime = mediumStart,
                    EndTime = mediumStart.AddSeconds(ShortDefaultTime - 1),
                    TimeDiff = ShortDefaultTime,
                    AveragePower = ShortDefaultPower
                },
            };

            IntervalsRefiner refiner = new IntervalsRefiner(container, PowerZones);
            refiner.Refine(intervals);

            Assert.AreEqual(2, intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervals[0].StartTime);
            Assert.AreEqual(LongDefaultTime - intervalDelay, intervals[0].TimeDiff);
            Assert.AreEqual(DefaultStartDate.AddSeconds(LongDefaultTime - intervalDelay), intervals[1].StartTime);
            Assert.AreEqual(ShortDefaultTime, intervals[1].TimeDiff);
        }
    }
}