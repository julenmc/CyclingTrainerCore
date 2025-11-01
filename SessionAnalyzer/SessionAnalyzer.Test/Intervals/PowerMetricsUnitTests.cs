using CyclingTrainer.SessionReader.Models;
using CyclingTrainer.SessionAnalyzer.Models;
using CyclingTrainer.SessionAnalyzer.Test.Models;
using CyclingTrainer.SessionAnalyzer.Services.Intervals;

namespace CyclingTrainer.SessionAnalyzer.Test.Intervals
{
    /// <summary>
    /// Contains the unit test of the <see cref="PowerMetricsCalculator"/> service.
    /// </summary>
    /// <remarks>
    /// This class verifies two different scenarios:
    /// Session without pauses and session with pauses
    /// 
    /// The test follow the convention:
    /// <c>Scenario</c>.
    /// </remarks>
    [TestClass]
    public sealed class PowerMetricsTests
    {
        /// <summary>
        /// Verifies that the averages are corrected calcualted when the session has no pauses
        /// </summary>
        [TestMethod]
        public void NoPauses()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = 120, Power = 150, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 120, Power = 250, HearRate = 150, Cadence = 90},
                new FitnessSection{ Time = 120, Power = 250, HearRate = 150, Cadence = 90},
                new FitnessSection{ Time = 120, Power = 150, HearRate = 180, Cadence = 95},
            };
            List<FitnessData> _fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<PowerMetrics> averages = PowerMetricsCalculator.CalculateMovingAverages(_fitnessData, 10, new IntervalContainer());
            Assert.AreEqual(120 * 4 - 9, averages.Count);       // -9 because the 10th already has 10 seconds for the calculation
            Assert.AreEqual(150, averages.First().AvrgPower);
            Assert.AreEqual(0, averages.First().Deviation);
            Assert.AreEqual(0, averages.First().MaxMinDelta);
        }

        /// <summary>
        /// Verifies that the averages are corrected calcualted when the session has one pause
        /// </summary>
        [TestMethod]
        public void WithPauses()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = 20, Power = 150, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 0, Power = 20, HearRate = 0, Cadence = 0},       // 20 second session stop
                new FitnessSection{ Time = 20, Power = 250, HearRate = 150, Cadence = 90},
            };
            List<FitnessData> _fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<PowerMetrics> averages = PowerMetricsCalculator.CalculateMovingAverages(_fitnessData, 10, new IntervalContainer());
            Assert.AreEqual((20 - 9) * 2, averages.Count);       // Two blocks, the first 20 seconds and the last 2. The first 10 seconds of the second block should not be included
            Assert.AreEqual(new DateTime(2025, 07, 14, 12, 00, 49), averages[11].PointDate);
            Assert.AreEqual(250, averages[11].AvrgPower);
        }

        /// <summary>
        /// Verifies that when a sessions point count is smaller
        /// than the window size, a exception is thrown
        /// </summary>
        [TestMethod]
        public void TooSmallSession()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = 5, Power = 150, HearRate = 120, Cadence = 85}
            };
            List<FitnessData> _fitnessData = FitnessDataService.SetData(fitnessTestSections);
            Assert.ThrowsException<ArgumentException>(() => PowerMetricsCalculator.CalculateMovingAverages(_fitnessData, 10, new IntervalContainer()));
        }
    }
}