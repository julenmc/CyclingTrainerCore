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
            List<IReader.PointInfo> points = new List<IReader.PointInfo>();
            points.Add(new IReader.PointInfo(0, 0, 0));
            points.Add(new IReader.PointInfo(1, 100, 0));
            points.Add(new IReader.PointInfo(2, 200, 0));
            points.Add(new IReader.PointInfo(2.8, 200, 0));
            points.Add(new IReader.PointInfo(4, 300, 0));
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
            List<IReader.PointInfo> points = new List<IReader.PointInfo>();
            points.Add(new IReader.PointInfo(0, 0, 0));
            points.Add(new IReader.PointInfo(1, 100, 0));
            points.Add(new IReader.PointInfo(2, 200, 0));
            points.Add(new IReader.PointInfo(2.8, 200, 0));
            points.Add(new IReader.PointInfo(4, 300, 0));
            points.Add(new IReader.PointInfo(4.2, 280, 0));
            points.Add(new IReader.PointInfo(5, 350, 0));
            points.Add(new IReader.PointInfo(6, 300, 0));
            points.Add(new IReader.PointInfo(7, 250, 0));
            points.Add(new IReader.PointInfo(8, 200, 0));
            points.Add(new IReader.PointInfo(9, 150, 0));
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
