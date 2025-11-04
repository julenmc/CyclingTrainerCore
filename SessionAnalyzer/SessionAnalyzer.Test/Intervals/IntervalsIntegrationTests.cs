using CoreModels = CyclingTrainer.Core.Models;
using CyclingTrainer.SessionReader.Models;
using CyclingTrainer.SessionAnalyzer.Test.Models;
using CyclingTrainer.SessionAnalyzer.Constants;
using CyclingTrainer.SessionAnalyzer.Models;
using CyclingTrainer.SessionAnalyzer.Services.Intervals;
using static CyclingTrainer.SessionAnalyzer.Test.Constants.FitnessDataCreation;
using static CyclingTrainer.SessionAnalyzer.Test.Intervals.IntervalsTestConstants;
using NLog;

namespace CyclingTrainer.SessionAnalyzer.Test.Intervals
{
    /// <summary>
    /// Contains the integration test of the <see cref="IntervalsService"/> class.
    /// </summary>
    /// <remarks>
    /// This class tests different iterations between the three classes used by <see cref="IntervalsService"/>:
    /// <see cref="SprintService"/>, <see cref="IntervalsFinder"/> and <see cref="IntervalsRefiner"/>.
    /// 
    /// The tests follow the convention:
    /// <c>Scenario_ExpectedResult</c>.
    /// The assertion of the expected result will allways contain the interval count; and for each interval:
    /// interval start date, time, average power and, if necessary, sub-interval count.
    /// </remarks>
    [TestClass]
    public sealed class IntervalsIntegrationTest
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Verifies that no interval is found.
        /// </summary>
        [TestMethod]
        [TestCategory("Integration")]
        public void NoIntervals_NotFound()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>()
            {
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.MaxPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            IntervalsService service = new IntervalsService(fitnessData, PowerZones);
            IntervalContainer container = service.Search();

            // Assertions
            Assert.AreEqual(0, container.Intervals.Count);
        }

        /// <summary>
        /// Verifies that a single short size interval can be found.
        /// </summary>
        [TestMethod]
        [TestCategory("Integration")]
        public void SingleShort_Found()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = ShortIntervalValues.DefaultTime, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            IntervalsService service = new IntervalsService(fitnessData, PowerZones);
            IntervalContainer container = service.Search();

