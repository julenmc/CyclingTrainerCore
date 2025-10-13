using CoreModels = CyclingTrainer.Core.Models;
using CyclingTrainer.SessionReader.Core.Models;
using CyclingTrainer.SessionAnalyzer.Test.Models;
using CyclingTrainer.SessionAnalyzer.Core.Models;
using CyclingTrainer.SessionAnalyzer.Core.Services.Intervals;
using static CyclingTrainer.SessionAnalyzer.Test.Intervals.IntervalsConstants;

namespace CyclingTrainer.SessionAnalyzer.Test.Intervals
{
    [TestClass]
    public sealed class IrregularIntervalsTest
    {
        private static readonly List<CoreModels.Zone> PowerZones = new List<CoreModels.Zone>{
            new CoreModels.Zone { Id = 1, LowLimit = 0, HighLimit = 129},
            new CoreModels.Zone { Id = 2, LowLimit = 130, HighLimit = NuleMaxValue - 1},
            new CoreModels.Zone { Id = 3, LowLimit = NuleMaxValue, HighLimit = LongMaxValue - 1},
            new CoreModels.Zone { Id = 4, LowLimit = LongMaxValue, HighLimit = MediumMaxValue - 1},
            new CoreModels.Zone { Id = 5, LowLimit = MediumMaxValue, HighLimit = ShortMaxValue - 1},
            new CoreModels.Zone { Id = 6, LowLimit = ShortMaxValue, HighLimit = ShortMaxValue + 49},
            new CoreModels.Zone { Id = 7, LowLimit = ShortMaxValue + 50, HighLimit = 2000}
        };
        private readonly Random rnd = new Random();
        
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
        public void UniqueShort()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>();
            for (int i = 0; i < 200; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(ShortMinValue, ShortMaxValue), HearRate = 90, Cadence = 80 });
            }
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
            Assert.AreEqual(1, intervals.Count);
            float expectedTime = 200;
            Assert.AreEqual(expectedTime, intervals.First().TimeDiff, expectedTime * ShortAcpDelta);
            Assert.IsTrue(intervals.First().AveragePower < ShortMaxValue && intervals.First().AveragePower > ShortMinValue);
        }

        [TestMethod]
        public void UniqueMedium()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>();
            for (int i = 0; i < 500; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(MediumMinValue, MediumMaxValue), HearRate = 90, Cadence = 80 });
            }
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
            Assert.AreEqual(1, intervals.Count);
            float expectedTime = 500;
            Assert.AreEqual(expectedTime, intervals.First().TimeDiff, expectedTime * MediumAcpDelta);
            Assert.IsTrue(intervals.First().AveragePower < MediumMaxValue && intervals.First().AveragePower > MediumMinValue);
        }

        [TestMethod]
        public void UniqueLong()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>();
            for (int i = 0; i < 1000; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(LongMinValue, LongMaxValue), HearRate = 90, Cadence = 80 });
            }
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
            Assert.AreEqual(1, intervals.Count);
            float expectedTime = 1000;
            Assert.AreEqual(expectedTime, intervals.First().TimeDiff, expectedTime * LongAcpDelta);
            Assert.IsTrue(intervals.First().AveragePower < LongMaxValue && intervals.First().AveragePower > LongMinValue);
        }

        [TestMethod]
        public void UniqueWithSprint()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = 200, Power = 300, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 10, Power = 700, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 200, Power = 300, HearRate = 120, Cadence = 85},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
            Assert.AreEqual(1, intervals.Count);
            Assert.AreEqual(300, intervals.First().AveragePower);
            Assert.AreEqual(410, intervals.First().TimeDiff);
        }

        [TestMethod]
        public void SmallDrop()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>();
            for (int i = 0; i < 100; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(ShortMinValue, ShortMaxValue), HearRate = 90, Cadence = 80 });
            }
            fitnessTestSections.Add(new FitnessSection { Time = 1, Power = 150, HearRate = 90, Cadence = 80 });
            for (int i = 0; i < 100; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(ShortMinValue, ShortMaxValue), HearRate = 90, Cadence = 80 });
            }
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
            Assert.AreEqual(1, intervals.Count);
            float expectedTime = 201;
            Assert.AreEqual(expectedTime, intervals.First().TimeDiff, expectedTime * ShortAcpDelta);
            Assert.IsTrue(intervals.First().AveragePower < ShortMaxValue && intervals.First().AveragePower > ShortMinValue);
        }

        [TestMethod]
        public void NuleShortNule()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>();
            for (int i = 0; i < 500; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(NuleMinValue, NuleMaxValue), HearRate = 90, Cadence = 80 });
            }
            for (int i = 0; i < 200; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(ShortMinValue, ShortMaxValue), HearRate = 90, Cadence = 80 });
            }
            for (int i = 0; i < 500; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(NuleMinValue, NuleMaxValue), HearRate = 90, Cadence = 80 });
            }
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
            Assert.AreEqual(1, intervals.Count);
            float expectedTime = 200;
            Assert.AreEqual(expectedTime, intervals.First().TimeDiff, expectedTime * ShortAcpDelta);
            Assert.IsTrue(intervals.First().AveragePower < ShortMaxValue && intervals.First().AveragePower > ShortMinValue);
        }

        [TestMethod]
        public void ShortMediumShort()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>();
            // Add short
            for (int i = 0; i < 150; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(ShortMinValue, ShortMaxValue), HearRate = 90, Cadence = 80 });
            }
            // Add medium
            for (int i = 0; i < 400; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(MediumMinValue, MediumMaxValue), HearRate = 90, Cadence = 80 });
            }
            // Add short
            for (int i = 0; i < 150; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(ShortMinValue, ShortMaxValue), HearRate = 90, Cadence = 80 });
            }

            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
            Assert.AreEqual(1, intervals.Count);
            float expectedTime = 700;
            Assert.AreEqual(expectedTime, intervals[0].TimeDiff, expectedTime * LongAcpDelta);
            Assert.IsTrue(intervals.First().AveragePower < ShortMaxValue && intervals.First().AveragePower > MediumMinValue);
            Assert.AreEqual(2, intervals[0].Intervals.Count);
            expectedTime = 150;
            Assert.AreEqual(expectedTime, intervals[0].Intervals[0].TimeDiff, expectedTime * ShortAcpDelta);
            Assert.IsTrue(intervals[0].Intervals[0].AveragePower < ShortMaxValue && intervals[0].Intervals[0].AveragePower > ShortMinValue);
            Assert.AreEqual(expectedTime, intervals[0].Intervals[1].TimeDiff, expectedTime * ShortAcpDelta);
            Assert.IsTrue(intervals[0].Intervals[1].AveragePower < ShortMaxValue && intervals[0].Intervals[1].AveragePower > ShortMinValue);
             
        }

        [TestMethod]
        public void MediumLongMedium()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>();
            // Add medium
            for (int i = 0; i < 400; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(MediumMinValue, MediumMaxValue), HearRate = 90, Cadence = 80 });
            }
            // Add long
            for (int i = 0; i < 1200; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(LongMinValue, LongMaxValue), HearRate = 90, Cadence = 80 });
            }
            // Add medium
            for (int i = 0; i < 400; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(MediumMinValue, MediumMaxValue), HearRate = 90, Cadence = 80 });
            }
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            List<Interval> intervals = IntervalsService.Search(fitnessData, PowerZones);
            Assert.AreEqual(1, intervals.Count);
            float expectedTime = 2000;
            Assert.AreEqual(expectedTime, intervals.First().TimeDiff, expectedTime * LongAcpDelta);
            Assert.IsTrue(intervals.First().AveragePower < MediumMaxValue && intervals.First().AveragePower > LongMinValue);
            Assert.AreEqual(2, intervals[0].Intervals.Count);
            expectedTime = 400;
            Assert.AreEqual(expectedTime, intervals[0].Intervals[0].TimeDiff, expectedTime * MediumAcpDelta);
            Assert.IsTrue(intervals[0].Intervals[0].AveragePower < MediumMaxValue && intervals[0].Intervals[0].AveragePower > MediumMinValue);
            Assert.AreEqual(expectedTime, intervals[0].Intervals[1].TimeDiff, expectedTime * MediumAcpDelta);
            Assert.IsTrue(intervals[0].Intervals[1].AveragePower < MediumMaxValue && intervals[0].Intervals[1].AveragePower > MediumMinValue);
        }
    }
}