using CyclingTrainer.SessionAnalyzer.Core.Models;
using CyclingTrainer.SessionReader.Core.Models;

namespace CyclingTrainer.SessionAnalyzer.Core.Services.Intervals
{
    internal static class SprintService
    {
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
            if (activityPoints == null || !activityPoints.Any())
                return;

            IntervalRepository.SetFitnessData(activityPoints);
            DetectSprints();
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
                        // Check if the next point continues as a sprint
                        var nextPower = points[i+1].Stats.Power ?? 0;
                        if (nextPower < _endTrigger)
                        {
                            break;
                        }
                    }

                    maxPower = Math.Max(maxPower, currentPower);
                    powerSum += currentPower;
                    pointCount++;
                    i++;
                }

                var sprintEndTime = points[Math.Max(0, i - 1)].Timestamp.GetDateTime();
                var sprintDuration = (sprintEndTime - sprintStartTime).TotalSeconds;

                // Solo guardar si supera el tiempo mínimo
                if (sprintDuration >= _minSprintTime)
                {
                    var sprint = new Sprint
                    {
                        StartTime = sprintStartTime,
                        EndTime = sprintEndTime,
                        MaxPower = maxPower,
                        AveragePower = (float)powerSum / pointCount
                    };

                    IntervalRepository.AddSprint(sprint);

                    // Como el repositorio elimina los puntos del sprint, necesitamos obtener los puntos restantes
                    points = IntervalRepository.GetRemainingFitnessData();
                    i = 0;
                }
            }
        }
    }
}