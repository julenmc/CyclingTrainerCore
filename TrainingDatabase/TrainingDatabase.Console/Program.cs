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
        }
    }
}