using CyclingTrainer.Core.Models;
using CyclingTrainer.Core.Constants;
using CyclingTrainer.SessionAnalyzer.Core.Services;
using CyclingTrainer.SessionAnalyzer.Test.Models;
using CyclingTrainer.SessionReader.Core.Models;

namespace CyclingTrainer.SessionAnalyzer.Test
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
            new FitnessSection{ Time = 2400, Power = 200, HearRate = 150, Cadence = 90},
            new FitnessSection{ Time = 60, Power = 300, HearRate = 150, Cadence = 90},
            new FitnessSection{ Time = 60, Power = 100, HearRate = 150, Cadence = 90},
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

        // [TestMethod]
        // public void PowerIntervals()
        // {
        //     AnalyzedData data = DataAnalyzeService.AnalyzeFitnessData(fitnessData);
        //     List<Interval> intervals = new List<Interval>()
        //     {
        //         new Interval
        //         {
        //             StartTime = new DateTime(2025, 07, 14, 12, 00, 00),
        //             EndTime = new DateTime(2025, 07, 14, 12, 20, 00),
        //             AveragePower = 150,
        //             AverageCadence = 85,
        //             AverageHeartRate = 120
        //         },
        //         new Interval
        //         {
        //             StartTime = new DateTime(2025, 07, 14, 12, 20, 00),
        //             EndTime = new DateTime(2025, 07, 14, 12, 40, 00),
        //             AveragePower = 200,
        //             AverageCadence = 90,
        //             AverageHeartRate = 150
        //         },
        //         new Interval
        //         {
        //             StartTime = new DateTime(2025, 07, 14, 12, 40, 00),
        //             EndTime = new DateTime(2025, 07, 14, 13, 00, 00),
        //             AveragePower = 250,
        //             AverageCadence = 95,
        //             AverageHeartRate = 180
        //         }
        //     };
        //     Assert.AreEqual(data.Intervals?.Count, intervals.Count);
        //     for (int i = 0; i < intervals.Count; i++)
        //     {
        //         Assert.AreEqual(data.Intervals?[i].AveragePower, intervals[i].AveragePower);
        //         Assert.AreEqual(data.Intervals?[i].AverageHeartRate, intervals[i].AverageHeartRate);
        //         Assert.AreEqual(data.Intervals?[i].AverageCadence, intervals[i].AverageCadence);
        //     }
        // }

        [TestMethod]
        public void PowerCurve()
        {
            AnalyzedData data = DataAnalyzeService.AnalyzeFitnessData(fitnessData);
            Dictionary<int, int> curve = new Dictionary<int, int>
            {
                { 1, 300 },
                { 60, 300 },
                { 1200, 250 },
                { 2400, 225 }
            };
            Assert.AreEqual(curve[1], data.PowerCurve?[1].Power);
            Assert.AreEqual(curve[60], data.PowerCurve?[60].Power);
            Assert.AreEqual(curve[1200], data.PowerCurve?[1200].Power);
            Assert.AreEqual(curve[2400], data.PowerCurve?[2400].Power);
            // Assert.AreEqual(curve[3600], data.PowerCurve?[3600].Power);
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

        [TestMethod]
        public void PowerCurve()
        {
            AnalyzedData data = DataAnalyzeService.AnalyzeFitnessData(fitnessData);
            Dictionary<int, int> curve = new Dictionary<int, int>
            {
                { 1, 250 },
                { 60, 250 },
                { 1200, 250 }
            };
            Assert.AreEqual(curve[1], data.PowerCurve?[1].Power);
            Assert.AreEqual(curve[60], data.PowerCurve?[60].Power);
            Assert.AreEqual(curve[1200], data.PowerCurve?[1200].Power);
            Assert.IsFalse(data.PowerCurve?.ContainsKey(2400));
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

        [TestMethod]
        public void PowerCurve()
        {
            AnalyzedData data = DataAnalyzeService.AnalyzeFitnessData(fitnessData);
            Dictionary<int, int> curve = new Dictionary<int, int>
            {
                { 1, 250 },
                { 60, 250 },
                { 200, 250 },
            };
            Assert.AreEqual(curve[1], data.PowerCurve?[1].Power);
            Assert.AreEqual(curve[60], data.PowerCurve?[60].Power);
            Assert.IsTrue(curve[200] > data.PowerCurve?[200].Power);
        }
    }
}
