using CoreModels = CyclingTrainer.Core.Models;
using CyclingTrainer.SessionReader.Core.Models;
using CyclingTrainer.SessionAnalyzer.Test.Models;
using CyclingTrainer.SessionAnalyzer.Core.Models;
using CyclingTrainer.SessionAnalyzer.Core.Services.Intervals;
using static CyclingTrainer.SessionAnalyzer.Test.Intervals.IntervalsConstants;

namespace CyclingTrainer.SessionAnalyzer.Test.Intervals
{
    [TestClass]
    public sealed class RegularIntervalsTest
    {
        private static readonly List<CoreModels.Zone> PowerZones = new List<CoreModels.Zone>{
            new CoreModels.Zone { Id = 1, LowLimit = 0, HighLimit = 129},
            new CoreModels.Zone { Id = 2, LowLimit = 130, HighLimit = 179},
            new CoreModels.Zone { Id = 3, LowLimit = 180, HighLimit = 214},
            new CoreModels.Zone { Id = 4, LowLimit = 215, HighLimit = 249},
            new CoreModels.Zone { Id = 5, LowLimit = 250, HighLimit = 289},
            new CoreModels.Zone { Id = 6, LowLimit = 290, HighLimit = 359},
            new CoreModels.Zone { Id = 7, LowLimit = 360, HighLimit = 2000}
        };
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
        public void UniqueInterval()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = 360, Power = MediumPowerDefaultValue, HearRate = 120, Cadence = 85}
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
            Assert.AreEqual(1, intervals.Count);
            Assert.AreEqual(MediumPowerDefaultValue, intervals.First().AveragePower);
            Assert.AreEqual(360, intervals.First().TimeDiff);
        }

        [TestMethod]
        public void NuleSimpleNule()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = 300, Power = 150, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 300, Power = MediumPowerDefaultValue, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 300, Power = 150, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
            Assert.AreEqual(1, intervals.Count);
            Assert.AreEqual(MediumPowerDefaultValue, intervals.First().AveragePower);
            Assert.AreEqual(300, intervals.First().TimeDiff);
        }

        [TestMethod]
        public void ShortMediumShort()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = 60, Power = ShortPowerDefaultValue, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 300, Power = MediumPowerDefaultValue, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 60, Power = ShortPowerDefaultValue, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
            Assert.AreEqual(3, intervals.Count);
            Assert.AreEqual(ShortPowerDefaultValue, intervals[0].AveragePower);
            Assert.AreEqual(60, intervals[0].TimeDiff);
            Assert.AreEqual(MediumPowerDefaultValue, intervals[1].AveragePower);
            Assert.AreEqual(300, intervals[1].TimeDiff);
            Assert.AreEqual(ShortPowerDefaultValue, intervals[2].AveragePower);
            Assert.AreEqual(60, intervals[2].TimeDiff);
        }

        [TestMethod]
        public void MediumLongMedium()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = 400, Power = MediumPowerDefaultValue, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 1200, Power = LongPowerDefaultValue, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 400, Power = MediumPowerDefaultValue, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
            Assert.AreEqual(3, intervals.Count);
            Assert.AreEqual(400, intervals[0].TimeDiff);
            Assert.AreEqual(MediumPowerDefaultValue, intervals[0].AveragePower);
            Assert.AreEqual(1200, intervals[1].TimeDiff);
            Assert.AreEqual(LongPowerDefaultValue, intervals[1].AveragePower);
            Assert.AreEqual(400, intervals[2].TimeDiff);   
            Assert.AreEqual(MediumPowerDefaultValue, intervals[2].AveragePower);
        }
    }
}