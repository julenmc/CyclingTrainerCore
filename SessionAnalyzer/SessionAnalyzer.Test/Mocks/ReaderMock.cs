using NLog;
using SessionAnalyzer.Core.Models;
using SessionAnalyzer.Core.Repository;
using static SessionAnalyzer.Core.Models.IReader;

namespace SessionAnalyzer.Test.Mocks
{
    public class ReaderMock : IReader
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private double _lenght;
        private double _elevation;

        private List<SectorInfo> _sectors;

        public ReaderMock(List<SectorInfo> p) 
        { 
            _sectors = new List<SectorInfo>();
            AssignSlopes(p);
        }

        private void AssignSlopes(List<SectorInfo> sectors)
        {
            foreach (SectorInfo sector in sectors)
            {
                double slope = (sector.EndAlt - sector.StartAlt) / (sector.EndPoint - sector.StartPoint) / 10;
                SectorInfo point = new SectorInfo
                {
                    StartPoint = sector.StartPoint,
                    EndPoint = sector.EndPoint,
                    StartAlt = sector.StartAlt,
                    EndAlt = sector.EndAlt,
                    Slope = slope
                };
                _sectors.Add(point);
            }
        }

        public bool Read()
        {
            _lenght = _sectors.Last().EndPoint;
            Log.Info($"Route lenght: {_lenght} km");
            foreach (SectorInfo sector in _sectors)
            {
                double diff = sector.EndAlt - sector.StartAlt;
                if (diff > 0) _elevation += diff;
            }
            return true;
        }

        public string GetName()
        {
            return "Test";
        }

        public double GetLenght() => _lenght;
        public double GetElevation() => _elevation;
        public List<SectorInfo> GetAllSectors() => _sectors;
    }
}
