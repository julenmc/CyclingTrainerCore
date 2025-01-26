using Route.Reader;
using Route.Test.Reader.Mock;

namespace Route.Test.Route.RegularMount
{
    [TestClass]
    public class Flat
    {
        [TestMethod]
        public void Basic()
        {
            List<IReader.SectorInfo> points = new List<IReader.SectorInfo>();
            points.Add(new IReader.SectorInfo(0, 1, 0, 0, 0));
            points.Add(new IReader.SectorInfo(1, 2, 0, 0, 0));
            points.Add(new IReader.SectorInfo(2, 3, 0, 0, 0));
            points.Add(new IReader.SectorInfo(3, 4, 0, 0, 0));
            ReaderMock reader = new ReaderMock(points);

            RouteRepository route = new RouteRepository(reader);
            Assert.AreEqual(4, route.Lenght);
            Assert.AreEqual(0, route.Elevation);
            Assert.AreEqual(0, route.Mountains.Count);
        }
    }

    [TestClass]
    public class UpDown
    {
        RouteRepository route;

        [TestInitialize]
        public void SetUp()
        {
            List<IReader.SectorInfo> points = new List<IReader.SectorInfo>();
            points.Add(new IReader.SectorInfo(0, 1, 0, 100, 0));
            points.Add(new IReader.SectorInfo(1, 2, 100, 200, 0));
            points.Add(new IReader.SectorInfo(2, 3, 200, 100, 0));
            points.Add(new IReader.SectorInfo(3, 4, 100, 100, 0));
            points.Add(new IReader.SectorInfo(4, 4.01, 100, 100, 0));
            ReaderMock reader = new ReaderMock(points);

            route = new RouteRepository(reader);
        }

        [TestMethod]
        public void Basic()
        {
            Assert.AreEqual(4.01, route.Lenght);
            Assert.AreEqual(200, route.Elevation);
            Assert.AreEqual(1, route.Mountains.Count);
        }

        [TestMethod]
        public void MountInfo()
        {
            Assert.AreEqual(1, route.Mountains[0].Id);
            Assert.AreEqual(2000, route.Mountains[0].Lenght);
            Assert.AreEqual(0, route.Mountains[0].InitKm);
            Assert.AreEqual(200, route.Mountains[0].Elevation);
            Assert.AreEqual(0, route.Mountains[0].InitAltitude);
            Assert.AreEqual(200, route.Mountains[0].MaxAltitude);
            Assert.AreEqual(10, route.Mountains[0].Slope);
            Assert.AreEqual(10, route.Mountains[0].MaxSlope);
        }
    }

    [TestClass]
    public class UpFlat
    {
        RouteRepository route;

        [TestInitialize]
        public void SetUp()
        {
            List<IReader.SectorInfo> points = new List<IReader.SectorInfo>();
            points.Add(new IReader.SectorInfo(0, 1, 0, 100, 0));
            points.Add(new IReader.SectorInfo(1, 2, 100, 200, 0));
            points.Add(new IReader.SectorInfo(2, 3, 200, 200, 0));
            points.Add(new IReader.SectorInfo(3, 4, 200, 200, 0));
            points.Add(new IReader.SectorInfo(4, 4.01, 200, 200, 0));
            ReaderMock reader = new ReaderMock(points);

            route = new RouteRepository(reader);
        }

        [TestMethod]
        public void Basic()
        {
            Assert.AreEqual(4.01, route.Lenght);
            Assert.AreEqual(200, route.Elevation);
            Assert.AreEqual(1, route.Mountains.Count);
        }

        [TestMethod]
        public void MountInfo()
        {
            Assert.AreEqual(1, route.Mountains[0].Id);
            Assert.AreEqual(2000, route.Mountains[0].Lenght);
            Assert.AreEqual(0, route.Mountains[0].InitKm);
            Assert.AreEqual(200, route.Mountains[0].Elevation);
            Assert.AreEqual(0, route.Mountains[0].InitAltitude);
            Assert.AreEqual(200, route.Mountains[0].MaxAltitude);
            Assert.AreEqual(10, route.Mountains[0].Slope);
            Assert.AreEqual(10, route.Mountains[0].MaxSlope);
        }
    }

