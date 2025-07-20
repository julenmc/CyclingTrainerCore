namespace CyclingTrainer.Core.Constants
{
    public static class PowerCurveConstants
    {
        public static readonly List<int> StandardTimePoints;

        private static readonly List<PowerCurveSectorInfo> SectorsInfo = new List<PowerCurveSectorInfo>
        {
            new PowerCurveSectorInfo{ StartTime = 1, EndTime = 15, Interval = 1},
            new PowerCurveSectorInfo{ StartTime = 16, EndTime = 60, Interval = 2},
            new PowerCurveSectorInfo{ StartTime = 65, EndTime = 120, Interval = 5},
            new PowerCurveSectorInfo{ StartTime = 130, EndTime = 300, Interval = 10},
            new PowerCurveSectorInfo{ StartTime = 330, EndTime = 600, Interval = 30},
            new PowerCurveSectorInfo{ StartTime = 660, EndTime = 1200, Interval = 60},
            new PowerCurveSectorInfo{ StartTime = 1320, EndTime = 3600, Interval = 120},
            new PowerCurveSectorInfo{ StartTime = 3900, EndTime = 60000, Interval = 300}
        };

        static PowerCurveConstants()
        {
            StandardTimePoints = new List<int>();
            foreach (PowerCurveSectorInfo info in SectorsInfo)
            {
                for (int i = info.StartTime; i <= info.EndTime; i += info.Interval)
                {
                    StandardTimePoints.Add(i);
                }
            }
        }

        internal class PowerCurveSectorInfo
        {
            internal int StartTime { get; set; }
            internal int EndTime { get; set; }
            internal int Interval { get; set; }
        }
    }
}