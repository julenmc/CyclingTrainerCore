using TrainingDatabase.Core.Models;
using TrainingDatabase.Core.Repository;

namespace TrainingDatabase.App
{
    class Program
    {
        static async Task Main()
        {
            Console.WriteLine("Init test");
            Dictionary<int, Cyclist> cyclists = CyclistRepository.GetAll();
            Console.WriteLine($"Name: {cyclists.First().Value.Name}");

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
            int id = CyclistRepository.AddCyclist(cyclist);
            Console.WriteLine($"Cyclist with ID {id} added");

            CyclistEvolution evolution = new CyclistEvolution()
            {
                UpdateDate = DateTime.Now,
                Height = cyclists[1].Details.Height,
                Weight = cyclists[1].Details.Weight,
                Vo2Max = 50.0f,
                MaxPowerCurve = cyclists[1].Details.MaxPowerCurve,
            };
            CyclistRepository.UpdateCyclist(1, evolution);
        }
    }
}