using CommonModels;
using SessionAnalyzer.Core.Services;
using SessionReader.Core.Models;

namespace SessionAnalyzer.Test
{
    [TestClass]
    public sealed class AnalyzeContinuousSession
    {
        const int FirstSectorTime = 1200;
        const int FirstSectorPower = 150;
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
            DateTime startDate = new DateTime(2025, 07, 14, 12, 00, 00);
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
        public void PowerIntervals()
        {
            AnalyzedData data = DataAnalyzeService.AnalyzeFitnessData(fitnessData);
            List<Interval> intervals = new List<Interval>()
            {
                new Interval
                {
                    StartTime = new DateTime(2025, 07, 14, 12, 00, 00),
                    EndTime = new DateTime(2025, 07, 14, 12, 20, 00),
                    AveragePower = 150,
                    AverageCadence = 85,
                    AverageHeartRate = 120
                },
                new Interval
                {
                    StartTime = new DateTime(2025, 07, 14, 12, 20, 00),
                    EndTime = new DateTime(2025, 07, 14, 12, 40, 00),
                    AveragePower = 200,
                    AverageCadence = 90,
                    AverageHeartRate = 150
                },
                new Interval
                {
                    StartTime = new DateTime(2025, 07, 14, 12, 40, 00),
                    EndTime = new DateTime(2025, 07, 14, 13, 00, 00),
                    AveragePower = 250,
                    AverageCadence = 95,
                    AverageHeartRate = 180
                }
            };
            Assert.AreEqual(data.Intervals?.Count, intervals.Count);
            for (int i = 0; i < intervals.Count; i++)
            {
                Assert.AreEqual(data.Intervals?[i].AveragePower, intervals[i].AveragePower);
                Assert.AreEqual(data.Intervals?[i].AverageHeartRate, intervals[i].AverageHeartRate);
                Assert.AreEqual(data.Intervals?[i].AverageCadence, intervals[i].AverageCadence);
            }
        }

        [TestMethod]
        public void PowerCurve()
        {
            AnalyzedData data = DataAnalyzeService.AnalyzeFitnessData(fitnessData);
            Dictionary<int, int> curve = new Dictionary<int, int>();
            Assert.AreEqual(data.PowerCurve?.Count, curve.Count);
            for (int i = 0; i < curve.Count; i++)
            {
                Assert.AreEqual(data.PowerCurve?[i], curve[i]);
            }
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
            DateTime startDate = new DateTime(2025, 07, 14, 12, 00, 00);
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
            DateTime startDate = new DateTime(2025, 07, 14, 12, 00, 00);
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