            // Assertions
            Assert.AreEqual(1, container.Intervals.Count);
            Assert.AreEqual(DefaultStartDate.AddSeconds(NuleIntervalValues.DefaultTime), container.Intervals[0].StartTime);
            Assert.AreEqual(ShortIntervalValues.DefaultTime, container.Intervals[0].TimeDiff);
            Assert.AreEqual(ShortIntervalValues.DefaultPower, container.Intervals[0].AveragePower);
        }

        /// <summary>
        /// Verifies that a single medium size interval can be found.
        /// </summary>
        [TestMethod]
        [TestCategory("Integration")]
        public void SingleMedium_Found()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime, Power = MediumIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            IntervalsService service = new IntervalsService(fitnessData, PowerZones);
            IntervalContainer container = service.Search();

            // Assertions
            Assert.AreEqual(1, container.Intervals.Count);
            Assert.AreEqual(DefaultStartDate.AddSeconds(NuleIntervalValues.DefaultTime), container.Intervals[0].StartTime);
            Assert.AreEqual(MediumIntervalValues.DefaultTime, container.Intervals[0].TimeDiff);
            Assert.AreEqual(MediumIntervalValues.DefaultPower, container.Intervals[0].AveragePower);
        }

        /// <summary>
        /// Verifies that a long medium size interval can be found.
        /// </summary>
        [TestMethod]
        [TestCategory("Integration")]
        public void SingleLong_Found()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = LongIntervalValues.DefaultTime, Power = LongIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            IntervalsService service = new IntervalsService(fitnessData, PowerZones);
            IntervalContainer container = service.Search();

            // Assertions
            Assert.AreEqual(1, container.Intervals.Count);
            Assert.AreEqual(DefaultStartDate.AddSeconds(NuleIntervalValues.DefaultTime), container.Intervals[0].StartTime);
            Assert.AreEqual(LongIntervalValues.DefaultTime, container.Intervals[0].TimeDiff);
            Assert.AreEqual(LongIntervalValues.DefaultPower, container.Intervals[0].AveragePower);
        }

        /// <summary>
        /// Short-Medium detection as a single interval.
        /// </summary>
        /// <remarks>
        /// "Small" differences should not be detected, short-medium-short is one interval with two short intervals inside
        /// </remarks>
        [TestMethod]
        [TestCategory("Integration")]
        public void ShortMediumShort_OneFound()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = ShortIntervalValues.DefaultTime, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime, Power = MediumIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = ShortIntervalValues.DefaultTime, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            IntervalsService service = new IntervalsService(fitnessData, PowerZones);
            IntervalContainer container = service.Search();

            // Assertions
            int totalTime = 2 * ShortIntervalValues.DefaultTime + MediumIntervalValues.DefaultTime;
            int expectedPower = (MediumIntervalValues.DefaultTime * MediumIntervalValues.DefaultPower + 2 * ShortIntervalValues.DefaultTime * ShortIntervalValues.DefaultPower) / totalTime;
            Assert.AreEqual(1, container.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, container.Intervals[0].StartTime);
            Assert.AreEqual(totalTime, container.Intervals[0].TimeDiff);
            Assert.AreEqual(expectedPower, container.Intervals[0].AveragePower, 1);
            Assert.AreEqual(2, container.Intervals[0].Intervals.Count);

            // Short intervals assertions
            Assert.AreEqual(DefaultStartDate, container.Intervals[0].Intervals[0].StartTime);
            Assert.AreEqual(ShortIntervalValues.DefaultTime, container.Intervals[0].Intervals[0].TimeDiff);
            Assert.AreEqual(ShortIntervalValues.DefaultPower, container.Intervals[0].Intervals[0].AveragePower);

            Assert.AreEqual(DefaultStartDate.AddSeconds(ShortIntervalValues.DefaultTime + MediumIntervalValues.DefaultTime), container.Intervals[0].Intervals[1].StartTime);
            Assert.AreEqual(ShortIntervalValues.DefaultTime, container.Intervals[0].Intervals[1].TimeDiff);
            Assert.AreEqual(ShortIntervalValues.DefaultPower, container.Intervals[0].Intervals[1].AveragePower);
        }

        /// <summary>
        /// Medium-Long detection as a single interval.
        /// </summary>
        /// <remarks>
        /// "Small" differences should not be detected, medium-long-medium is one interval with two short intervals inside
        /// </remarks>
        [TestMethod]
        [TestCategory("Integration")]
        public void MediumLongMedium_OneFound()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime, Power = MediumIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = LongIntervalValues.DefaultTime, Power = LongIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime, Power = MediumIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            IntervalsService service = new IntervalsService(fitnessData, PowerZones);
            IntervalContainer container = service.Search();

            // Assertions
            int totalTime = 2 * MediumIntervalValues.DefaultTime + LongIntervalValues.DefaultTime;
            int expectedPower = (LongIntervalValues.DefaultTime * LongIntervalValues.DefaultPower + 2 * MediumIntervalValues.DefaultTime * MediumIntervalValues.DefaultPower) / totalTime;
            Assert.AreEqual(1, container.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, container.Intervals[0].StartTime);
            Assert.AreEqual(totalTime, container.Intervals[0].TimeDiff);
            Assert.AreEqual(expectedPower, container.Intervals[0].AveragePower, 1);
            Assert.AreEqual(2, container.Intervals[0].Intervals.Count);

            // Medium intervals assertions
            Assert.AreEqual(DefaultStartDate, container.Intervals[0].Intervals[0].StartTime);
            Assert.AreEqual(MediumIntervalValues.DefaultTime, container.Intervals[0].Intervals[0].TimeDiff);
            Assert.AreEqual(MediumIntervalValues.DefaultPower, container.Intervals[0].Intervals[0].AveragePower);

            Assert.AreEqual(DefaultStartDate.AddSeconds(MediumIntervalValues.DefaultTime + LongIntervalValues.DefaultTime), container.Intervals[0].Intervals[1].StartTime);
            Assert.AreEqual(MediumIntervalValues.DefaultTime, container.Intervals[0].Intervals[1].TimeDiff);
            Assert.AreEqual(MediumIntervalValues.DefaultPower, container.Intervals[0].Intervals[1].AveragePower);
        }

        /// <summary>
        /// Short-Long detection as three different intervals.
        /// </summary>
        /// <remarks>
        /// "Big" differences should be detected, short-long-short must be detected as three different intervals
        /// </remarks>
        [TestMethod]
        [TestCategory("Integration")]
        public void ShortLongShort_ThreeFound()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = ShortIntervalValues.DefaultTime, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = LongIntervalValues.DefaultTime, Power = LongIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = ShortIntervalValues.DefaultTime, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            IntervalsService service = new IntervalsService(fitnessData, PowerZones);
            IntervalContainer container = service.Search();

            // Assertions
            Assert.AreEqual(3, container.Intervals.Count);
            // First short
            Assert.AreEqual(DefaultStartDate, container.Intervals[0].StartTime);
            Assert.AreEqual(ShortIntervalValues.DefaultTime, container.Intervals[0].TimeDiff);
            Assert.AreEqual(ShortIntervalValues.DefaultPower, container.Intervals[0].AveragePower, 1);
            // Long
            Assert.AreEqual(DefaultStartDate.AddSeconds(ShortIntervalValues.DefaultTime), container.Intervals[1].StartTime);
            Assert.AreEqual(LongIntervalValues.DefaultTime, container.Intervals[1].TimeDiff);
            Assert.AreEqual(LongIntervalValues.DefaultPower, container.Intervals[1].AveragePower);
            // Second short
            Assert.AreEqual(DefaultStartDate.AddSeconds(ShortIntervalValues.DefaultTime + LongIntervalValues.DefaultTime), container.Intervals[2].StartTime);
            Assert.AreEqual(ShortIntervalValues.DefaultTime, container.Intervals[2].TimeDiff);
            Assert.AreEqual(ShortIntervalValues.DefaultPower, container.Intervals[2].AveragePower);
        }

        /// <summary>
        /// Sprint inside a medium size interval is detected.
        /// </summary>
        /// <remarks>
        /// The power of the sprint must be ignored for the average power of the interval.
        /// </remarks>
        [TestMethod]
        [TestCategory("Integration")]
        public void SprintInMedium_Found()
        {
            int sprintTime = 10;
            int sprintPower = 500;
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime / 2, Power = MediumIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = sprintTime, Power = sprintPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime / 2, Power = MediumIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            IntervalsService service = new IntervalsService(fitnessData, PowerZones);
            IntervalContainer container = service.Search();

            // Assertions
            Assert.AreEqual(1, container.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, container.Intervals[0].StartTime);
            Assert.AreEqual(MediumIntervalValues.DefaultTime + sprintTime, container.Intervals[0].TimeDiff);
            Assert.AreEqual(MediumIntervalValues.DefaultPower, container.Intervals[0].AveragePower, 1);
            Assert.AreEqual(0, container.Intervals[0].Intervals.Count);

            // Sprint assertions
            Assert.AreEqual(1, container.Sprints.Count);
            Assert.AreEqual(DefaultStartDate.AddSeconds(MediumIntervalValues.DefaultTime / 2), container.Sprints[0].StartTime);
            Assert.AreEqual(sprintTime, container.Sprints[0].TimeDiff);
            Assert.AreEqual(sprintPower, container.Sprints[0].AveragePower);
            Assert.AreEqual(sprintPower, container.Sprints[0].MaxPower);
        }

        /// <summary>
        /// Verifies that threshold can be changed and has its impact.
        /// </summary>
        /// <remarks>
        /// When searching with default thresholds the interval mustn't be found.
        /// When changing the thresholds the interval must be found.
        /// The interval will be a medium size interval with big power changes.
        /// Only the medium threshold will be updated.
        /// </remarks>
        [TestMethod]
        [TestCategory("Integration")]
        public void ThresholdChange_Found()
        {
            int totalChanges = 6;
            int constantPowerTime = 30;
            int highPower = 280;
            int lowPower = 160;
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>();
            for (int i = 0; i < totalChanges; i++)
            {
                fitnessTestSections.Add(
                    new FitnessSection { Time = constantPowerTime, Power = highPower, HearRate = 120, Cadence = 85 }
                    );
                fitnessTestSections.Add(
                    new FitnessSection { Time = constantPowerTime, Power = lowPower, HearRate = 120, Cadence = 85 }
                    );
            }
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            IntervalsService service = new IntervalsService(fitnessData, PowerZones);
            IntervalContainer emptyContainer = service.Search();
            IntervalGroupThresholds newThresholds = new IntervalGroupThresholds
            {
                Short = new Thresholds
                {
                    CvStart = IntervalSearchValues.ShortIntervals.Default.CvStart,
                    CvFollow = IntervalSearchValues.ShortIntervals.Default.CvFollow,
                    Range = IntervalSearchValues.ShortIntervals.Default.Range,
                    MaRel = IntervalSearchValues.ShortIntervals.Default.MaRel,
                },
                Medium = new Thresholds
                {
                    CvStart = IntervalSearchValues.MediumIntervals.Default.CvStart,
                    CvFollow = IntervalSearchValues.MediumIntervals.Default.CvFollow,
                    Range = IntervalSearchValues.MediumIntervals.Default.Range,
                    MaRel = 0.30f,
                },
                Long = new Thresholds
                {
                    CvStart = IntervalSearchValues.LongIntervals.Default.CvStart,
                    CvFollow = IntervalSearchValues.LongIntervals.Default.CvFollow,
                    Range = IntervalSearchValues.LongIntervals.Default.Range,
                    MaRel = IntervalSearchValues.LongIntervals.Default.MaRel,
                },
            };
            service.SetThresholds(newThresholds);
            IntervalContainer fullContainer = service.Search();

            // Assertions
            Assert.AreEqual(0, emptyContainer.Intervals.Count);

            Assert.AreEqual(1, fullContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, fullContainer.Intervals[0].StartTime);
            Assert.AreEqual((totalChanges * constantPowerTime * 2), fullContainer.Intervals[0].TimeDiff);
            Assert.AreEqual((highPower + lowPower) / 2, fullContainer.Intervals[0].AveragePower);
        }

        /// <summary>
        /// Verifies that threshold can be changed and has its impact.
        /// </summary>
        /// <remarks>
        /// When searching with default thresholds the interval must be found.
        /// When changing the thresholds the interval mustn't be found.
        /// The interval will be a medium size interval with small power changes.
        /// Only the medium threshold will be updated.
        /// </remarks>
        [TestMethod]
        [TestCategory("Integration")]
        public void ThresholdChange_NotFound()
        {
            int totalChanges = 6;
            int constantPowerTime = 30;
            int highPower = 250;
            int lowPower = 190;
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>();
            for (int i = 0; i < totalChanges; i++)
            {
                fitnessTestSections.Add(
                    new FitnessSection { Time = constantPowerTime, Power = highPower, HearRate = 120, Cadence = 85 }
                    );
                fitnessTestSections.Add(
                    new FitnessSection { Time = constantPowerTime, Power = lowPower, HearRate = 120, Cadence = 85 }
                    );
            }
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            IntervalsService service = new IntervalsService(fitnessData, PowerZones);
            IntervalContainer fullContainer = service.Search();
            IntervalGroupThresholds newThresholds = new IntervalGroupThresholds
            {
                Short = new Thresholds
                {
                    CvStart = IntervalSearchValues.ShortIntervals.Default.CvStart,
                    CvFollow = IntervalSearchValues.ShortIntervals.Default.CvFollow,
                    Range = IntervalSearchValues.ShortIntervals.Default.Range,
                    MaRel = IntervalSearchValues.ShortIntervals.Default.MaRel,
                },
                Medium = new Thresholds
                {
                    CvStart = IntervalSearchValues.MediumIntervals.Default.CvStart,
                    CvFollow = IntervalSearchValues.MediumIntervals.Default.CvFollow,
                    Range = IntervalSearchValues.MediumIntervals.Default.Range,
                    MaRel = 0.30f,
                },
                Long = new Thresholds
                {
                    CvStart = IntervalSearchValues.LongIntervals.Default.CvStart,
                    CvFollow = IntervalSearchValues.LongIntervals.Default.CvFollow,
                    Range = IntervalSearchValues.LongIntervals.Default.Range,
                    MaRel = IntervalSearchValues.LongIntervals.Default.MaRel,
                },
            };
            newThresholds.Medium.MaRel = 0.10f;
            service.SetThresholds(newThresholds);
            IntervalContainer emptyContainer = service.Search();

            // Assertions
            Assert.AreEqual(1, fullContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, fullContainer.Intervals[0].StartTime);
            Assert.AreEqual((totalChanges * constantPowerTime * 2), fullContainer.Intervals[0].TimeDiff);
            Assert.AreEqual((highPower + lowPower) / 2, fullContainer.Intervals[0].AveragePower);

            Assert.AreEqual(0, emptyContainer.Intervals.Count);
        }
    }
}