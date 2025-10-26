using CyclingTrainer.SessionAnalyzer.Models;
using CyclingTrainer.SessionReader.Models;

namespace CyclingTrainer.SessionAnalyzer.Services.Intervals
{
    internal static class IntervalRepository
    {
        private readonly static List<FitnessData> _fitnessData = new();
        private readonly static List<Interval> _sprints = new();

        internal static void SetFitnessData(List<FitnessData> fitnessData)
        {
            _fitnessData.Clear();
            _sprints.Clear();
            _fitnessData.AddRange(fitnessData);
        }

        internal static void AddSprint(Interval sprint)
        {
            _sprints.Add(sprint);
            
            // Remove sprint data points from fitness data
            _fitnessData.RemoveAll(data => 
                data.Timestamp.GetDateTime() >= sprint.StartTime && 
                data.Timestamp.GetDateTime() < sprint.EndTime);
        }

        internal static List<FitnessData> GetRemainingFitnessData()
        {
            return _fitnessData.ToList();
        }

        internal static List<Interval> GetSprints()
        {
            return _sprints.ToList();
        }

        internal static bool IsTheGapASprint(DateTime time)
        {
            return _sprints.Find(x => x.EndTime == time) != null;
        }

        internal static void Clear()
        {
            _fitnessData.Clear();
            _sprints.Clear();
        }
    }
}