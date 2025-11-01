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
    public sealed class IntervalsServiceUnitTests
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Verifies that when no valid points given, an exceptio is thrown.
        /// </summary>
        [TestMethod]
        public void InvalidPoints_Exception()
        {
            List<FitnessData> fitnessData = new List<FitnessData>();
            void AuxMethod()
            {
                IntervalsService service = new IntervalsService(fitnessData, PowerZones);
            }
            Assert.ThrowsException<ArgumentException>(() => AuxMethod());
        }

        /// <summary>
        /// Verifies that when no valid power zones given, an exceptio is thrown.
        /// </summary>
        [TestMethod]
        public void InvalidPowerZones_Exception()
        {
            List<CoreModels.Zone> powerZones = new List<CoreModels.Zone>{   // Doesn't have zone 7
                new CoreModels.Zone { Id = 1, LowLimit = 0, HighLimit = 129},
                new CoreModels.Zone { Id = 2, LowLimit = 130, HighLimit = NuleIntervalValues.MaxPower - 1},
                new CoreModels.Zone { Id = 3, LowLimit = NuleIntervalValues.MaxPower, HighLimit = LongIntervalValues.MaxPower - 1},
                new CoreModels.Zone { Id = 4, LowLimit = LongIntervalValues.MaxPower, HighLimit = MediumIntervalValues.MaxPower - 1},
                new CoreModels.Zone { Id = 5, LowLimit = MediumIntervalValues.MaxPower, HighLimit = ShortIntervalValues.MaxPower - 1},
                new CoreModels.Zone { Id = 6, LowLimit = ShortIntervalValues.MaxPower, HighLimit = ShortIntervalValues.MaxPower + 49},
            };
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.MaxPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            IntervalsService service = new IntervalsService(fitnessData, powerZones);
            Assert.ThrowsException<ArgumentException>(() => service.Search());
        }

        private (IntervalsService service, IntervalGroupThresholds defaultThresholds) ThresholdExceptionSetup()
        {
            var sections = new List<FitnessSection> {
                new FitnessSection { Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.MaxPower, HearRate = 120, Cadence = 85 }
            };
            var data = FitnessDataService.SetData(sections);
            IntervalsService service = new IntervalsService(data, PowerZones);

            IntervalGroupThresholds defaultThresholds = new IntervalGroupThresholds
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
                    MaRel = IntervalSearchValues.MediumIntervals.Default.MaRel,
                },
                Long = new Thresholds
                {
                    CvStart = IntervalSearchValues.LongIntervals.Default.CvStart,
                    CvFollow = IntervalSearchValues.LongIntervals.Default.CvFollow,
                    Range = IntervalSearchValues.LongIntervals.Default.Range,
                    MaRel = IntervalSearchValues.LongIntervals.Default.MaRel,
                },
            };

            return (service, defaultThresholds);
        }

        private void AssertThrowsForThreshold(IntervalsService service, IntervalGroupThresholds defaultThresholds, Action<IntervalGroupThresholds> modifier)
        {
            var t = defaultThresholds;
            modifier(t);
            Assert.ThrowsException<ArgumentException>(() => service.SetThresholds(t));
        }

        /// <summary>
        /// Verifies that in the <see cref="IntervalsService.SetThresholds(IntervalGroupThresholds)"/>
        /// method the short group arguments can't exceed the limits.
        /// </summary>
        [TestMethod]
        public void ShortThresholdChange_Exception()
        {
            var init = ThresholdExceptionSetup();
            AssertThrowsForThreshold(init.service, init.defaultThresholds, t => t.Short.CvStart = IntervalSearchValues.ShortIntervals.Min.CvStart * 0.99f);
            AssertThrowsForThreshold(init.service, init.defaultThresholds, t => t.Short.CvStart = IntervalSearchValues.ShortIntervals.Max.CvStart * 1.01f);
            AssertThrowsForThreshold(init.service, init.defaultThresholds, t => t.Short.CvFollow = IntervalSearchValues.ShortIntervals.Min.CvFollow * 0.99f);
            AssertThrowsForThreshold(init.service, init.defaultThresholds, t => t.Short.CvFollow = IntervalSearchValues.ShortIntervals.Max.CvFollow * 1.01f);
            AssertThrowsForThreshold(init.service, init.defaultThresholds, t => t.Short.Range = IntervalSearchValues.ShortIntervals.Min.Range * 0.99f);
            AssertThrowsForThreshold(init.service, init.defaultThresholds, t => t.Short.Range = IntervalSearchValues.ShortIntervals.Max.Range * 1.01f);
            AssertThrowsForThreshold(init.service, init.defaultThresholds, t => t.Short.MaRel = IntervalSearchValues.ShortIntervals.Min.MaRel * 0.99f);
            AssertThrowsForThreshold(init.service, init.defaultThresholds, t => t.Short.MaRel = IntervalSearchValues.ShortIntervals.Max.MaRel * 1.01f);
        }

        /// <summary>
        /// Verifies that in the <see cref="IntervalsService.SetThresholds(IntervalGroupThresholds)"/>
        /// method the medium group arguments can't exceed the limits.
        /// </summary>
        [TestMethod]
        public void MediumThresholdChange_Exception()
        {
            var init = ThresholdExceptionSetup();
            AssertThrowsForThreshold(init.service, init.defaultThresholds, t => t.Medium.CvStart = IntervalSearchValues.MediumIntervals.Min.CvStart * 0.99f);
            AssertThrowsForThreshold(init.service, init.defaultThresholds, t => t.Medium.CvStart = IntervalSearchValues.MediumIntervals.Max.CvStart * 1.01f);
            AssertThrowsForThreshold(init.service, init.defaultThresholds, t => t.Medium.CvFollow = IntervalSearchValues.MediumIntervals.Min.CvFollow * 0.99f);
            AssertThrowsForThreshold(init.service, init.defaultThresholds, t => t.Medium.CvFollow = IntervalSearchValues.MediumIntervals.Max.CvFollow * 1.01f);
            AssertThrowsForThreshold(init.service, init.defaultThresholds, t => t.Medium.Range = IntervalSearchValues.MediumIntervals.Min.Range * 0.99f);
            AssertThrowsForThreshold(init.service, init.defaultThresholds, t => t.Medium.Range = IntervalSearchValues.MediumIntervals.Max.Range * 1.01f);
            AssertThrowsForThreshold(init.service, init.defaultThresholds, t => t.Medium.MaRel = IntervalSearchValues.MediumIntervals.Min.MaRel * 0.99f);
            AssertThrowsForThreshold(init.service, init.defaultThresholds, t => t.Medium.MaRel = IntervalSearchValues.MediumIntervals.Max.MaRel * 1.01f);
        }

        /// <summary>
        /// Verifies that in the <see cref="IntervalsService.SetThresholds(IntervalGroupThresholds)"/>
        /// method the long group arguments can't exceed the limits.
        /// </summary>
        [TestMethod]
        public void LongThresholdChange_Exception()
        {
            var init = ThresholdExceptionSetup();
            AssertThrowsForThreshold(init.service, init.defaultThresholds, t => t.Long.CvStart = IntervalSearchValues.LongIntervals.Min.CvStart * 0.99f);
            AssertThrowsForThreshold(init.service, init.defaultThresholds, t => t.Long.CvStart = IntervalSearchValues.LongIntervals.Max.CvStart * 1.01f);
            AssertThrowsForThreshold(init.service, init.defaultThresholds, t => t.Long.CvFollow = IntervalSearchValues.LongIntervals.Min.CvFollow * 0.99f);
            AssertThrowsForThreshold(init.service, init.defaultThresholds, t => t.Long.CvFollow = IntervalSearchValues.LongIntervals.Max.CvFollow * 1.01f);
            AssertThrowsForThreshold(init.service, init.defaultThresholds, t => t.Long.Range = IntervalSearchValues.LongIntervals.Min.Range * 0.99f);
            AssertThrowsForThreshold(init.service, init.defaultThresholds, t => t.Long.Range = IntervalSearchValues.LongIntervals.Max.Range * 1.01f);
            AssertThrowsForThreshold(init.service, init.defaultThresholds, t => t.Long.MaRel = IntervalSearchValues.LongIntervals.Min.MaRel * 0.99f);
            AssertThrowsForThreshold(init.service, init.defaultThresholds, t => t.Long.MaRel = IntervalSearchValues.LongIntervals.Max.MaRel * 1.01f);
        }
    }
}