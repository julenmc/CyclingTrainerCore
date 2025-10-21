using CyclingTrainer.SessionAnalyzer.Models;
using CyclingTrainer.SessionReader.Models;

namespace CyclingTrainer.SessionAnalyzer.Services.Intervals
{
    internal static class IntervalRepository
    {
        private readonly static List<FitnessData> _fitnessData = new();
        private readonly static List<Sprint> _sprints = new();

        public static void SetFitnessData(List<FitnessData> fitnessData)
        {
            _fitnessData.Clear();
            _sprints.Clear();
            _fitnessData.AddRange(fitnessData);
        }

        public static void AddSprint(Sprint sprint)
        {
            _sprints.Add(sprint);
            
            // Remove sprint data points from fitness data
            _fitnessData.RemoveAll(data => 
                data.Timestamp.GetDateTime() >= sprint.StartTime && 
                data.Timestamp.GetDateTime() < sprint.EndTime);
        }

        public static List<FitnessData> GetRemainingFitnessData()
        {
            return _fitnessData.ToList();
        }

        public static List<Sprint> GetSprints()
        {
            return _sprints.ToList();
        }

        public static void Clear()
        {
            _fitnessData.Clear();
            _sprints.Clear();
        }
    }
}