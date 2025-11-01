using CoreModels = CyclingTrainer.Core.Models;
using CyclingTrainer.SessionReader.Models;
using CyclingTrainer.SessionAnalyzer.Test.Models;
using CyclingTrainer.SessionAnalyzer.Constants;
using CyclingTrainer.SessionAnalyzer.Enums;
using CyclingTrainer.SessionAnalyzer.Models;
using CyclingTrainer.SessionAnalyzer.Services.Intervals;
using static CyclingTrainer.SessionAnalyzer.Test.Constants.FitnessDataCreation;
using static CyclingTrainer.SessionAnalyzer.Test.Intervals.IntervalsTestConstants;

namespace CyclingTrainer.SessionAnalyzer.Test.Intervals
{
    /// <summary>
    /// Contains the unit test of the <see cref="IntervalsFinder"/> class.
    /// </summary>
    /// <remarks>
    /// This class verifies different scenarios in each of the three interval search times:
    /// Long, medium and short
    /// 
    /// The test follow the convention:
    /// <c>Configuration_Scenario_ExpectedResult</c>.
    /// The assertion of the expected result will allways contain the interval count; and for each interval:
    /// interval start date, time, average power and, if necessary, sub-interval count.
    /// </remarks>
    [TestClass]
    public sealed class FinderUnitTests
    {
        /// <summary>
        /// Verifies whith invalid power zones an exception is thrown.
        /// </summary>
        /// <remarks>
        /// When giving an invalid power zone list to the constructor, 
        /// an exception is thrown.
        /// </remarks>
        [TestMethod]
        public void Short_PowerZones_ExceptionThrown()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.MaxPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();

            List<CoreModels.Zone> powerZones = new List<CoreModels.Zone>{   // Doesn't have zone 7
                new CoreModels.Zone { Id = 1, LowLimit = 0, HighLimit = 129},
                new CoreModels.Zone { Id = 2, LowLimit = 130, HighLimit = NuleIntervalValues.MaxPower - 1},
                new CoreModels.Zone { Id = 3, LowLimit = NuleIntervalValues.MaxPower, HighLimit = LongIntervalValues.MaxPower - 1},
                new CoreModels.Zone { Id = 4, LowLimit = LongIntervalValues.MaxPower, HighLimit = MediumIntervalValues.MaxPower - 1},
                new CoreModels.Zone { Id = 5, LowLimit = MediumIntervalValues.MaxPower, HighLimit = ShortIntervalValues.MaxPower - 1},
                new CoreModels.Zone { Id = 6, LowLimit = ShortIntervalValues.MaxPower, HighLimit = ShortIntervalValues.MaxPower + 49},
            };
            
            void AuxMethod()
            {
                IntervalsFinder finder = new IntervalsFinder(
                    fitnessDataContainer, intervalContainer, powerZones,
                    IntervalSeachGroups.Short, ShortThresholds
                );
            }
            Assert.ThrowsException<ArgumentException>(() => AuxMethod());
        }

