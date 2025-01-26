using Route;
using Route.Reader;
using Route.Test.Reader.Mock;

namespace Cyclist.Test
{
    [TestClass]
    public class RouteTime
    {
        [TestMethod]
        public void FlatSimple()
        {
            int weight = 70;
            int power = 220;

            List<IReader.SectorInfo> points = new List<IReader.SectorInfo>();
            points.Add(new IReader.SectorInfo(0, 0, 0));
            points.Add(new IReader.SectorInfo(1, 0, 0));
            points.Add(new IReader.SectorInfo(2, 0, 0));
            points.Add(new IReader.SectorInfo(3, 0, 0));
            points.Add(new IReader.SectorInfo(4, 0, 0));
            ReaderMock reader = new ReaderMock(points);
            RouteRepository route = new RouteRepository(reader);

            CyclistRepository cyclist = new CyclistRepository(weight, route);
            cyclist.SetPower(power);
            double distance = 0;
            double time = 0;
            while (distance < route.Lenght)
            {
                distance = cyclist.Advance();
                time += SpeedCalculator.DeltaTime;
            }
        }
    }
}
