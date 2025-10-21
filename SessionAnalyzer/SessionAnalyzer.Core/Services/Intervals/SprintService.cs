using CyclingTrainer.SessionAnalyzer.Models;
using CyclingTrainer.SessionReader.Core.Models;
using NLog;

namespace CyclingTrainer.SessionAnalyzer.Services.Intervals
{
    internal static class SprintService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static int _minSprintTime;
        private static int _startTrigger;
        private static int _endTrigger;

        internal static void SetConfiguration(int minSprintTime, int startTrigger, int endTrigger)
        {
            if (startTrigger <= endTrigger)
                throw new ArgumentException("Start trigger must be greater than end trigger for hysteresis to work", nameof(startTrigger));

            if (minSprintTime <= 0)
                throw new ArgumentException("Minimum sprint time must be positive", nameof(minSprintTime));

            _minSprintTime = minSprintTime;
            _startTrigger = startTrigger;
            _endTrigger = endTrigger;
        }

        public static void AnalyzeActivity(List<FitnessData> activityPoints)
        {
            Log.Info($"Searching for sprints...");
            if (activityPoints == null || !activityPoints.Any())
                return;

            IntervalRepository.SetFitnessData(activityPoints);
            DetectSprints();
            Log.Info($"Sprint search finished");
        }

        private static void DetectSprints()
        {
            var points = IntervalRepository.GetRemainingFitnessData();
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
                        var nextPower = points[i+1].Stats.Power ?? 0;
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
                    var sprint = new Sprint
                    {
                        StartTime = sprintStartTime,
                        EndTime = sprintEndTime,
                        TimeDiff = (int)sprintDuration,
                        MaxPower = maxPower,
                        AveragePower = (float)powerSum / pointCount
                    };

                    IntervalRepository.AddSprint(sprint);
                    Log.Debug($"New sprint detected! Duration: {sprintDuration} s. AvrPower: {sprint.AveragePower} W");

                    // Como el repositorio elimina los puntos del sprint, necesitamos obtener los puntos restantes
                    points = IntervalRepository.GetRemainingFitnessData();
                    i = 0;
                }
            }
        }
    }
}