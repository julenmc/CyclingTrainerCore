using CyclingTrainer.Core.Models;
using CyclingTrainer.TrainingDatabase.Core.Models;
using CyclingTrainer.TrainingDatabase.Core.Repository;

namespace CyclingTrainer.TrainingDatabase.App
{
    class Program
    {
        static async Task Main()
        {
            Console.WriteLine("Init test");
            await CyclistsRepository.InitializeRepositoryAsync();
            Dictionary<int, Cyclist> cyclists = CyclistsRepository.GetAll();

            //await AddCyclistAsync();
            //await UpdateCyclistAsync(cyclists[1]);
            await AddSessionAsync(cyclists[1]);
            //await FilterClimbs();
            //await AddClimbAsync();
        }

        static async Task AddCyclistAsync()
        {
            Cyclist cyclist = new Cyclist()
            {
                Name = "Test",
                LastName = "Cyclist",
                BirthDate = new DateTime(1990, 1, 1),
                Details = new CyclistEvolution
                {
                    UpdateDate = DateTime.Now,
                    Height = 180,
                    Weight = 75,
                    Vo2Max = 50.0f,
                    MaxPowerCurve = new Dictionary<int, int>
                    {
                        { 15, 600 },
                        { 30, 500 },
                        { 60, 380 },
                        { 120, 350 },
                        { 300, 300 },
                        { 480, 280 },
                        { 600, 265 },
                        { 1200, 250 }
                    }
                },
            };
            int id = await CyclistsRepository.AddCyclistAsync(cyclist);
            Console.WriteLine($"Cyclist with ID {id} added");
        }

        static async Task UpdateCyclistAsync(Cyclist cyclist)
        {
            CyclistEvolution evolution = new CyclistEvolution()
            {
                UpdateDate = DateTime.Now,
                Vo2Max = 50.0f,
            };
            await CyclistsRepository.UpdateCyclist(cyclist.Id, evolution);
        }

        static async Task AddSessionAsync(Cyclist cyclist)
        {
            DateTime now = DateTime.Now;
            Session session = new Session
            {
                Path = "example_path.fit",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddHours(1),
                Distance = 20000,
                HeightDiff = 500,
                AnalyzedData = new AnalyzedData
                {
                    Calories = 800,
                    AverageHr = 150,
                    AveragePower = 250,
                    PowerCurve = new Dictionary<int, int>
                    {
                        { 15, 600 },
                        { 30, 500 },
                        { 60, 380 },
                        { 120, 350 },
                        { 300, 300 },
                        { 480, 280 },
                        { 600, 265 },
                        { 1200, 250 }
                    },
                    Intervals = new List<Interval>
                    {
                        new Interval
                        {
                            StartTime = now.Add(new TimeSpan(-1,0,0)),
                            EndTime =  now.Add(new TimeSpan(0,-50,0)),
                            TotalDistance = 5000,
                            AverageHeartRate = 145,
                            AveragePower = 260,
                            AverageCadence = 100
                        },
                        new Interval
                        {
                            StartTime = now.Add(new TimeSpan(0,-50,0)),
                            EndTime =  now.Add(new TimeSpan(0,-40,0)),
                            TotalDistance = 2000,
                            AverageHeartRate = 135,
                            AveragePower = 200,
                            AverageCadence = 90
                        },
                        new Interval
                        {
                            StartTime = now.Add(new TimeSpan(0,-40,0)),
                            EndTime =  now.Add(new TimeSpan(0,-25,0)),
                            TotalDistance = 5000,
                            AverageHeartRate = 155,
                            AveragePower = 270,
                            AverageCadence = 100
                        },
                        new Interval
                        {
                            StartTime = now.Add(new TimeSpan(0,-25,0)),
                            EndTime =  now.Add(new TimeSpan(0,-5,0)),
                            TotalDistance = 3000,
                            AverageHeartRate = 125,
                            AveragePower = 190,
                            AverageCadence = 80
                        },
                    },
                    Climbs = new Dictionary<Climb, Interval>
                    {
                        {
                            await ClimbRepository.GetClimbAsync(3),
                            new Interval
                            {
                                StartTime = now.Add(new TimeSpan(-1,0,0)),
                                EndTime =  now.Add(new TimeSpan(0,-40,0)),
                                TotalDistance = 6000,
                                AverageHeartRate = 145,
                                AveragePower = 260,
                                AverageCadence = 100
                            }
                        }
                    }
                },
                IsIndoor = false,
            };
            SessionsRepository sessionsRepository = await CyclistsRepository.GetCyclistSessionsAsync(cyclist.Id);
            await sessionsRepository.AddSessionAsync(session);
        }

        static async Task FilterClimbs()
        {
            double longitude = 1.0;
            double latitude = 40.0;
            double size = 200.0; // Size in km
            List<Climb> climbs = await ClimbRepository.GetClimbsAsync(longitude, latitude, size);
            Console.WriteLine($"Found {climbs.Count} climbs near coordinates ({longitude}, {latitude}) in {size} kms");
        }

        static async Task AddClimbAsync()
        {
            Climb climb = new Climb
            {
                Name = "Test Climb",
                Path = "test_climb_path.gpx",
                LongitudeInit = 2.0,
                LatitudeInit = 41.0,
                LongitudeEnd = 2.1,
                LatitudeEnd = 41.1,
                AltitudeInit = 450,
                AltitudeEnd = 650,
                HeightDiff = 200,
                Distance = 5000,
                AverageSlope = 5.5f,
                MaxSlope = 5.0f
            };
            int climbId = await ClimbRepository.AddClimb(climb);
            Console.WriteLine($"Climb with ID {climbId} added: {climb.Name}");
        }
    }
}