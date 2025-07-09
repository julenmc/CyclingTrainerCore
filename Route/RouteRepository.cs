using NLog;
using Route.Reader;
using Route.Repository;

namespace Route
{
    public class RouteRepository
    {
        private readonly Logger Log = LogManager.GetCurrentClassLogger();
        public double Lenght { get; private set; }
        public double Elevation { get; private set; }
        public List<Mountain> Mountains { get; private set; }
        public string Name { get; private set; }

        private IReader _reader;

        public RouteRepository(IReader reader)
        {
            _reader = reader;
            _reader.Read();
            Name = reader.GetName();

            Lenght = Math.Round(_reader.GetLenght(), 2);
            Elevation = Math.Round(_reader.GetElevation(), 0);

            Mountains = Mountain.GetMountains(_reader.GetAllSectors());

            Log.Info("Analysis finished");
        }

        public List<SectorInfo> GetAllPoints()
        {
            return _reader.GetAllSectors();
        }
    }
}
