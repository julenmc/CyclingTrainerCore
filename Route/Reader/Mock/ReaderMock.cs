using System.Drawing;
using static Route.Reader.IReader;

namespace Route.Test.Reader.Mock
{
    public class ReaderMock : Route.Reader.IReader
    {
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
                SectorInfo point = new SectorInfo(sector.StartPoint, sector.EndPoint, sector.StartAlt, sector.EndAlt, slope);
                _sectors.Add(point);
            }
        }

        public bool Read()
        {
            _lenght = _sectors.Last().EndPoint;
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

        public double GetLenght()
        {
            return _lenght;
        }

        public double GetElevation()
        {
            return _elevation;
        }

        public List<SectorInfo> GetAllSectors()
        {
            return _sectors; 
        }
    }
}
