using CyclingTrainer.SessionAnalyzer.Models;
using CyclingTrainer.SessionAnalyzer.Services.Intervals;
using CyclingTrainer.SessionAnalyzer.Test.Models;
using CyclingTrainer.SessionReader.Models;

namespace CyclingTrainer.SessionAnalyzer.Test.Intervals
{
    [TestClass]
    public sealed class SprintTest
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
        public void NormalSprint()
        {
            List<FitnessSection> fitnessTestSections = new List<FitnessSection>
            {
                new FitnessSection{ Time = 10, Power = 150, HearRate = 120, Cadence = 85},
                new FitnessSection{ Time = 5, Power = 600, HearRate = 150, Cadence = 90},
                new FitnessSection{ Time = 5, Power = 550, HearRate = 150, Cadence = 90},
                new FitnessSection{ Time = 10, Power = 150, HearRate = 180, Cadence = 95},
            };
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer container = new FitnessDataContainer(fitnessData);
            SprintService service = new SprintService(container, 5, 550, 500);
            List<Interval> sprints = service.SearchSprints();
            Assert.AreEqual(1, sprints.Count);
            Assert.AreEqual(575, sprints.First().AveragePower);
            Assert.AreEqual(10, sprints.First().TimeDiff);
            List<FitnessData> remainingPoints = container.FitnessData;
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
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer container = new FitnessDataContainer(fitnessData);
            SprintService service = new SprintService(container, 5, 550, 500);
            List<Interval> sprints = service.SearchSprints();
            Assert.AreEqual(1, sprints.Count);
            Assert.AreEqual((float)5750/11, sprints.First().AveragePower);
            Assert.AreEqual(11, sprints.First().TimeDiff);
            List<FitnessData> remainingPoints = container.FitnessData;
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
            List<FitnessData> fitnessData = FitnessDataService.SetData(fitnessTestSections);
            FitnessDataContainer container = new FitnessDataContainer(fitnessData);
            SprintService service = new SprintService(container, 5, 550, 500);
            List<Interval> sprints = service.SearchSprints();
            Assert.AreEqual(2, sprints.Count);
            Assert.AreEqual(600, sprints.First().AveragePower);
            Assert.AreEqual(550, sprints.Last().AveragePower);
            List<FitnessData> remainingPoints = container.FitnessData;
            Assert.AreEqual(22, remainingPoints.Count);
        }

        [TestMethod]
        public void InputErrorMinTime()
        {
            FitnessDataContainer container = new FitnessDataContainer(new List<FitnessData>());
            void AuxMethod()
            {
                SprintService service = new SprintService(container, -1, 550, 500);
            }
            Assert.ThrowsException<ArgumentException>(() => AuxMethod());
        }

        [TestMethod]
        public void InputErrorHysteresis()
        {
            FitnessDataContainer container = new FitnessDataContainer(new List<FitnessData>());
            void AuxMethod()
            {
                SprintService service = new SprintService(container, 10, 500, 550);
            }
            Assert.ThrowsException<ArgumentException>(() => AuxMethod());
        }
    }
}