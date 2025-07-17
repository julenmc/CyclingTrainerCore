using CommonModels;
using SessionAnalyzer.Core.Services;
using SessionReader.Core.Models;

namespace SessionAnalyzer.Test
{
    [TestClass]
    public sealed class AnalyzeContinuousSession
    {
        List<FitnessData> fitnessData = default!;
        List<FitnessSection> fitnessTestSections = new List<FitnessSection>
        {
            new FitnessSection{ Time = 1200, Power = 150, HearRate = 120, Cadence = 85},
            new FitnessSection{ Time = 1200, Power = 200, HearRate = 150, Cadence = 90},
            new FitnessSection{ Time = 1200, Power = 250, HearRate = 180, Cadence = 95},
        };

        [TestInitialize]
        public void SetUp()
        {
            fitnessData = new List<FitnessData>();
            double totalTime = 0;
            foreach (FitnessSection section in fitnessTestSections)
            {
                totalTime = section.Time;
            }

            DateTime startDate = DateTime.Now.AddSeconds(-totalTime);
            foreach (FitnessSection section in fitnessTestSections)
            {
                for (int i = 0; i < section.Time; i++)
                {
                    fitnessData.Add(new FitnessData
                    {
                        Timestamp = new Dynastream.Fit.DateTime(startDate),
                        Stats = new PointStats
                        {
                            Power = section.Power,
                            HeartRate = section.HearRate,
                            Cadence = section.Cadence
                        }
                    });
                    startDate = startDate.AddSeconds(1);
                }
            }
        }

        [TestMethod]
        public void Averages()
        {
            AnalyzedData data = DataAnalyzeService.AnalyzeFitnessData(fitnessData);
            Assert.AreEqual(200, data.AveragePower);
            Assert.AreEqual(150, data.AverageHr);
            Assert.AreEqual(90, data.AverageCadence);
        }

        [TestMethod]
        public void PowerInervals()
        {
            
        }
    }

    [TestClass]
    public sealed class AnalyzeDiscontinuousSession
    {
        List<FitnessData> fitnessData = default!;
        List<FitnessSection> fitnessTestSections = new List<FitnessSection>
        {
            new FitnessSection{ Time = 1200, Power = 150, HearRate = 120, Cadence = 85},
            new FitnessSection{ Time = 1200, Power = 200, HearRate = 150, Cadence = 90},
            new FitnessSection{ Time = 1200, Power = 250, HearRate = 180, Cadence = 95},
        };

        [TestInitialize]
        public void SetUp()
        {
            fitnessData = new List<FitnessData>();
            double totalTime = 0;
            foreach (FitnessSection section in fitnessTestSections)
            {
                totalTime = section.Time;
            }

            DateTime startDate = DateTime.Now.AddSeconds(-totalTime-60);
            foreach (FitnessSection section in fitnessTestSections)
            {
                for (int i = 0; i < section.Time; i++)
                {
                    fitnessData.Add(new FitnessData
                    {
                        Timestamp = new Dynastream.Fit.DateTime(startDate),
                        Stats = new PointStats
                        {
                            Power = section.Power,
                            HeartRate = section.HearRate,
                            Cadence = section.Cadence
                        }
                    });
                    startDate = startDate.AddSeconds(1);
                }
                fitnessData.Add(new FitnessData
                {
                    Timestamp = new Dynastream.Fit.DateTime(startDate),
                    Stats = new PointStats
                    {
                        Power = 0,
                        HeartRate = 150,
                        Cadence = 0
                    }
                });
                startDate = startDate.AddSeconds(10);
            }
        }

        [TestMethod]
        public void Averages()
        {
            AnalyzedData data = DataAnalyzeService.AnalyzeFitnessData(fitnessData);
            Assert.AreEqual(200, data.AveragePower);
            Assert.AreEqual(150, data.AverageHr);
            Assert.AreEqual(90, data.AverageCadence);
        }
    }

    [TestClass]
    public sealed class AnalyzeSessionSmallStops
    {
        List<FitnessData> fitnessData = default!;
        List<FitnessSection> fitnessTestSections = new List<FitnessSection>
        {
            new FitnessSection{ Time = 100, Power = 150, HearRate = 120, Cadence = 85},
            new FitnessSection{ Time = 100, Power = 200, HearRate = 150, Cadence = 90},
            new FitnessSection{ Time = 100, Power = 250, HearRate = 180, Cadence = 95},
        };

        [TestInitialize]
        public void SetUp()
        {
            fitnessData = new List<FitnessData>();
            double totalTime = 0;
            foreach (FitnessSection section in fitnessTestSections)
            {
                totalTime = section.Time;
            }

            DateTime startDate = DateTime.Now.AddSeconds(-totalTime-60);
            foreach (FitnessSection section in fitnessTestSections)
            {
                for (int i = 0; i < section.Time; i++)
                {
                    fitnessData.Add(new FitnessData
                    {
                        Timestamp = new Dynastream.Fit.DateTime(startDate),
                        Stats = new PointStats
                        {
                            Power = section.Power,
                            HeartRate = section.HearRate,
                            Cadence = section.Cadence
                        }
                    });
                    startDate = startDate.AddSeconds(1);
                }
                fitnessData.Add(new FitnessData
                {
                    Timestamp = new Dynastream.Fit.DateTime(startDate),
                    Stats = new PointStats
                    {
                        Power = 0,
                        HeartRate = 150,
                        Cadence = 0
                    }
                });
                startDate = startDate.AddSeconds(2);
            }
        }

        [TestMethod]
        public void Averages()
        {
            AnalyzedData data = DataAnalyzeService.AnalyzeFitnessData(fitnessData);
            Assert.IsTrue(data.AveragePower < 200);
        }
    }

    internal class FitnessSection
    {
        internal int Time;
        internal int Power;
        internal int HearRate;
        internal int Cadence;
    }
}
