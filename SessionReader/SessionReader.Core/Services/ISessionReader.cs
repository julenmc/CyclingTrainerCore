using CyclingTrainer.SessionReader.Core.Models;

namespace CyclingTrainer.SessionReader.Core.Services
{
    public interface ISessionReader
    {
        public enum ReaderResult
        {
            Value,
            End,
            Error
        }

        public bool Read();
        public double GetLenght();
        public double GetElevation();
        public string GetName();
        public List<SectorInfo> GetSmoothedSectors();
        public List<FitnessData> GetFitnessData();
    }
}
