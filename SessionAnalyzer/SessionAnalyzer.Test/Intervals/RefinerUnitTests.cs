using CyclingTrainer.SessionReader.Models;
using CyclingTrainer.SessionAnalyzer.Test.Models;
using CyclingTrainer.SessionAnalyzer.Models;
using CyclingTrainer.SessionAnalyzer.Services.Intervals;
using static CyclingTrainer.SessionAnalyzer.Test.Constants.FitnessDataCreation;
using static CyclingTrainer.SessionAnalyzer.Test.Intervals.IntervalsTestConstants;

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
    /// </remarks>
    [TestClass]
    public sealed class RefinerUnitTests
    {
        /// <summary>
        /// Verifies that two intervals don't collide, they stay the same.
        /// </summary>
        /// <remarks>
        /// A default medium size interval and a defaut long size interval divided by a rest period.
        /// </remarks>
        [TestMethod]
        public void NoCollision_Nothing()
        {
            int restTime = 100;
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime, Power = MediumIntervalValues.MinPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = restTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = LongIntervalValues.DefaultTime, Power = LongIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);

            DateTime longStart = DefaultStartDate.AddSeconds(MediumIntervalValues.DefaultTime + restTime);
            IntervalContainer intervalContainer = new IntervalContainer();
            intervalContainer.Intervals = new List<Interval>()
            {
                new Interval()
                {
                    StartTime = DefaultStartDate,
                    EndTime = DefaultStartDate.AddSeconds(MediumIntervalValues.DefaultTime - 1),
                    TimeDiff = MediumIntervalValues.DefaultTime,
                    AveragePower = MediumIntervalValues.MinPower
                },
                new Interval()
                {
                    StartTime = longStart,
                    EndTime = longStart.AddSeconds(LongIntervalValues.DefaultTime),
                    TimeDiff = LongIntervalValues.DefaultTime,
                    AveragePower = LongIntervalValues.MaxPower
                },
            };

            IntervalsRefiner refiner = new IntervalsRefiner(intervalContainer, fitnessDataContainer);
            refiner.Refine();

            Assert.AreEqual(2, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(MediumIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(longStart, intervalContainer.Intervals[1].StartTime);
            Assert.AreEqual(LongIntervalValues.DefaultTime, intervalContainer.Intervals[1].TimeDiff);
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
                new FitnessSection{ Time = intervalDelay, Power = MediumIntervalValues.MinPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime - intervalDelay, Power = MediumIntervalValues.MinPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = LongIntervalValues.DefaultTime, Power = LongIntervalValues.MaxPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);

            DateTime longStart = DefaultStartDate.AddSeconds(intervalDelay);
            IntervalContainer intervalContainer = new IntervalContainer();
            intervalContainer.Intervals = new List<Interval>()
            {
                new Interval()
                {
                    StartTime = DefaultStartDate,
                    EndTime = DefaultStartDate.AddSeconds(MediumIntervalValues.DefaultTime - 1),
                    TimeDiff = MediumIntervalValues.DefaultTime,
                    AveragePower = MediumIntervalValues.MinPower
                },
                new Interval()
                {
                    StartTime = longStart,
                    EndTime = longStart.AddSeconds(LongIntervalValues.DefaultTime + (MediumIntervalValues.DefaultTime - intervalDelay) - 1),
                    TimeDiff = LongIntervalValues.DefaultTime + (MediumIntervalValues.DefaultTime - intervalDelay),
                    AveragePower = LongIntervalValues.MaxPower
                },
            };

            IntervalsRefiner refiner = new IntervalsRefiner(intervalContainer, fitnessDataContainer);
            refiner.Refine();

            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(LongIntervalValues.DefaultTime + MediumIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(1, intervalContainer.Intervals[0].Intervals?.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].Intervals?[0].StartTime);
            Assert.AreEqual(MediumIntervalValues.DefaultTime, intervalContainer.Intervals[0].Intervals?[0].TimeDiff);
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
                new FitnessSection{ Time = intervalDelay, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = ShortIntervalValues.DefaultTime - intervalDelay, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = LongIntervalValues.DefaultTime, Power = LongIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);

            DateTime longStart = DefaultStartDate.AddSeconds(intervalDelay);
            IntervalContainer intervalContainer = new IntervalContainer();
            intervalContainer.Intervals = new List<Interval>()
            {
                new Interval()
                {
                    StartTime = DefaultStartDate,
                    EndTime = DefaultStartDate.AddSeconds(ShortIntervalValues.DefaultTime - 1),
                    TimeDiff = ShortIntervalValues.DefaultTime,
                    AveragePower = ShortIntervalValues.DefaultPower
                },
                new Interval()
                {
                    StartTime = longStart,
                    EndTime = longStart.AddSeconds(LongIntervalValues.DefaultTime + (ShortIntervalValues.DefaultTime - intervalDelay) - 1),
                    TimeDiff = LongIntervalValues.DefaultTime + (ShortIntervalValues.DefaultTime - intervalDelay),
                    AveragePower = LongIntervalValues.DefaultPower
                },
            };

            IntervalsRefiner refiner = new IntervalsRefiner(intervalContainer, fitnessDataContainer);
            refiner.Refine();

            Assert.AreEqual(2, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(ShortIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(DefaultStartDate.AddSeconds(ShortIntervalValues.DefaultTime), intervalContainer.Intervals[1].StartTime);
            Assert.AreEqual(LongIntervalValues.DefaultTime, intervalContainer.Intervals[1].TimeDiff);
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
                new FitnessSection{ Time = LongIntervalValues.DefaultTime, Power = LongIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime - intervalDelay, Power = MediumIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime, Power = MediumIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);

            DateTime mediumStart = DefaultStartDate.AddSeconds(LongIntervalValues.DefaultTime);
            IntervalContainer intervalContainer = new IntervalContainer();
            intervalContainer.Intervals = new List<Interval>()
            {
                new Interval()
                {
                    StartTime = DefaultStartDate,
                    EndTime = DefaultStartDate.AddSeconds(LongIntervalValues.DefaultTime + (MediumIntervalValues.DefaultTime - intervalDelay) - 1),
                    TimeDiff = LongIntervalValues.DefaultTime + (MediumIntervalValues.DefaultTime - intervalDelay),
                    AveragePower = LongIntervalValues.DefaultPower
                },
                new Interval()
                {
                    StartTime = mediumStart,
                    EndTime = mediumStart.AddSeconds(MediumIntervalValues.DefaultTime - 1),
                    TimeDiff = MediumIntervalValues.DefaultTime,
                    AveragePower = MediumIntervalValues.DefaultPower
                },
            };

            IntervalsRefiner refiner = new IntervalsRefiner(intervalContainer, fitnessDataContainer);
            refiner.Refine();

            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(LongIntervalValues.DefaultTime + MediumIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(1, intervalContainer.Intervals[0].Intervals?.Count);
            Assert.AreEqual(DefaultStartDate.AddSeconds(LongIntervalValues.DefaultTime), intervalContainer.Intervals[0].Intervals?[0].StartTime);
            Assert.AreEqual(MediumIntervalValues.DefaultTime, intervalContainer.Intervals[0].Intervals?[0].TimeDiff);
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
                new FitnessSection{ Time = LongIntervalValues.DefaultTime, Power = LongIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = ShortIntervalValues.DefaultTime - intervalDelay, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = ShortIntervalValues.DefaultTime, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);

            DateTime mediumStart = DefaultStartDate.AddSeconds(LongIntervalValues.DefaultTime);
            IntervalContainer intervalContainer = new IntervalContainer();
            intervalContainer.Intervals = new List<Interval>()
            {
                new Interval()
                {
                    StartTime = DefaultStartDate,
                    EndTime = DefaultStartDate.AddSeconds(LongIntervalValues.DefaultTime + (ShortIntervalValues.DefaultTime - intervalDelay) - 1),
                    TimeDiff = LongIntervalValues.DefaultTime + (ShortIntervalValues.DefaultTime - intervalDelay),
                    AveragePower = LongIntervalValues.DefaultPower
                },
                new Interval()
                {
                    StartTime = mediumStart,
                    EndTime = mediumStart.AddSeconds(ShortIntervalValues.DefaultTime - 1),
                    TimeDiff = ShortIntervalValues.DefaultTime,
                    AveragePower = ShortIntervalValues.DefaultPower
                },
            };

            IntervalsRefiner refiner = new IntervalsRefiner(intervalContainer, fitnessDataContainer);
            refiner.Refine();

            Assert.AreEqual(2, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(LongIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(DefaultStartDate.AddSeconds(LongIntervalValues.DefaultTime), intervalContainer.Intervals[1].StartTime);
            Assert.AreEqual(ShortIntervalValues.DefaultTime, intervalContainer.Intervals[1].TimeDiff);
        }

        /// <summary>
        /// Verifies that when one interval is inside another, the short one is integrated in the long one
        /// </summary>
        /// <remarks>
        /// A default medium size interval is inside a default long size interval. Medium interval must end inside 
        /// the long interval.
        /// </remarks>
        [TestMethod]
        public void CollisionInside_Integrated()
        {
            int intervalDelay = 60;
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = intervalDelay, Power = LongIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime, Power = MediumIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = LongIntervalValues.DefaultTime - (intervalDelay + MediumIntervalValues.DefaultTime), Power = LongIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);

            DateTime mediumStart = DefaultStartDate.AddSeconds(intervalDelay);
            IntervalContainer intervalContainer = new IntervalContainer();
            intervalContainer.Intervals = new List<Interval>()
            {
                new Interval()
                {
                    StartTime = DefaultStartDate,
                    EndTime = DefaultStartDate.AddSeconds(LongIntervalValues.DefaultTime),
                    TimeDiff = LongIntervalValues.DefaultTime,
                    AveragePower = LongIntervalValues.DefaultPower
                },
                new Interval()
                {
                    StartTime = mediumStart,
                    EndTime = mediumStart.AddSeconds(MediumIntervalValues.DefaultTime),
                    TimeDiff = MediumIntervalValues.DefaultTime,
                    AveragePower = MediumIntervalValues.DefaultPower
                },
            };

            IntervalsRefiner refiner = new IntervalsRefiner(intervalContainer, fitnessDataContainer);
            refiner.Refine();

            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(LongIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(1, intervalContainer.Intervals[0].Intervals?.Count);
            Assert.AreEqual(mediumStart, intervalContainer.Intervals[0].Intervals?[0].StartTime);
            Assert.AreEqual(MediumIntervalValues.DefaultTime, intervalContainer.Intervals[0].Intervals?[0].TimeDiff);
        }

        /// <summary>
        /// Verifies that when one interval is inside another, with the same start time,
        /// the short one is integrated in the long one
        /// </summary>
        /// <remarks>
        /// A default medium size interval is inside a default long size interval, both with the same
        /// start time. Medium interval must end inside the long interval.
        /// </remarks>
        [TestMethod]
        public void SameStartTime_Integrated()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime, Power = MediumIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = LongIntervalValues.DefaultTime - (MediumIntervalValues.DefaultTime), Power = LongIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);

            DateTime mediumStart = DefaultStartDate;
            IntervalContainer intervalContainer = new IntervalContainer();
            intervalContainer.Intervals = new List<Interval>()
            {
                new Interval()
                {
                    StartTime = DefaultStartDate,
                    EndTime = DefaultStartDate.AddSeconds(LongIntervalValues.DefaultTime),
                    TimeDiff = LongIntervalValues.DefaultTime,
                    AveragePower = LongIntervalValues.DefaultPower
                },
                new Interval()
                {
                    StartTime = mediumStart,
                    EndTime = mediumStart.AddSeconds(MediumIntervalValues.DefaultTime),
                    TimeDiff = MediumIntervalValues.DefaultTime,
                    AveragePower = MediumIntervalValues.DefaultPower
                },
            };

            IntervalsRefiner refiner = new IntervalsRefiner(intervalContainer, fitnessDataContainer);
            refiner.Refine();

            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(LongIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(1, intervalContainer.Intervals[0].Intervals?.Count);
            Assert.AreEqual(mediumStart, intervalContainer.Intervals[0].Intervals?[0].StartTime);
            Assert.AreEqual(MediumIntervalValues.DefaultTime, intervalContainer.Intervals[0].Intervals?[0].TimeDiff);
        }

        /// <summary>
        /// Verifies that when one interval is inside another, the short one is integrated in the long one
        /// </summary>
        /// <remarks>
        /// A default short size interval is inside a medium size interval, which is also inside a default long size interval. 
        /// Short interval must end inside the medium interval, which must end inside the long interval.
        /// </remarks>
        [TestMethod]
        public void MultipleCollisionInside_Integrated()
        {
            int intervalDelay = 60;
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = intervalDelay, Power = LongIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = intervalDelay, Power = MediumIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = ShortIntervalValues.DefaultTime, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime - (intervalDelay + MediumIntervalValues.DefaultTime), Power = MediumIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = LongIntervalValues.DefaultTime - (intervalDelay + MediumIntervalValues.DefaultTime), Power = LongIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);

            DateTime mediumStart = DefaultStartDate.AddSeconds(intervalDelay);
            DateTime shortStart = DefaultStartDate.AddSeconds(intervalDelay * 2);
            IntervalContainer intervalContainer = new IntervalContainer();
            intervalContainer.Intervals = new List<Interval>()
            {
                new Interval()
                {
                    StartTime = DefaultStartDate,
                    EndTime = DefaultStartDate.AddSeconds(LongIntervalValues.DefaultTime),
                    TimeDiff = LongIntervalValues.DefaultTime,
                    AveragePower = LongIntervalValues.DefaultPower
                },
                new Interval()
                {
                    StartTime = mediumStart,
                    EndTime = mediumStart.AddSeconds(MediumIntervalValues.DefaultTime),
                    TimeDiff = MediumIntervalValues.DefaultTime,
                    AveragePower = MediumIntervalValues.DefaultPower
                },
                new Interval()
                {
                    StartTime = shortStart,
                    EndTime = shortStart.AddSeconds(ShortIntervalValues.DefaultTime),
                    TimeDiff = ShortIntervalValues.DefaultTime,
                    AveragePower = ShortIntervalValues.DefaultPower
                },
            };

            IntervalsRefiner refiner = new IntervalsRefiner(intervalContainer, fitnessDataContainer);
            refiner.Refine();

            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(LongIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(1, intervalContainer.Intervals[0].Intervals?.Count);
            Assert.AreEqual(mediumStart, intervalContainer.Intervals[0].Intervals?[0].StartTime);
            Assert.AreEqual(MediumIntervalValues.DefaultTime, intervalContainer.Intervals[0].Intervals?[0].TimeDiff);
            Assert.AreEqual(1, intervalContainer.Intervals[0].Intervals?[0].Intervals?.Count);
            Assert.AreEqual(shortStart, intervalContainer.Intervals?[0].Intervals?[0].Intervals?[0].StartTime);
            Assert.AreEqual(ShortIntervalValues.DefaultTime, intervalContainer.Intervals?[0].Intervals?[0].Intervals?[0].TimeDiff);
        }
    }
}