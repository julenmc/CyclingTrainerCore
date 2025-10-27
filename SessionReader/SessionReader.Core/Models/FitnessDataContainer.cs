namespace CyclingTrainer.SessionReader.Models
{
    public class FitnessDataContainer
    {
        public List<FitnessData> FitnessData { private set; get; }

        public FitnessDataContainer(List<FitnessData> fitnessData)
        {
            FitnessData = new List<FitnessData>();
            FitnessData.AddRange(fitnessData);
        }
    }
}