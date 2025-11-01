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

        // [TestMethod]
        // public void ShortStopShort()
        // {
        //     List<FitnessSection> fitnessTestSections = new List<FitnessSection>
        //     {
        //         new FitnessSection{ Time = ShortDefaultTime, Power = ShortDefaultPower, HearRate = 120, Cadence = 85},
        //         new FitnessSection{ Time = 0, Power = 20, HearRate = 0, Cadence = 0},
        //         new FitnessSection{ Time = ShortDefaultTime, Power = ShortDefaultPower, HearRate = 120, Cadence = 85},
        //     };
        //     List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
        //     List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
        //     Assert.AreEqual(2, intervals.Count);
        //     Assert.AreEqual(ShortDefaultTime, intervals.First().TimeDiff);
        //     Assert.AreEqual(ShortDefaultPower, intervals.First().AveragePower);
        //     Assert.AreEqual(ShortDefaultTime, intervals.Last().TimeDiff);
        //     Assert.AreEqual(ShortDefaultPower, intervals.Last().AveragePower);
        // }

        // [TestMethod]
        // public void NuleMediumNule()
        // {
        //     List<FitnessSection> fitnessTestSections = new List<FitnessSection>
        //     {
        //         new FitnessSection{ Time = 300, Power = 150, HearRate = 120, Cadence = 85},
        //         new FitnessSection{ Time = MediumDefaultTime, Power = MediumDefaultPower, HearRate = 120, Cadence = 85},
        //         new FitnessSection{ Time = 300, Power = 150, HearRate = 120, Cadence = 85},
        //     };
        //     List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
        //     List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
        //     Assert.AreEqual(1, intervals.Count);
        //     Assert.AreEqual(MediumDefaultPower, intervals.First().AveragePower);
        //     Assert.AreEqual(MediumDefaultTime, intervals.First().TimeDiff);
        // }

        // [TestMethod]
        // public void ShortMediumShort()      // "Small" differences should not be detected, short-medium-short is one interval with two short intervals inside
        // {
        //     List<FitnessSection> fitnessTestSections = new List<FitnessSection>
        //     {
        //         new FitnessSection{ Time = ShortDefaultTime, Power = ShortDefaultPower, HearRate = 120, Cadence = 85},
        //         new FitnessSection{ Time = MediumDefaultTime, Power = MediumDefaultPower, HearRate = 120, Cadence = 85},
        //         new FitnessSection{ Time = ShortDefaultTime, Power = ShortDefaultPower, HearRate = 120, Cadence = 85},
        //     };
        //     List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
        //     List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
        //     Assert.AreEqual(1, intervals.Count);
        //     int expectedTime = ShortDefaultTime * 2 + MediumDefaultTime;
        //     float expectedPower = (ShortDefaultTime * ShortDefaultPower * 2 + MediumDefaultTime * MediumDefaultPower) / expectedTime;
        //     Assert.AreEqual(expectedTime, intervals[0].TimeDiff);
        //     Assert.AreEqual(expectedPower, intervals[0].AveragePower, 1f);
        //     Assert.AreEqual(2, intervals[0].Intervals?.Count);
        //     Assert.AreEqual(ShortDefaultTime, intervals[0].Intervals?[0].TimeDiff);
        //     Assert.AreEqual(ShortDefaultPower, intervals[0].Intervals?[0].AveragePower);
        //     Assert.AreEqual(ShortDefaultTime, intervals[0].Intervals?[1].TimeDiff);
        //     Assert.AreEqual(ShortDefaultPower, intervals[0].Intervals?[1].AveragePower);
        // }

        // [TestMethod]
        // public void MediumLongMedium()
        // {
        //     List<FitnessSection> fitnessTestSections = new List<FitnessSection>
        //     {
        //         new FitnessSection{ Time = MediumDefaultTime, Power = MediumDefaultPower, HearRate = 120, Cadence = 85},
        //         new FitnessSection{ Time = LongDefaultTime, Power = LongDefaultPower, HearRate = 120, Cadence = 85},
        //         new FitnessSection{ Time = MediumDefaultTime, Power = MediumDefaultPower, HearRate = 120, Cadence = 85},
        //     };
        //     List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
        //     List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
        //     Assert.AreEqual(1, intervals.Count);
        //     int expectedTime = LongDefaultTime + MediumDefaultTime * 2;
        //     float expectedPower = (MediumDefaultTime * MediumDefaultPower * 2 + LongDefaultTime * LongDefaultPower) / expectedTime;
        //     Assert.AreEqual(expectedTime, intervals[0].TimeDiff);
        //     Assert.AreEqual(expectedPower, intervals[0].AveragePower, 1f);
        //     Assert.AreEqual(2, intervals[0].Intervals?.Count);
        //     Assert.AreEqual(MediumDefaultTime, intervals[0].Intervals?[0].TimeDiff);
        //     Assert.AreEqual(MediumDefaultPower, intervals[0].Intervals?[0].AveragePower);
        //     Assert.AreEqual(MediumDefaultTime, intervals[0].Intervals?[1].TimeDiff);
        //     Assert.AreEqual(MediumDefaultPower, intervals[0].Intervals?[1].AveragePower);
        // }

        // [TestMethod]
        // public void ShortLongShort()        // Bigger differences must be detected, short-long-short are three different intervals
        // {
        //     List<FitnessSection> fitnessTestSections = new List<FitnessSection>
        //     {
        //         new FitnessSection{ Time = ShortDefaultTime, Power = ShortDefaultPower, HearRate = 120, Cadence = 85},
        //         new FitnessSection{ Time = LongDefaultTime, Power = LongDefaultPower, HearRate = 120, Cadence = 85},
        //         new FitnessSection{ Time = ShortDefaultTime, Power = ShortDefaultPower, HearRate = 120, Cadence = 85},
        //     };
        //     List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
        //     List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
        //     Assert.AreEqual(3, intervals.Count);
        //     Assert.AreEqual(ShortDefaultTime, intervals[0].TimeDiff);
        //     Assert.AreEqual(ShortDefaultPower, intervals[0].AveragePower);
        //     Assert.AreEqual(LongDefaultTime, intervals[1].TimeDiff);
        //     Assert.AreEqual(LongDefaultPower, intervals[1].AveragePower);
        //     Assert.AreEqual(ShortDefaultTime, intervals[2].TimeDiff);
        //     Assert.AreEqual(ShortDefaultPower, intervals[2].AveragePower);
        // }
    }
}