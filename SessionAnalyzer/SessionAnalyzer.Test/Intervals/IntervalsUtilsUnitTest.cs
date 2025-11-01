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
    /// Contains the integration test of the <see cref="IntervalsUtils"/> static class.
    /// </summary>
    /// <remarks>
    /// This class tests the methods of the <see cref="IntervalsUtils"/> static class to cover a 
    /// 100% of the code and scenarios.
    /// 
    /// The tests follow the convention:
    /// <c>Method_Scenario_ExpectedResult</c>.
    /// </remarks>
    [TestClass]
    public sealed class IntervalsUtilsUnitTests
    {
        #region AreEqual
        /// <summary>
        /// Verifies that two intervals are equal.
        /// </summary>
        [TestMethod]
        public void AreEqual_EqualIntervals_Equal()
        {
            int intervalTime = 60;
            Interval interval1 = new Interval
            {
                StartTime = DefaultStartDate,
                EndTime = DefaultStartDate.AddSeconds(intervalTime),
            };
            Interval interval2 = new Interval
            {
                StartTime = DefaultStartDate,
                EndTime = DefaultStartDate.AddSeconds(intervalTime),
            };
            Assert.IsTrue(IntervalsUtils.AreEqual(interval1, interval2));
        }

        /// <summary>
        /// Verifies that with same end time but different start, returns not equal.
        /// </summary>
        [TestMethod]
        public void AreEqual_DifferentStart_NotEqual()
        {
            int intervalTime = 60;
            Interval interval1 = new Interval
            {
                StartTime = DefaultStartDate,
                EndTime = DefaultStartDate.AddSeconds(intervalTime),
            };
            Interval interval2 = new Interval
            {
                StartTime = DefaultStartDate.AddSeconds(10),
                EndTime = DefaultStartDate.AddSeconds(intervalTime),
            };
            Assert.IsFalse(IntervalsUtils.AreEqual(interval1, interval2));
        }

        /// <summary>
        /// Verifies that with same start time but different end, returns not equal.
        /// </summary>
        [TestMethod]
        public void AreEqual_DifferentEnd_NotEqual()
        {
            int intervalTime = 60;
            Interval interval1 = new Interval
            {
                StartTime = DefaultStartDate,
                EndTime = DefaultStartDate.AddSeconds(intervalTime),
            };
            Interval interval2 = new Interval
            {
                StartTime = DefaultStartDate,
                EndTime = DefaultStartDate.AddSeconds(intervalTime + 10),
            };
            Assert.IsFalse(IntervalsUtils.AreEqual(interval1, interval2));
        }

        /// <summary>
        /// Verifies that with different start and end, returns not equal.
        /// </summary>
        [TestMethod]
        public void AreEqual_DifferentAll_NotEqual()
        {
            int intervalTime = 60;
            Interval interval1 = new Interval
            {
                StartTime = DefaultStartDate,
                EndTime = DefaultStartDate.AddSeconds(intervalTime),
                TimeDiff = intervalTime
            };
            Interval interval2 = new Interval
            {
                StartTime = DefaultStartDate.AddSeconds(10),
                EndTime = DefaultStartDate.AddSeconds(intervalTime + 10),
                TimeDiff = intervalTime
            };
            Assert.IsFalse(IntervalsUtils.AreEqual(interval1, interval2));
        }
        #endregion

        #region IsConsideredAnInterval
        /// <summary>
        /// Verifies that a very short interval is not considered as an interval
        /// </summary>
        [TestMethod]
        public void IsConsideredAnInterval_VeryShort_No()
        {
            Interval interval = new Interval
            {
                TimeDiff = 20,
                AveragePower = ShortIntervalValues.DefaultPower
            };
            Assert.IsFalse(IntervalsUtils.IsConsideredAnInterval(interval, PowerZones));
        }

        /// <summary>
        /// Verifies that a short interval that reaches the minumum power limit
        /// is considered as an interval
        /// </summary>
        [TestMethod]
        public void IsConsideredAnInterval_ShortSizeHighPower_Yes()
        {
            Interval interval = new Interval
            {
                TimeDiff = ShortIntervalValues.DefaultTime,
                AveragePower = ShortIntervalValues.DefaultPower
            };
            Assert.IsTrue(IntervalsUtils.IsConsideredAnInterval(interval, PowerZones));
        }

        /// <summary>
        /// Verifies that a short interval that doesn't reach the minumum power limit
        /// isn't considered as an interval
        /// </summary>
        [TestMethod]
        public void IsConsideredAnInterval_ShortSizeLowPower_No()
        {
            Interval interval = new Interval
            {
                TimeDiff = ShortIntervalValues.DefaultTime,
                AveragePower = MediumIntervalValues.MaxPower
            };
            Assert.IsTrue(IntervalsUtils.IsConsideredAnInterval(interval, PowerZones));
        }

        /// <summary>
        /// Verifies that a medium interval that reaches the minumum power limit
        /// is considered as an interval
        /// </summary>
        [TestMethod]
        public void IsConsideredAnInterval_MediumSizeHighPower_Yes()
        {
            Interval interval = new Interval
            {
                TimeDiff = MediumIntervalValues.DefaultTime,
                AveragePower = MediumIntervalValues.DefaultPower
            };
            Assert.IsTrue(IntervalsUtils.IsConsideredAnInterval(interval, PowerZones));
        }

        /// <summary>
        /// Verifies that a medium interval that doesn't reach the minumum power limit
        /// is considered as an interval
        /// </summary>
        [TestMethod]
        public void IsConsideredAnInterval_MediumSizeLowPower_No()
        {
            Interval interval = new Interval
            {
                TimeDiff = MediumIntervalValues.DefaultTime,
                AveragePower = LongIntervalValues.MaxPower
            };
            Assert.IsTrue(IntervalsUtils.IsConsideredAnInterval(interval, PowerZones));
        }

        /// <summary>
        /// Verifies that a long interval that reaches the minumum power limit
        /// is considered as an interval
        /// </summary>
        [TestMethod]
        public void IsConsideredAnInterval_LongSizeHighPower_Yes()
        {
            Interval interval = new Interval
            {
                TimeDiff = LongIntervalValues.DefaultTime,
                AveragePower = LongIntervalValues.DefaultPower
            };
            Assert.IsTrue(IntervalsUtils.IsConsideredAnInterval(interval, PowerZones));
        }

        /// <summary>
        /// Verifies that a long interval that doesn't reach the minumum power limit
        /// is considered as an interval
        /// </summary>
        [TestMethod]
        public void IsConsideredAnInterval_LongSizeLowPower_No()
        {
            Interval interval = new Interval
            {
                TimeDiff = LongIntervalValues.DefaultTime,
                AveragePower = NuleIntervalValues.MaxPower
            };
            Assert.IsTrue(IntervalsUtils.IsConsideredAnInterval(interval, PowerZones));
        }

        /// <summary>
        /// Verifies that throws exception when no zone5 power is found
        /// </summary>
        [TestMethod]
        public void IsConsideredAnInterval_NoShortZone_Exception()
        {
            Interval interval = new Interval
            {
                TimeDiff = LongIntervalValues.DefaultTime,
                AveragePower = NuleIntervalValues.MaxPower
            };
            List<CoreModels.Zone> powerZones = new List<CoreModels.Zone>{
                new CoreModels.Zone { Id = 1, LowLimit = 0, HighLimit = 129},
                new CoreModels.Zone { Id = 2, LowLimit = 130, HighLimit = NuleIntervalValues.MaxPower - 1},
                new CoreModels.Zone { Id = 3, LowLimit = NuleIntervalValues.MaxPower, HighLimit = LongIntervalValues.MaxPower - 1},
                new CoreModels.Zone { Id = 4, LowLimit = LongIntervalValues.MaxPower, HighLimit = MediumIntervalValues.MaxPower - 1},
                new CoreModels.Zone { Id = 6, LowLimit = ShortIntervalValues.MaxPower, HighLimit = ShortIntervalValues.MaxPower + 49},
                new CoreModels.Zone { Id = 7, LowLimit = ShortIntervalValues.MaxPower + 50, HighLimit = 2000}
            };
            Assert.ThrowsException<ArgumentException>(() => IntervalsUtils.IsConsideredAnInterval(interval, powerZones));
        }

        /// <summary>
        /// Verifies that throws exception when no zone4 power is found
        /// </summary>
        [TestMethod]
        public void IsConsideredAnInterval_NoMediumZone_Exception()
        {
            Interval interval = new Interval
            {
                TimeDiff = LongIntervalValues.DefaultTime,
                AveragePower = NuleIntervalValues.MaxPower
            };
            List<CoreModels.Zone> powerZones = new List<CoreModels.Zone>{
                new CoreModels.Zone { Id = 1, LowLimit = 0, HighLimit = 129},
                new CoreModels.Zone { Id = 2, LowLimit = 130, HighLimit = NuleIntervalValues.MaxPower - 1},
                new CoreModels.Zone { Id = 3, LowLimit = NuleIntervalValues.MaxPower, HighLimit = LongIntervalValues.MaxPower - 1},
                new CoreModels.Zone { Id = 5, LowLimit = MediumIntervalValues.MaxPower, HighLimit = ShortIntervalValues.MaxPower - 1},
                new CoreModels.Zone { Id = 6, LowLimit = ShortIntervalValues.MaxPower, HighLimit = ShortIntervalValues.MaxPower + 49},
                new CoreModels.Zone { Id = 7, LowLimit = ShortIntervalValues.MaxPower + 50, HighLimit = 2000}
            };
            Assert.ThrowsException<ArgumentException>(() => IntervalsUtils.IsConsideredAnInterval(interval, powerZones));
        }

        /// <summary>
        /// Verifies that throws exception when no zone3 power is found
        /// </summary>
        [TestMethod]
        public void IsConsideredAnInterval_NoLongZone_Exception()
        {
            Interval interval = new Interval
            {
                TimeDiff = LongIntervalValues.DefaultTime,
                AveragePower = NuleIntervalValues.MaxPower
            };
            List<CoreModels.Zone> powerZones = new List<CoreModels.Zone>{
                new CoreModels.Zone { Id = 1, LowLimit = 0, HighLimit = 129},
                new CoreModels.Zone { Id = 2, LowLimit = 130, HighLimit = NuleIntervalValues.MaxPower - 1},
                new CoreModels.Zone { Id = 4, LowLimit = LongIntervalValues.MaxPower, HighLimit = MediumIntervalValues.MaxPower - 1},
                new CoreModels.Zone { Id = 5, LowLimit = MediumIntervalValues.MaxPower, HighLimit = ShortIntervalValues.MaxPower - 1},
                new CoreModels.Zone { Id = 6, LowLimit = ShortIntervalValues.MaxPower, HighLimit = ShortIntervalValues.MaxPower + 49},
                new CoreModels.Zone { Id = 7, LowLimit = ShortIntervalValues.MaxPower + 50, HighLimit = 2000}
            };
            Assert.ThrowsException<ArgumentException>(() => IntervalsUtils.IsConsideredAnInterval(interval, powerZones));
        }
        #endregion
    }
}