using Route;
using Route.Reader;

namespace Cyclist
{
    public class CyclistRepository
    {
        public double Weight { private set; get; }
        public double Speed { private set; get; }
        public double CurrentPower { private set; get; }

        private RouteRepository _routeRepo;
        private List<IReader.PointInfo> _points;
        private int _index;
        private double _dist;

        public CyclistRepository (double weight, RouteRepository route)
        {
            Weight = weight;
            Speed = 0;
            CurrentPower = 0;

            _routeRepo = route;
            _points = _routeRepo.GetAllPoints();
            _index = 0;
            _dist = 0;
        }

        public double Advance()
        {
            double slope = _points[_index].Slope;
            var r = SpeedCalculator.CalculateDistance(this, slope, 0, CurrentPower);
            _dist += r.Item1 / 1000;

            // Update index with distance
            for (int i = _index; i < _points.Count; i++)
            {
                if (_dist < _points[i].Len) break;
                _index++;
            }
            Speed = r.Item2;

            return _dist;
        }

        public void SetPower(double power)
        {
            CurrentPower = power;
        }
    }
}
