using Route.Test.Reader.Mock;
using Route.Reader;

namespace Route.Test.Route.IrregularMount
{
    [TestClass]
    public class UpFinish
    {
        RouteRepository route;

        [TestInitialize]
        public void SetUp()
        {
            List<SectorInfo> points = new List<SectorInfo>();
            points.Add(new SectorInfo(0, 1, 0, 100, 0));
            points.Add(new SectorInfo(1, 2, 100, 200, 0));
            points.Add(new SectorInfo(2, 2.8, 200, 200, 0));
            points.Add(new SectorInfo(2.8, 4, 200, 300, 0));
            ReaderMock reader = new ReaderMock(points);

            route = new RouteRepository(reader);
        }

        [TestMethod]
        public void Basic()
        {
            Assert.AreEqual(4, route.Lenght);
            Assert.AreEqual(300, route.Elevation);
            Assert.AreEqual(1, route.Mountains.Count);
        }

        [TestMethod]
        public void MountInfo()
        {
            Assert.AreEqual(1, route.Mountains[0].Id);
            Assert.AreEqual(4000, route.Mountains[0].Lenght);
            Assert.AreEqual(0, route.Mountains[0].InitKm);
            Assert.AreEqual(300, route.Mountains[0].Elevation);
            Assert.AreEqual(0, route.Mountains[0].InitAltitude);
            Assert.AreEqual(300, route.Mountains[0].MaxAltitude);
            Assert.AreEqual(7.5, route.Mountains[0].Slope);
            Assert.AreEqual(10, route.Mountains[0].MaxSlope);
        }
    }

    [TestClass]
    public class DoubleWarning
    {
        RouteRepository route;

        [TestInitialize]
        public void SetUp()
        {
            List<SectorInfo> points = new List<SectorInfo>();
            points.Add(new SectorInfo(0, 1, 0, 100, 0));
            points.Add(new SectorInfo(1, 2, 100, 200,  0));
            points.Add(new SectorInfo(2, 2.8, 200, 200, 0));
            points.Add(new SectorInfo(2.8, 4, 200, 300, 0));
            points.Add(new SectorInfo(4, 4.2, 300, 280, 0));
            points.Add(new SectorInfo(4.2, 5, 280, 350, 0));
            points.Add(new SectorInfo(5, 6, 350, 300, 0));
            points.Add(new SectorInfo(6, 7, 300, 250, 0));
            points.Add(new SectorInfo(7, 8, 250, 200, 0));
            points.Add(new SectorInfo(8, 9, 200, 150, 0));
            ReaderMock reader = new ReaderMock(points);

            route = new RouteRepository(reader);
        }

        [TestMethod]
        public void Basic()
        {
            Assert.AreEqual(9, route.Lenght);
            Assert.AreEqual(370, route.Elevation);
            Assert.AreEqual(1, route.Mountains.Count);
        }

        [TestMethod]
        public void MountInfo()
        {
            Assert.AreEqual(1, route.Mountains[0].Id);
            Assert.AreEqual(5000, route.Mountains[0].Lenght);
            Assert.AreEqual(0, route.Mountains[0].InitKm);
            Assert.AreEqual(370, route.Mountains[0].Elevation);
            Assert.AreEqual(0, route.Mountains[0].InitAltitude);
            Assert.AreEqual(350, route.Mountains[0].MaxAltitude);
        }
    }
}
