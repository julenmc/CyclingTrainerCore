using CyclingTrainer.Core.Models;

namespace CyclingTrainer.SessionReader.Models
{
    public class SessionContainer
    {
        public Session Session { private set; get; }
        public RouteSections RouteSections { private set; get; }
        public FitnessDataContainer FitnessDataContainer { private set; get; }

        public SessionContainer(Session session, RouteSections routeSections, List<FitnessData> fitnessData)
        {
            Session = session;
            RouteSections = routeSections;
            FitnessDataContainer = new(fitnessData);
        }
    }
}