    [TestClass]
    public class UpFinish
    {
        RouteRepository route;

        [TestInitialize]
        public void SetUp()
        {
            List<IReader.SectorInfo> points = new List<IReader.SectorInfo>();
            points.Add(new IReader.SectorInfo(0, 1, 0, 100, 0));
            points.Add(new IReader.SectorInfo(1, 2, 100, 200, 0));
            points.Add(new IReader.SectorInfo(2, 3, 200, 300, 0));
            points.Add(new IReader.SectorInfo(3, 4, 300, 400, 0));
            points.Add(new IReader.SectorInfo(4, 4.01, 400, 401, 0));
            ReaderMock reader = new ReaderMock(points);

            route = new RouteRepository(reader);
        }

        [TestMethod]
        public void Basic()
        {
            Assert.AreEqual(4.01, route.Lenght);
            Assert.AreEqual(401, route.Elevation);
            Assert.AreEqual(1, route.Mountains.Count);
        }

        [TestMethod]
        public void MountInfo()
        {
            Assert.AreEqual(1, route.Mountains[0].Id);
            Assert.AreEqual(4010, route.Mountains[0].Lenght);
            Assert.AreEqual(0, route.Mountains[0].InitKm);
            Assert.AreEqual(401, route.Mountains[0].Elevation);
            Assert.AreEqual(0, route.Mountains[0].InitAltitude);
            Assert.AreEqual(401, route.Mountains[0].MaxAltitude);
            Assert.AreEqual(10, route.Mountains[0].Slope);
            Assert.AreEqual(10, route.Mountains[0].MaxSlope, 0.01);
        }
    }

    [TestClass]
    public class Multiple
    {
        RouteRepository route;

        [TestInitialize]
        public void SetUp()
        {
            List<IReader.SectorInfo> points = new List<IReader.SectorInfo>();
            points.Add(new IReader.SectorInfo(0, 1, 0, 100, 0));
            points.Add(new IReader.SectorInfo(1, 2, 100, 200, 0));
            points.Add(new IReader.SectorInfo(2, 3, 200, 100, 0));
            points.Add(new IReader.SectorInfo(3, 5, 100, 100, 0));
            points.Add(new IReader.SectorInfo(5, 6, 100, 200, 0));
            ReaderMock reader = new ReaderMock(points);

            route = new RouteRepository(reader);
        }

        [TestMethod]
        public void Basic()
        {
            Assert.AreEqual(6, route.Lenght);
            Assert.AreEqual(300, route.Elevation);
            Assert.AreEqual(2, route.Mountains.Count);
        }

        [TestMethod]
        public void MountInfo()
        {
            Assert.AreEqual(1, route.Mountains[0].Id);
            Assert.AreEqual(2000, route.Mountains[0].Lenght);
            Assert.AreEqual(0, route.Mountains[0].InitKm);
            Assert.AreEqual(200, route.Mountains[0].Elevation);
            Assert.AreEqual(0, route.Mountains[0].InitAltitude);
            Assert.AreEqual(200, route.Mountains[0].MaxAltitude);
            Assert.AreEqual(10, route.Mountains[0].Slope);
            Assert.AreEqual(10, route.Mountains[0].MaxSlope);

            Assert.AreEqual(2, route.Mountains[1].Id);
            Assert.AreEqual(1000, route.Mountains[1].Lenght);
            Assert.AreEqual(5, route.Mountains[1].InitKm);
            Assert.AreEqual(100, route.Mountains[1].Elevation);
            Assert.AreEqual(100, route.Mountains[1].InitAltitude);
            Assert.AreEqual(200, route.Mountains[1].MaxAltitude);
            Assert.AreEqual(10, route.Mountains[1].Slope);
            Assert.AreEqual(10, route.Mountains[1].MaxSlope);
        }
    }
}
