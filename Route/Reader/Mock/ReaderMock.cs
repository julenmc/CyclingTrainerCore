using System.Drawing;
using static Route.Reader.IReader;

namespace Route.Test.Reader.Mock
{
    public class ReaderMock : Route.Reader.IReader
    {
        private double _lenght;
        private double _elevation;

        private List<PointInfo> _points;

        public ReaderMock(List<PointInfo> p) 
        { 
            _points = new List<PointInfo>();
            AssignSlopes(p);
        }

        private void AssignSlopes(List<PointInfo> p)
        {
            _points.Add(p.First());
            for (int i = 1; i < p.Count; i++)
            {
                double slope = (p[i].Alt - p[i-1].Alt) / (p[i].Len - p[i-1].Len) / 10;
                PointInfo point = new PointInfo(p[i].Len, p[i].Alt, slope);
                _points.Add(point);
            }
        }

        public bool Read()
        {
            _lenght = _points.Last().Len;
            double _prevElevation = _points.First().Alt;
            foreach (PointInfo point in _points)
            {
                double diff = point.Alt - _prevElevation;
                _prevElevation = point.Alt;
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

        public List<PointInfo> GetAllPoints()
        {
            return _points; 
        }
    }
}
