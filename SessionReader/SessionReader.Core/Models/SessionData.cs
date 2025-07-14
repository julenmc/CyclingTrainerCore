namespace SessionReader.Core.Models
{
    public class SessionData
    {
        public string Name { get; set; } = default!;
        public RouteData Route { get; set; } = default!;
        public List<FitnessData> FitnessData { get; set; } = default!;

        internal SessionData() 
        {
            Route = new RouteData();
            FitnessData = new List<FitnessData>();
        }
    }
}