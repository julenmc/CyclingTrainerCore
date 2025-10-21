using CoreModels = CyclingTrainer.Core.Models;
using CyclingTrainer.SessionReader.Core.Models;
using CyclingTrainer.SessionAnalyzer.Test.Models;
using CyclingTrainer.SessionAnalyzer.Models;
using CyclingTrainer.SessionAnalyzer.Services.Intervals;
using static CyclingTrainer.SessionAnalyzer.Test.Intervals.IntervalsConstants;

namespace CyclingTrainer.SessionAnalyzer.Test.Intervals
{
    [TestClass]
    public sealed class IrregularIntervalsTest
    {
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
            for (int i = 0; i < ShortDefaultTime; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(ShortMinValue, ShortMaxValue), HearRate = 90, Cadence = 80 });
            }
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            IntervalsService service = new IntervalsService(PowerZones);
            List<Interval> intervals = service.Search(fitnessData);
            Assert.AreEqual(1, intervals.Count);
            float expectedTime = ShortDefaultTime;
            Assert.AreEqual(expectedTime, intervals.First().TimeDiff, expectedTime * ShortAcpDelta);
            Assert.IsTrue(intervals.First().AveragePower < ShortMaxValue && intervals.First().AveragePower > ShortMinValue);
        }

        [TestMethod]
        public void UniqueMedium()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>();
            for (int i = 0; i < MediumDefaultTime; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(MediumMinValue, MediumMaxValue), HearRate = 90, Cadence = 80 });
            }
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            IntervalsService service = new IntervalsService(PowerZones);
            List<Interval> intervals = service.Search(fitnessData);
            Assert.AreEqual(1, intervals.Count);
            float expectedTime = MediumDefaultTime;
            Assert.AreEqual(expectedTime, intervals.First().TimeDiff, expectedTime * MediumAcpDelta);
            Assert.IsTrue(intervals.First().AveragePower < MediumMaxValue && intervals.First().AveragePower > MediumMinValue);
        }

        [TestMethod]
        public void UniqueLong()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>();
            for (int i = 0; i < LongDefaultTime; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(LongMinValue, LongMaxValue), HearRate = 90, Cadence = 80 });
            }
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            IntervalsService service = new IntervalsService(PowerZones);
            List<Interval> intervals = service.Search(fitnessData);
            Assert.AreEqual(1, intervals.Count);
            float expectedTime = LongDefaultTime;
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
            IntervalsService service = new IntervalsService(PowerZones);
            List<Interval> intervals = service.Search(fitnessData);
            Assert.AreEqual(1, intervals.Count);
            Assert.AreEqual(300, intervals.First().AveragePower);
            Assert.AreEqual(410, intervals.First().TimeDiff);
        }

        [TestMethod]
        public void SmallDrop()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>();
            for (int i = 0; i < ShortDefaultTime / 2; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(ShortMinValue, ShortMaxValue), HearRate = 90, Cadence = 80 });
            }
            fitnessTestSections.Add(new FitnessSection { Time = 1, Power = 150, HearRate = 90, Cadence = 80 });
            for (int i = 0; i < ShortDefaultTime / 2; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(ShortMinValue, ShortMaxValue), HearRate = 90, Cadence = 80 });
            }
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            IntervalsService service = new IntervalsService(PowerZones);
            List<Interval> intervals = service.Search(fitnessData);
            Assert.AreEqual(1, intervals.Count);
            float expectedTime = ShortDefaultTime + 1;
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
            for (int i = 0; i < ShortDefaultTime; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(ShortMinValue, ShortMaxValue), HearRate = 90, Cadence = 80 });
            }
            for (int i = 0; i < 500; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(NuleMinValue, NuleMaxValue), HearRate = 90, Cadence = 80 });
            }
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            IntervalsService service = new IntervalsService(PowerZones);
            List<Interval> intervals = service.Search(fitnessData);
            Assert.AreEqual(1, intervals.Count);
            float expectedTime = ShortDefaultTime;
            Assert.AreEqual(expectedTime, intervals.First().TimeDiff, expectedTime * ShortAcpDelta);
            Assert.IsTrue(intervals.First().AveragePower < ShortMaxValue && intervals.First().AveragePower > ShortMinValue);
        }

        [TestMethod]
        public void ShortMediumShort()      // "Small" differences should not be detected, short-medium-short is one interval with two short intervals inside
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>();
            // Add short
            for (int i = 0; i < ShortDefaultTime; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(ShortMinValue, ShortMaxValue), HearRate = 90, Cadence = 80 });
            }
            // Add medium
            for (int i = 0; i < MediumDefaultTime; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(MediumMinValue, MediumMaxValue), HearRate = 90, Cadence = 80 });
            }
            // Add short
            for (int i = 0; i < ShortDefaultTime; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(ShortMinValue, ShortMaxValue), HearRate = 90, Cadence = 80 });
            }

            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            IntervalsService service = new IntervalsService(PowerZones);
            List<Interval> intervals = service.Search(fitnessData);
            Assert.AreEqual(1, intervals.Count);
            float expectedTime = MediumDefaultTime + ShortDefaultTime * 2;
            Assert.AreEqual(expectedTime, intervals[0].TimeDiff, expectedTime * LongAcpDelta);
            Assert.IsTrue(intervals.First().AveragePower < ShortMaxValue && intervals.First().AveragePower > MediumMinValue);
            Assert.AreEqual(2, intervals[0].Intervals?.Count);
            Assert.AreEqual(ShortDefaultTime, intervals[0].Intervals[0].TimeDiff, ShortDefaultTime * ShortAcpDelta);
            Assert.IsTrue(intervals[0].Intervals?[0].AveragePower < ShortMaxValue && intervals[0].Intervals?[0].AveragePower > ShortMinValue);
            Assert.AreEqual(ShortDefaultTime, intervals[0].Intervals[1].TimeDiff, ShortDefaultTime * ShortAcpDelta);
            Assert.IsTrue(intervals[0].Intervals?[1].AveragePower < ShortMaxValue && intervals[0].Intervals?[1].AveragePower > ShortMinValue);

        }

        [TestMethod]
        public void MediumLongMedium()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>();
            // Add medium
            for (int i = 0; i < MediumDefaultTime; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(MediumMinValue, MediumMaxValue), HearRate = 90, Cadence = 80 });
            }
            // Add long
            for (int i = 0; i < LongDefaultTime; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(LongMinValue, LongMaxValue), HearRate = 90, Cadence = 80 });
            }
            // Add medium
            for (int i = 0; i < MediumDefaultTime; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(MediumMinValue, MediumMaxValue), HearRate = 90, Cadence = 80 });
            }
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            IntervalsService service = new IntervalsService(PowerZones);
            List<Interval> intervals = service.Search(fitnessData);
            Assert.AreEqual(1, intervals.Count);
            float expectedTime = MediumDefaultTime * 2 + LongDefaultTime;
            Assert.AreEqual(expectedTime, intervals.First().TimeDiff, expectedTime * LongAcpDelta);
            Assert.IsTrue(intervals.First().AveragePower < MediumMaxValue && intervals.First().AveragePower > LongMinValue);
            Assert.AreEqual(2, intervals[0].Intervals?.Count);
            Assert.AreEqual(MediumDefaultTime, intervals[0].Intervals[0].TimeDiff, MediumDefaultTime * MediumAcpDelta);
            Assert.IsTrue(intervals[0].Intervals?[0].AveragePower < MediumMaxValue && intervals[0].Intervals?[0].AveragePower > MediumMinValue);
            Assert.AreEqual(MediumDefaultTime, intervals[0].Intervals[1].TimeDiff, MediumDefaultTime * MediumAcpDelta);
            Assert.IsTrue(intervals[0].Intervals?[1].AveragePower < MediumMaxValue && intervals[0].Intervals?[1].AveragePower > MediumMinValue);
        }

        [TestMethod]
        public void ShortLongShort()      // Bigger differences must be detected, short-long-short are three different intervals
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>();
            // Add short
            for (int i = 0; i < ShortDefaultTime; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(ShortMinValue, ShortMaxValue), HearRate = 90, Cadence = 80 });
            }
            // Add medium
            for (int i = 0; i < LongDefaultTime; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(LongMinValue, LongMaxValue), HearRate = 90, Cadence = 80 });
            }
            // Add short
            for (int i = 0; i < ShortDefaultTime; i++)
            {
                fitnessTestSections.Add(new FitnessSection { Time = 1, Power = rnd.Next(ShortMinValue, ShortMaxValue), HearRate = 90, Cadence = 80 });
            }

            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            IntervalsService service = new IntervalsService(PowerZones);
            List<Interval> intervals = service.Search(fitnessData);
            Assert.AreEqual(3, intervals.Count);
            Assert.AreEqual(ShortDefaultTime, intervals[0].TimeDiff, ShortDefaultTime * ShortAcpDelta);
            Assert.IsTrue(intervals[0].AveragePower < ShortMaxValue && intervals[0].AveragePower > ShortMinValue);
            Assert.AreEqual(LongDefaultTime, intervals[1].TimeDiff, LongDefaultTime * LongAcpDelta);
            Assert.IsTrue(intervals[1].AveragePower < LongMaxValue && intervals[1].AveragePower > LongMinValue);
            Assert.AreEqual(ShortDefaultTime, intervals[2].TimeDiff, ShortDefaultTime * ShortAcpDelta);
            Assert.IsTrue(intervals[2].AveragePower < ShortMaxValue && intervals[2].AveragePower > ShortMinValue);
        }
    }
}