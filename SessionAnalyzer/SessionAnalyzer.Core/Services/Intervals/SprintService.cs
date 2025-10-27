using CyclingTrainer.SessionAnalyzer.Models;
using CyclingTrainer.SessionReader.Models;
using NLog;

namespace CyclingTrainer.SessionAnalyzer.Services.Intervals
{
    internal class SprintService
    {
        private readonly Logger Log = LogManager.GetCurrentClassLogger();
        private int _minSprintTime;
        private int _startTrigger;
        private int _endTrigger;
        private FitnessDataContainer _container;

        internal SprintService(FitnessDataContainer container, int minSprintTime, int startTrigger, int endTrigger)
        {
            _container = container;
            if (startTrigger <= endTrigger)
                throw new ArgumentException("Start trigger must be greater than end trigger for hysteresis to work", nameof(startTrigger));

            if (minSprintTime <= 0)
                throw new ArgumentException("Minimum sprint time must be positive", nameof(minSprintTime));

            _minSprintTime = minSprintTime;
            _startTrigger = startTrigger;
            _endTrigger = endTrigger;
        }

        internal List<Interval> SearchSprints()
        {
            List<Interval> sprints = new List<Interval>();
            var points = _container.FitnessData;
            int i = 0;

            while (i < points.Count)
            {
                // Buscar el inicio del sprint (punto que supera el trigger de inicio)
                while (i < points.Count && (points[i].Stats.Power ?? 0) < _startTrigger)
                    i++;

                if (i >= points.Count)
                    break;

                Log.Debug($"Possible sprint detected at index {i}");
                var sprintStartIndex = i;
                var sprintStartTime = points[i].Timestamp.GetDateTime();
                var maxPower = points[i].Stats.Power ?? 0;
                var powerSum = maxPower;
                var pointCount = 1;

                // Buscar el final del sprint (punto que cae por debajo del trigger de fin)
                i++;
                var lowTriggerStartTime = DateTime.MinValue;

                while (i < points.Count)
                {
                    var currentPower = points[i].Stats.Power ?? 0;

                    if (currentPower < _endTrigger)
                    {
                        Log.Debug($"Sprint might end at index {i}");
                        // Check if the next point continues as a sprint
                        var nextPower = points[i + 1].Stats.Power ?? 0;
                        if (nextPower < _endTrigger)
                        {
                            Log.Debug($"Sprint ends at index {i}");
                            break;
                        }
                    }

                    maxPower = Math.Max(maxPower, currentPower);
                    powerSum += currentPower;
                    pointCount++;
                    i++;
                }

                var sprintEndTime = points[Math.Max(0, i)].Timestamp.GetDateTime();
                var sprintDuration = (sprintEndTime - sprintStartTime).TotalSeconds;

                // Solo guardar si supera el tiempo mínimo
                Log.Debug($"Sprint duration = {sprintDuration} secs");
                if (sprintDuration >= _minSprintTime)
                {
                    var sprint = new Interval
                    {
                        StartTime = sprintStartTime,
                        EndTime = sprintEndTime,
                        TimeDiff = (int)sprintDuration,
                        MaxPower = maxPower,
                        AveragePower = (float)powerSum / pointCount
                    };

                    sprints.Add(sprint);
                    // Remove sprint data points from fitness data
                    _container.FitnessData.RemoveAll(data => 
                        data.Timestamp.GetDateTime() >= sprint.StartTime && 
                        data.Timestamp.GetDateTime() < sprint.EndTime);
                    Log.Debug($"New sprint detected! Duration: {sprintDuration} s. AvrPower: {sprint.AveragePower} W");

                    // Como el repositorio elimina los puntos del sprint, necesitamos obtener los puntos restantes
                    points = _container.FitnessData;
                    i = 0;
                }
            }
            return sprints;
        }
    }
}