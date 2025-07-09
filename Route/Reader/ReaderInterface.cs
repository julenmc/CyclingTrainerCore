using Route.Repository;

namespace Route.Reader
{
    public interface IReader
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
        public List<SectorInfo> GetAllSectors();
    }
}
