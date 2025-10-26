using CyclingTrainer.SessionAnalyzer.Models;
using CyclingTrainer.SessionAnalyzer.Services.Intervals;
using CyclingTrainer.SessionAnalyzer.Test.Models;
using CyclingTrainer.SessionReader.Models;

namespace CyclingTrainer.SessionAnalyzer.Test.Intervals
{
    [TestClass]
    public sealed class SprintTest
    {
        List<FitnessData> _fitnessData = default!;

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
        public void NormalSprint()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = 10, Power = 150, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 5, Power = 600, HearRate = 150, Cadence = 90},
                new FitnessSection{ Time = 5, Power = 550, HearRate = 150, Cadence = 90},
                new FitnessSection{ Time = 10, Power = 150, HearRate = 180, Cadence = 95},
            };
            _fitnessData = FitnessDataService.SetData(fitnessTestSections);
            SprintService.SetConfiguration(5, 550, 500);
            SprintService.AnalyzeActivity(_fitnessData);
            List<Interval> sprints = IntervalRepository.GetSprints();
            Assert.AreEqual(1, sprints.Count);
            Assert.AreEqual(575, sprints.First().AveragePower);
            Assert.AreEqual(10, sprints.First().TimeDiff);
            List<FitnessData> remainingPoints = IntervalRepository.GetRemainingFitnessData();
            Assert.AreEqual(20, remainingPoints.Count);
        }

        [TestMethod]
        public void OneSecStopSprint()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = 10, Power = 150, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 5, Power = 600, HearRate = 150, Cadence = 90},
                new FitnessSection{ Time = 1, Power = 0, HearRate = 150, Cadence = 90},
                new FitnessSection{ Time = 5, Power = 550, HearRate = 150, Cadence = 90},
                new FitnessSection{ Time = 10, Power = 150, HearRate = 180, Cadence = 95},
            };
            _fitnessData = FitnessDataService.SetData(fitnessTestSections);
            SprintService.SetConfiguration(5, 550, 500);
            SprintService.AnalyzeActivity(_fitnessData);
            List<Interval> sprints = IntervalRepository.GetSprints();
            Assert.AreEqual(1, sprints.Count);
            Assert.AreEqual((float)5750/11, sprints.First().AveragePower);
            Assert.AreEqual(11, sprints.First().TimeDiff);
            List<FitnessData> remainingPoints = IntervalRepository.GetRemainingFitnessData();
            Assert.AreEqual(20, remainingPoints.Count);
        }

        [TestMethod]
        public void TwoSecStopSprint()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = 10, Power = 150, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 5, Power = 600, HearRate = 150, Cadence = 90},
                new FitnessSection{ Time = 2, Power = 0, HearRate = 150, Cadence = 90},
                new FitnessSection{ Time = 5, Power = 550, HearRate = 150, Cadence = 90},
                new FitnessSection{ Time = 10, Power = 150, HearRate = 180, Cadence = 95},
            };
            _fitnessData = FitnessDataService.SetData(fitnessTestSections);
            SprintService.SetConfiguration(5, 550, 500);
            SprintService.AnalyzeActivity(_fitnessData);
            List<Interval> sprints = IntervalRepository.GetSprints();
            Assert.AreEqual(2, sprints.Count);
            Assert.AreEqual(600, sprints.First().AveragePower);
            Assert.AreEqual(550, sprints.Last().AveragePower);
            List<FitnessData> remainingPoints = IntervalRepository.GetRemainingFitnessData();
            Assert.AreEqual(22, remainingPoints.Count);
        }

        [TestMethod]
        public void InputErrorMinTime()
        {
            try
            {
                SprintService.SetConfiguration(-1, 550, 500);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("Minimum sprint time must be positive"));
            }
        }

        [TestMethod]
        public void InputErrorHysteresis()
        {
            try
            {
                SprintService.SetConfiguration(5, 500, 550);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("Start trigger must be greater than end trigger for hysteresis to work"));
            }
        }
    }
}