        /// <summary>
        /// Verifies that no interval is found.
        /// </summary>
        /// <remarks>
        /// When searching with short configuration in a session with no valid intervals,
        /// no interval will be found.
        /// </remarks>
        [TestMethod]
        public void Short_NoIntervals_NotFound()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.MaxPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.MaxPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Short, ShortThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(0, intervalContainer.Intervals.Count);
        }

        /// <summary>
        /// Verifies that a short interval is found with short configuration.
        /// </summary>
        /// <remarks>
        /// When searching with short configuration in a session with one short interval,
        /// the interval will be found.
        /// </remarks>
        [TestMethod]
        public void Short_SingleShortInterval_Found()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = ShortIntervalValues.DefaultTime, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Short, ShortThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate.AddSeconds(NuleIntervalValues.DefaultTime), intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(ShortIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(ShortIntervalValues.DefaultPower, intervalContainer.Intervals[0].AveragePower);
        }

        /// <summary>
        /// Verifies that a short interval is found with short configuration (default thresholds).
        /// </summary>
        /// <remarks>
        /// When searching with short configuration (default thresholds) in a session with one short interval,
        /// the interval will be found.
        /// </remarks>
        [TestMethod]
        public void ShortDefault_SingleShortInterval_Found()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = ShortIntervalValues.DefaultTime, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Short
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate.AddSeconds(NuleIntervalValues.DefaultTime), intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(ShortIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(ShortIntervalValues.DefaultPower, intervalContainer.Intervals[0].AveragePower);
        }

        /// <summary>
        /// Verifies that a short interval divided by a pause won't be found.
        /// </summary>
        /// <remarks>
        /// When searching with short configuration in a session with one short interval
        /// divided by a pause, the interval won't be found.
        /// </remarks>
        [TestMethod]
        public void Short_DividedByPause_NotFound()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = 20, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 0, Power = 10, HearRate = 0, Cadence = 0},       // 10 second session stop
                new FitnessSection{ Time = 20, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 90},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Short, ShortThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(0, intervalContainer.Intervals.Count);
        }

        /// <summary>
        /// Verifies that a short interval divided by a pause will be found in two.
        /// </summary>
        /// <remarks>
        /// When searching with short configuration in a session with one short interval
        /// divided by a pause, two intervals will be found.
        /// </remarks>
        [TestMethod]
        public void Short_DividedByPause_TwoFound()
        {
            int pauseTime = 10;
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = ShortIntervalValues.DefaultTime, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 0, Power = pauseTime, HearRate = 0, Cadence = 0},       // 10 second session stop
                new FitnessSection{ Time = ShortIntervalValues.DefaultTime, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 90},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Short, ShortThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(2, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(ShortIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(ShortIntervalValues.DefaultPower, intervalContainer.Intervals[0].AveragePower);
            // Second
            Assert.AreEqual(DefaultStartDate.AddSeconds(ShortIntervalValues.DefaultTime + pauseTime), intervalContainer.Intervals[1].StartTime);
            Assert.AreEqual(ShortIntervalValues.DefaultTime, intervalContainer.Intervals[1].TimeDiff);
            Assert.AreEqual(ShortIntervalValues.DefaultPower, intervalContainer.Intervals[1].AveragePower);
        }

        /// <summary>
        /// Verifies that a short interval with a pause before its start is detected.
        /// </summary>
        /// <remarks>
        /// When searching with short configuration in a session with one short interval
        /// that has a pause a few seconds before the start, the interval will be found.
        /// </remarks>
        [TestMethod]
        public void Short_PauseBeforeInterval_Found()
        {
            int pauseTime = 5;
            int timeGap = 5;    // Time between pause and interval
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 0, Power = pauseTime, HearRate = 0, Cadence = 0},
                new FitnessSection{ Time = timeGap, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = ShortIntervalValues.DefaultTime, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 90},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Short, ShortThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate.AddSeconds(NuleIntervalValues.DefaultTime + pauseTime + timeGap), intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(ShortIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(ShortIntervalValues.DefaultPower, intervalContainer.Intervals[0].AveragePower);
        }

        /// <summary>
        /// Verifies that a short interval with a pause after its end is detected.
        /// </summary>
        /// <remarks>
        /// When searching with short configuration in a session with one short interval
        /// that has a pause a few seconds after the end, the interval will be found.
        /// </remarks>
        [TestMethod]
        public void Short_PauseAfterInterval_Found()
        {
            int pauseTime = 5;
            int timeGap = 5;    // Time between pause and interval
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = ShortIntervalValues.DefaultTime, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 90},
                new FitnessSection{ Time = timeGap, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 0, Power = pauseTime, HearRate = 0, Cadence = 0},
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Short, ShortThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(ShortIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(ShortIntervalValues.DefaultPower, intervalContainer.Intervals[0].AveragePower);
        }

        /// <summary>
        /// Verifies that already saved intervals won't be saved again.
        /// </summary>
        /// <remarks>
        /// When searching with short configuration in a session with one short interval,
        /// if the found interval is already in the container, it won't be saved again.
        /// </remarks>
        [TestMethod]
        public void Short_AlreadyFound_NotSaved()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = ShortIntervalValues.DefaultTime, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            intervalContainer.Intervals.Add(new Interval()
            {
                StartTime = DefaultStartDate,
                EndTime = DefaultStartDate.AddSeconds(ShortIntervalValues.DefaultTime - 1),
                TimeDiff = ShortIntervalValues.DefaultTime,
                AveragePower = ShortIntervalValues.DefaultPower
            });
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Short, ShortThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(ShortIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(ShortIntervalValues.DefaultPower, intervalContainer.Intervals[0].AveragePower);
        }

        /// <summary>
        /// Verifies that a medium interval is not found with short configuration.
        /// </summary>
        /// <remarks>
        /// When searching with short configuration in a session with one medium interval,
        /// the interval will not be found.
        /// </remarks>
        [TestMethod]
        public void Short_SingleMediumInterval_NotFound()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime, Power = MediumIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Short, ShortThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(0, intervalContainer.Intervals.Count);
        }

        /// <summary>
        /// Verifies that a short interval at limit power but that doesn't pass the threshold
        /// isn't found with short configuration.
        /// </summary>
        /// <remarks>
        /// When searching with short configuration in a session with one short interval
        /// with the average power at the limit, but that doesn't pass the finding low threshold, 
        /// the interval won't be found.
        /// </remarks>
        [TestMethod]
        public void Short_SingleShortBellowThreshold_NotFound()
        {
            int intervalTime = 200;
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = intervalTime, Power = MediumIntervalValues.MaxPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Short, ShortThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(0, intervalContainer.Intervals.Count);
        }

        /// <summary>
        /// Verifies that a short interval at limit power is found with short configuration.
        /// </summary>
        /// <remarks>
        /// When searching with short configuration in a session with one short interval
        /// with the average power at the limit, the interval will be found.
        /// </remarks>
        [TestMethod]
        public void Short_SingleShortAverageLimit_Found()
        {
            int intervalTime = 200;
            int firstPower = ShortIntervalValues.MinPower;
            int secondPower = MediumIntervalValues.DefaultPower;
            int averagePower = MediumIntervalValues.MaxPower;
            int intervalFirstPeriod = (averagePower - secondPower) * intervalTime / (firstPower - secondPower);
            int intervalSecondPeriod = intervalTime - intervalFirstPeriod;
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = intervalFirstPeriod, Power = ShortIntervalValues.MinPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = intervalSecondPeriod, Power = MediumIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Short, ShortThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate.AddSeconds(NuleIntervalValues.DefaultTime), intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(intervalTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(averagePower, intervalContainer.Intervals[0].AveragePower, 10);
        }

        /// <summary>
        /// Verifies that a short interval with a power drop is found as two separate intervals.
        /// </summary>
        /// <remarks>
        /// When searching with short configuration in a session with one short interval
        /// divided by a 30 second ~20% power drop, the finder will find two separate intervals.
        /// Even if the average power for the full interval is higher than the low limit.
        /// </remarks>
        [TestMethod]
        public void Short_ShortIntervalDrop_TwoFound()
        {
            int dropTime = 30;
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = ShortIntervalValues.DefaultTime, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = dropTime, Power = MediumIntervalValues.MinPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = ShortIntervalValues.DefaultTime, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Short, ShortThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(2, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(ShortIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(ShortIntervalValues.DefaultPower, intervalContainer.Intervals[0].AveragePower);
            // Second interval
            Assert.AreEqual(DefaultStartDate.AddSeconds(ShortIntervalValues.DefaultTime + dropTime), intervalContainer.Intervals[1].StartTime);
            Assert.AreEqual(ShortIntervalValues.DefaultTime, intervalContainer.Intervals[1].TimeDiff);
            Assert.AreEqual(ShortIntervalValues.DefaultPower, intervalContainer.Intervals[1].AveragePower);
        }

        /// <summary>
        /// Verifies that a medium interval with a high power is found with short configuration.
        /// </summary>
        /// <remarks>
        /// This tests the improbable scenario where a medium size interval with the power of a 
        /// short size interval appears in the session. When searching with short configuration, 
        /// the finder will find the interval.
        /// </remarks>
        [TestMethod]
        public void Short_MediumIntervalHighPower_Found()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Short, ShortThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(MediumIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(ShortIntervalValues.DefaultPower, intervalContainer.Intervals[0].AveragePower);
        }

        /// <summary>
        /// Verifies that finds the exact point where the interval starts.
        /// </summary>
        /// <remarks>
        /// Usually the tested intervals mantain the same power through the
        /// interval period. This method tests if the finder can detect the exact 
        /// start point even if the start power doesn't mantain constant.
        /// </remarks>
        [TestMethod]
        public void Short_IrregularStart_Found()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 2, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 2, Power = MediumIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},     // Should not be detected as part of the interval
                new FitnessSection{ Time = ShortIntervalValues.DefaultTime, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Short, ShortThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate.AddSeconds(NuleIntervalValues.DefaultTime + 4), intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(ShortIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(ShortIntervalValues.DefaultPower, intervalContainer.Intervals[0].AveragePower);
        }

        /// <summary>
        /// Verifies that finds the exact point where the interval ends.
        /// </summary>
        /// <remarks>
        /// Usually the tested intervals mantain the same power through the
        /// interval period. This method tests if the finder can detect the exact 
        /// end point even if the end power doesn't mantain constant.
        /// </remarks>
        [TestMethod]
        public void Short_IrregularEnd_Found()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = ShortIntervalValues.DefaultTime, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 2, Power = MediumIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 2, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 2, Power = MediumIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},  // Should not be detected as part of the interval
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Short, ShortThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(ShortIntervalValues.DefaultTime + 4, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(ShortIntervalValues.DefaultPower - 1, intervalContainer.Intervals[0].AveragePower, 1);
        }

        /// <summary>
        /// Verifies that a short interval is not found with medium configuration.
        /// </summary>
        /// <remarks>
        /// When searching with medium configuration in a session with one short interval,
        /// the interval won't be found.
        /// </remarks>
        [TestMethod]
        public void Medium_SingleShortInterval_NotFound()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = ShortIntervalValues.DefaultTime, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Medium, MediumThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(0, intervalContainer.Intervals.Count);
        }

        /// <summary>
        /// Verifies that a medium interval is found with medium configuration.
        /// </summary>
        /// <remarks>
        /// When searching with medium configuration in a session with one medium interval,
        /// the interval will be found.
        /// </remarks>
        [TestMethod]
        public void Medium_SingleMediumInterval_Found()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime, Power = MediumIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Medium, MediumThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(MediumIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(MediumIntervalValues.DefaultPower, intervalContainer.Intervals[0].AveragePower);
        }

        /// <summary>
        /// Verifies that a medium interval is found with medium configuration (default thresholds).
        /// </summary>
        /// <remarks>
        /// When searching with medium configuration (default thresholds) in a session with one medium interval,
        /// the interval will be found.
        /// </remarks>
        [TestMethod]
        public void MediumDefault_SingleMediumInterval_Found()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime, Power = MediumIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Medium
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate.AddSeconds(NuleIntervalValues.DefaultTime), intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(MediumIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(MediumIntervalValues.DefaultPower, intervalContainer.Intervals[0].AveragePower);
        }

        /// <summary>
        /// Verifies that a long interval isn't found with medium configuration.
        /// </summary>
        /// <remarks>
        /// When searching with medium configuration in a session with one long interval,
        /// the interval won't be found.
        /// </remarks>
        [TestMethod]
        public void Medium_SingleLongInterval_NotFound()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = LongIntervalValues.DefaultTime, Power = LongIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Medium, MediumThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(0, intervalContainer.Intervals.Count);
        }

        /// <summary>
        /// Verifies that a medium interval with a high power is found with medium configuration.
        /// </summary>
        /// <remarks>
        /// This tests the improbable scenario where a medium size interval with the power of a 
        /// short size interval appears in the session. When searching with medium configuration, 
        /// the finder will find the interval.
        /// </remarks>
        [TestMethod]
        public void Medium_MediumIntervalHighPower_Found()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime, Power = ShortIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Medium, MediumThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(MediumIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(ShortIntervalValues.DefaultPower, intervalContainer.Intervals[0].AveragePower);
        }

        /// <summary>
        /// Verifies that a long interval with a high power is found with medium configuration.
        /// </summary>
        /// <remarks>
        /// This tests the improbable scenario where a long size interval with the power of a 
        /// medium size interval appears in the session. When searching with medium configuration, 
        /// the finder will find the interval.
        /// </remarks>
        [TestMethod]
        public void Medium_LongIntervalHighPower_Found()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = LongIntervalValues.DefaultTime, Power = MediumIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Medium, MediumThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(LongIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(MediumIntervalValues.DefaultPower, intervalContainer.Intervals[0].AveragePower);
        }

        /// <summary>
        /// Verifies that a medium interval with a power drop is found as one interval.
        /// </summary>
        /// <remarks>
        /// When searching with medium configuration in a session with one medium interval
        /// divided by a 60 second ~15% power drop, the finder will find the full interval.
        /// </remarks>
        [TestMethod]
        public void Medium_MediumIntervalDrop_OneFound()
        {
            int dropTime = 60;
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime, Power = MediumIntervalValues.MaxPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = dropTime, Power = LongIntervalValues.MaxPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime, Power = MediumIntervalValues.MaxPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Medium, MediumThresholds
            );
            finder.Search();

            // Assertions
            int expectedPower = (MediumIntervalValues.DefaultTime * MediumIntervalValues.MaxPower * 2 + dropTime * LongIntervalValues.MaxPower) / (MediumIntervalValues.DefaultTime * 2 + dropTime);
            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(MediumIntervalValues.DefaultTime * 2 + dropTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(expectedPower, intervalContainer.Intervals[0].AveragePower, 1);
        }

        /// <summary>
        /// Verifies that a medium interval with a short critical power drop is found as one interval.
        /// </summary>
        /// <remarks>
        /// When searching with medium configuration in a session with one medium interval
        /// divided by a 5 second ~50% power drop, the finder will find the full interval.
        /// </remarks>
        [TestMethod]
        public void Medium_MediumIntervalCriticalDrop_OneFound()
        {
            int dropTime = 20;
            int dropPower = 135;
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime, Power = MediumIntervalValues.MaxPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = dropTime, Power = dropPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime, Power = MediumIntervalValues.MaxPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Medium, MediumThresholds
            );
            finder.Search();

            // Assertions
            int expectedPower = (MediumIntervalValues.DefaultTime * MediumIntervalValues.MaxPower * 2 + dropTime * dropPower) / (MediumIntervalValues.DefaultTime * 2 + dropTime);
            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(MediumIntervalValues.DefaultTime * 2 + dropTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(expectedPower, intervalContainer.Intervals[0].AveragePower, 1);
        }

        /// <summary>
        /// Verifies that a medium interval with a power drop is found as two separate intervals.
        /// </summary>
        /// <remarks>
        /// When searching with medium configuration in a session with one medium interval
        /// divided by a 60 second ~30% power drop, the finder will find two separate intervals.
        /// Even if the average power for the full interval is higher than the low limit.
        /// </remarks>
        [TestMethod]
        public void Medium_MediumIntervalDrop_TwoFound()
        {
            int dropTime = 60;
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime, Power = MediumIntervalValues.MaxPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = dropTime, Power = LongIntervalValues.MinPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime, Power = MediumIntervalValues.MaxPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Medium, MediumThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(2, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(MediumIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(MediumIntervalValues.MaxPower, intervalContainer.Intervals[0].AveragePower);
            // Second interval
            Assert.AreEqual(DefaultStartDate.AddSeconds(MediumIntervalValues.DefaultTime + dropTime), intervalContainer.Intervals[1].StartTime);
            Assert.AreEqual(MediumIntervalValues.DefaultTime, intervalContainer.Intervals[1].TimeDiff);
            Assert.AreEqual(MediumIntervalValues.MaxPower, intervalContainer.Intervals[1].AveragePower);
        }

        /// <summary>
        /// Verifies that a medium interval with a power lift is found as one interval.
        /// </summary>
        /// <remarks>
        /// When searching with medium configuration in a session with one medium interval
        /// divided by a 60 second ~15% power lift, the finder will find the full interval.
        /// </remarks>
        [TestMethod]
        public void Medium_MediumIntervalLift_OneFound()
        {
            int liftTime = 60;
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime, Power = MediumIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = liftTime, Power = ShortIntervalValues.MinPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime, Power = MediumIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Medium, MediumThresholds
            );
            finder.Search();

            // Assertions
            int expectedPower = (MediumIntervalValues.DefaultTime * MediumIntervalValues.DefaultPower * 2 + liftTime * ShortIntervalValues.MinPower) / (MediumIntervalValues.DefaultTime * 2 + liftTime);
            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(MediumIntervalValues.DefaultTime * 2 + liftTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(expectedPower, intervalContainer.Intervals[0].AveragePower, 1);
        }

        /// <summary>
        /// Verifies that a medium interval with a power lift is found as two separate intervals.
        /// </summary>
        /// <remarks>
        /// When searching with medium configuration in a session with one medium interval
        /// divided by a 60 second ~35% power lift, the finder will find two separate intervals.
        /// Even if the average power for the full interval is lower than the high limit.
        /// </remarks>
        [TestMethod]
        public void Medium_MediumIntervalLift_TwoFound()
        {
            int liftTime = 60;
            int liftPower = 350;
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime, Power = MediumIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = liftTime, Power = liftPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime, Power = MediumIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Medium, MediumThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(2, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(MediumIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(MediumIntervalValues.DefaultPower, intervalContainer.Intervals[0].AveragePower);
            // Second interval
            Assert.AreEqual(DefaultStartDate.AddSeconds(MediumIntervalValues.DefaultTime + liftTime), intervalContainer.Intervals[1].StartTime);
            Assert.AreEqual(MediumIntervalValues.DefaultTime, intervalContainer.Intervals[1].TimeDiff);
            Assert.AreEqual(MediumIntervalValues.DefaultPower, intervalContainer.Intervals[1].AveragePower);
        }

        /// <summary>
        /// Verifies that an unstable interval won't be found.
        /// </summary>
        /// <remarks>
        /// With the average power and size of the interval, the intervals should 
        /// be detected. But given its changes, it can't be found.
        /// </remarks>
        [TestMethod]
        public void Medium_UnstableMedium_Found()
        {
            int totalChanges = 6;
            int constantPowerTime = 30;
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>();
            for (int i = 0; i < totalChanges; i++)
            {
                fitnessTestSections.Add(
                    new FitnessSection { Time = constantPowerTime, Power = ShortIntervalValues.MinPower, HearRate = 120, Cadence = 85 }
                    );
                fitnessTestSections.Add(
                    new FitnessSection { Time = constantPowerTime, Power = LongIntervalValues.MinPower, HearRate = 120, Cadence = 85 }
                    );
            }
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Medium, MediumThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(0, intervalContainer.Intervals.Count);
        }

        /// <summary>
        /// Verifies that a medium interval is not found with long configuration.
        /// </summary>
        /// <remarks>
        /// When searching with long configuration in a session with one medium interval,
        /// the interval won't be found.
        /// </remarks>
        [TestMethod]
        public void Long_SingleMediumInterval_NotFound()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = MediumIntervalValues.DefaultTime, Power = MediumIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Long, LongThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(0, intervalContainer.Intervals.Count);
        }

        /// <summary>
        /// Verifies that a long interval is found with long configuration.
        /// </summary>
        /// <remarks>
        /// When searching with long configuration in a session with one long interval,
        /// the interval will be found.
        /// </remarks>
        [TestMethod]
        public void Long_SingleLongInterval_Found()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = LongIntervalValues.DefaultTime, Power = LongIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Long, LongThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(LongIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(LongIntervalValues.DefaultPower, intervalContainer.Intervals[0].AveragePower);
        }

        /// <summary>
        /// Verifies that a long interval is found with long configuration (default thresholds).
        /// </summary>
        /// <remarks>
        /// When searching with long configuration (default thresholds) in a session with one long interval,
        /// the interval will be found.
        /// </remarks>
        [TestMethod]
        public void LongDefault_SingleLongInterval_Found()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = LongIntervalValues.DefaultTime, Power = LongIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = NuleIntervalValues.DefaultTime, Power = NuleIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Long
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate.AddSeconds(NuleIntervalValues.DefaultTime), intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(LongIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(LongIntervalValues.DefaultPower, intervalContainer.Intervals[0].AveragePower);
        }

        /// <summary>
        /// Verifies that a very long interval is found with long configuration.
        /// </summary>
        /// <remarks>
        /// When searching with long configuration in a session with one very long interval,
        /// the interval will be found.
        /// </remarks>
        [TestMethod]
        public void Long_VeryLongInterval_Found()
        {
            int intervalTime = 60 * 60 * 2; // 2 hours
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = intervalTime, Power = LongIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Long, LongThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(intervalTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(LongIntervalValues.DefaultPower, intervalContainer.Intervals[0].AveragePower);
        }

        /// <summary>
        /// Verifies that a long interval with a high power is found with long configuration.
        /// </summary>
        /// <remarks>
        /// This tests the improbable scenario where a long size interval with the power of a 
        /// medium size interval appears in the session. When searching with long configuration, 
        /// the finder will find the interval.
        /// </remarks>
        [TestMethod]
        public void Long_LongIntervalHighPower_Found()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = LongIntervalValues.DefaultTime, Power = MediumIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Long, LongThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(LongIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(MediumIntervalValues.DefaultPower, intervalContainer.Intervals[0].AveragePower);
        }

        /// <summary>
        /// Verifies that a long interval with a power drop is found as one interval.
        /// </summary>
        /// <remarks>
        /// When searching with long configuration in a session with one long interval
        /// divided by a 240 second ~20% power drop, the finder will find the full interval.
        /// </remarks>
        [TestMethod]
        public void Long_LongIntervalDrop_OneFound()
        {
            int dropTime = 240;
            int dropPower = 165;
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = LongIntervalValues.DefaultTime / 2, Power = LongIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = dropTime, Power = dropPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = LongIntervalValues.DefaultTime / 2, Power = LongIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Long, LongThresholds
            );
            finder.Search();

            // Assertions
            int expectedPower = (LongIntervalValues.DefaultTime * LongIntervalValues.DefaultPower + dropTime * dropPower) / (LongIntervalValues.DefaultTime + dropTime);
            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(LongIntervalValues.DefaultTime + dropTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(expectedPower, intervalContainer.Intervals[0].AveragePower, 1);
        }

        /// <summary>
        /// Verifies that a long interval with a power drop is found as two separate intervals.
        /// </summary>
        /// <remarks>
        /// When searching with long configuration in a session with one long interval
        /// divided by a 240 second ~35% power drop, the finder will find two separate intervals.
        /// Even if the average power for the full interval is higher than the low limit.
        /// </remarks>
        [TestMethod]
        public void Long_LongIntervalDrop_TwoFound()
        {
            int dropTime = 240;
            int dropPower = 140;
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = LongIntervalValues.DefaultTime, Power = LongIntervalValues.MaxPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = dropTime, Power = dropPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = LongIntervalValues.DefaultTime, Power = LongIntervalValues.MaxPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Long, LongThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(2, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(LongIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(LongIntervalValues.MaxPower, intervalContainer.Intervals[0].AveragePower);
            // Second interval
            Assert.AreEqual(DefaultStartDate.AddSeconds(LongIntervalValues.DefaultTime + dropTime), intervalContainer.Intervals[1].StartTime);
            Assert.AreEqual(LongIntervalValues.DefaultTime, intervalContainer.Intervals[1].TimeDiff);
            Assert.AreEqual(LongIntervalValues.MaxPower, intervalContainer.Intervals[1].AveragePower);
        }

        /// <summary>
        /// Verifies that a long interval with a power lift is found as one interval.
        /// </summary>
        /// <remarks>
        /// When searching with long configuration in a session with one long interval
        /// divided by a 240 second ~25% power lift, the finder will find the full interval.
        /// </remarks>
        [TestMethod]
        public void Long_LongIntervalLift_OneFound()
        {
            int liftTime = 240;
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = LongIntervalValues.DefaultTime / 2, Power = LongIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = liftTime, Power = MediumIntervalValues.MinPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = LongIntervalValues.DefaultTime / 2, Power = LongIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Long, LongThresholds
            );
            finder.Search();

            // Assertions
            int expectedPower = (LongIntervalValues.DefaultTime * LongIntervalValues.DefaultPower + liftTime * MediumIntervalValues.MinPower) / (LongIntervalValues.DefaultTime + liftTime);
            Assert.AreEqual(1, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(LongIntervalValues.DefaultTime + liftTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(expectedPower, intervalContainer.Intervals[0].AveragePower, 1);
        }

        /// <summary>
        /// Verifies that a long interval with a power lift is found as two separate intervals.
        /// </summary>
        /// <remarks>
        /// When searching with long configuration in a session with one long interval
        /// divided by a 240 second ~45% power lift, the finder will find two separate intervals.
        /// Even if the average power for the full interval is lower than the high limit.
        /// </remarks>
        [TestMethod]
        public void Long_LongIntervalLift_TwoFound()
        {
            int liftTime = 240;
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = LongIntervalValues.DefaultTime, Power = LongIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = liftTime, Power = ShortIntervalValues.MinPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = LongIntervalValues.DefaultTime, Power = LongIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Long, LongThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(2, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(LongIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(LongIntervalValues.DefaultPower, intervalContainer.Intervals[0].AveragePower);
            // Second interval 
            Assert.AreEqual(DefaultStartDate.AddSeconds(LongIntervalValues.DefaultTime + liftTime), intervalContainer.Intervals[1].StartTime);
            Assert.AreEqual(LongIntervalValues.DefaultTime, intervalContainer.Intervals[1].TimeDiff);
            Assert.AreEqual(LongIntervalValues.DefaultPower, intervalContainer.Intervals[1].AveragePower);
        }

        /// <summary>
        /// Verifies that a when the start of one interval is just after
        /// the end of other interval, both intervals are found correctly.
        /// </summary>
        /// <remarks>
        /// There will be two intervals, both long sized, but the second will be at a higher power
        /// than the first one. The power differential will separate them. 
        /// Both intervals must be found correctly, special attention in the second's start date.
        /// </remarks>
        [TestMethod]
        public void Long_TwoIntervalAdjacent_TwoFound()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = LongIntervalValues.DefaultTime, Power = LongIntervalValues.DefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = LongIntervalValues.DefaultTime, Power = ShortIntervalValues.MinPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer fitnessDataContainer = new FitnessDataContainer(fitnessData);
            IntervalContainer intervalContainer = new IntervalContainer();
            IntervalsFinder finder = new IntervalsFinder(
                fitnessDataContainer, intervalContainer, PowerZones,
                IntervalSeachGroups.Long, LongThresholds
            );
            finder.Search();

            // Assertions
            Assert.AreEqual(2, intervalContainer.Intervals.Count);
            Assert.AreEqual(DefaultStartDate, intervalContainer.Intervals[0].StartTime);
            Assert.AreEqual(LongIntervalValues.DefaultTime, intervalContainer.Intervals[0].TimeDiff);
            Assert.AreEqual(LongIntervalValues.DefaultPower, intervalContainer.Intervals[0].AveragePower);
            // Second interval 
            Assert.AreEqual(DefaultStartDate.AddSeconds(LongIntervalValues.DefaultTime), intervalContainer.Intervals[1].StartTime);
            Assert.AreEqual(LongIntervalValues.DefaultTime, intervalContainer.Intervals[1].TimeDiff);
            Assert.AreEqual(ShortIntervalValues.MinPower, intervalContainer.Intervals[1].AveragePower);
        }
    }
}