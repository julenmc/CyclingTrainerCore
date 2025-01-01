using static Route.Reader.IReader;

namespace Route.Reader.Mock
{
    public class ReaderMock : IReader
    {
        private double _lenght;
        private double _elevation;

        private List<PointInfo> _points;

        public ReaderMock(List<PointInfo> p) 
        { 
            _points = p;
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
