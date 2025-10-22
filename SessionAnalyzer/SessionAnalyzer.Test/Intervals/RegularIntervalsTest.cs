using CyclingTrainer.SessionReader.Models;
using CyclingTrainer.SessionAnalyzer.Test.Models;
using CyclingTrainer.SessionAnalyzer.Models;
using CyclingTrainer.SessionAnalyzer.Services.Intervals;
using static CyclingTrainer.SessionAnalyzer.Test.Intervals.IntervalsConstants;

namespace CyclingTrainer.SessionAnalyzer.Test.Intervals
{
    [TestClass]
    public sealed class RegularIntervalsTest
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
        public void NoIntervals()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>();
            for (int i = 0; i < 100; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 5, Power = NulePowerDefaultValue * 9 / 10, HearRate = 90, Cadence = 80 });
                fitnessTestSections.Add(new FitnessSection { Time = 5, Power = NulePowerDefaultValue * 11 / 10, HearRate = 90, Cadence = 80 });
            }
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
            Assert.AreEqual(0, intervals.Count);
        }

        [TestMethod]
        public void UniqueMedium()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = MediumDefaultTime, Power = MediumDefaultPower, HearRate = 120, Cadence = 85}
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
            Assert.AreEqual(1, intervals.Count);
            Assert.AreEqual(MediumDefaultPower, intervals.First().AveragePower);
            Assert.AreEqual(MediumDefaultTime, intervals.First().TimeDiff);
        }

        [TestMethod]
        public void NuleMediumNule()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = 300, Power = 150, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = MediumDefaultTime, Power = MediumDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 300, Power = 150, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
            Assert.AreEqual(1, intervals.Count);
            Assert.AreEqual(MediumDefaultPower, intervals.First().AveragePower);
            Assert.AreEqual(MediumDefaultTime, intervals.First().TimeDiff);
        }

        [TestMethod]
        public void ShortMediumShort()      // "Small" differences should not be detected, short-medium-short is one interval with two short intervals inside
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = ShortDefaultTime, Power = ShortDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = MediumDefaultTime, Power = MediumDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = ShortDefaultTime, Power = ShortDefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
            Assert.AreEqual(1, intervals.Count);
            int expectedTime = ShortDefaultTime * 2 + MediumDefaultTime;
            float expectedPower = (ShortDefaultTime * ShortDefaultPower * 2 + MediumDefaultTime * MediumDefaultPower) / expectedTime;
            Assert.AreEqual(expectedTime, intervals[0].TimeDiff);
            Assert.AreEqual(expectedPower, intervals[0].AveragePower, 1f);
            Assert.AreEqual(2, intervals[0].Intervals?.Count);
            Assert.AreEqual(ShortDefaultTime, intervals[0].Intervals?[0].TimeDiff);
            Assert.AreEqual(ShortDefaultPower, intervals[0].Intervals?[0].AveragePower);
            Assert.AreEqual(ShortDefaultTime, intervals[0].Intervals?[1].TimeDiff);
            Assert.AreEqual(ShortDefaultPower, intervals[0].Intervals?[1].AveragePower);
        }

        [TestMethod]
        public void MediumLongMedium()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = MediumDefaultTime, Power = MediumDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = LongDefaultTime, Power = LongDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = MediumDefaultTime, Power = MediumDefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
            Assert.AreEqual(1, intervals.Count);
            int expectedTime = LongDefaultTime + MediumDefaultTime * 2;
            float expectedPower = (MediumDefaultTime * MediumDefaultPower * 2 + LongDefaultTime * LongDefaultPower) / expectedTime;
            Assert.AreEqual(expectedTime, intervals[0].TimeDiff);
            Assert.AreEqual(expectedPower, intervals[0].AveragePower, 1f);
            Assert.AreEqual(2, intervals[0].Intervals?.Count);
            Assert.AreEqual(MediumDefaultTime, intervals[0].Intervals?[0].TimeDiff);
            Assert.AreEqual(MediumDefaultPower, intervals[0].Intervals?[0].AveragePower);
            Assert.AreEqual(MediumDefaultTime, intervals[0].Intervals?[1].TimeDiff);
            Assert.AreEqual(MediumDefaultPower, intervals[0].Intervals?[1].AveragePower);
        }

        [TestMethod]
        public void ShortLongShort()        // Bigger differences must be detected, short-long-short are three different intervals
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = ShortDefaultTime, Power = ShortDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = LongDefaultTime, Power = LongDefaultPower, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = ShortDefaultTime, Power = ShortDefaultPower, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
            Assert.AreEqual(3, intervals.Count);
            Assert.AreEqual(ShortDefaultTime, intervals[0].TimeDiff);
            Assert.AreEqual(ShortDefaultPower, intervals[0].AveragePower);
            Assert.AreEqual(LongDefaultTime, intervals[1].TimeDiff);
            Assert.AreEqual(LongDefaultPower, intervals[1].AveragePower);
            Assert.AreEqual(ShortDefaultTime, intervals[2].TimeDiff);
            Assert.AreEqual(ShortDefaultPower, intervals[2].AveragePower);
        }
    }
}