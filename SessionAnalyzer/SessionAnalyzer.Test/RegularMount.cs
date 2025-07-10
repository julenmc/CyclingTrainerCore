using SessionAnalyzer.Core.Models;
using SessionAnalyzer.Core.Repository;
using SessionAnalyzer.Test.Mocks;

namespace SessionAnalyzer.Test.Regular
{
    [TestClass]
    public class Flat
    {
        Route route = default!;

        [TestMethod]
        public void Basic()
        {
            List<SectorInfo> points = new List<SectorInfo>();
            points.Add(new SectorInfo(0, 1, 0, 0, 0));
            points.Add(new SectorInfo(1, 2, 0, 0, 0));
            points.Add(new SectorInfo(2, 3, 0, 0, 0));
            points.Add(new SectorInfo(3, 4, 0, 0, 0));
            ReaderMock reader = new ReaderMock(points);

            route = RouteRepository.AnalyzeRoute(reader);
            Assert.AreEqual(4, route.Lenght);
            Assert.AreEqual(0, route.Elevation);
            Assert.AreEqual(0, route.Climbs.Count);
        }
    }

    [TestClass]
    public class UpDown
    {
        Route route = default!;

        [TestInitialize]
        public void SetUp()
        {
            List<SectorInfo> points = new List<SectorInfo>();
            points.Add(new SectorInfo(0, 1, 0, 100, 0));
            points.Add(new SectorInfo(1, 2, 100, 200, 0));
            points.Add(new SectorInfo(2, 3, 200, 100, 0));
            points.Add(new SectorInfo(3, 4, 100, 100, 0));
            points.Add(new SectorInfo(4, 4.01, 100, 100, 0));
            ReaderMock reader = new ReaderMock(points);

            route = RouteRepository.AnalyzeRoute(reader);
        }

        [TestMethod]
        public void Basic()
        {
            Assert.AreEqual(4.01, route.Lenght);
            Assert.AreEqual(200, route.Elevation);
            Assert.AreEqual(1, route.Climbs.Count);
            Assert.AreEqual(1, route.Climbs[0].Id);
            Assert.AreEqual(2000, route.Climbs[0].Lenght);
            Assert.AreEqual(0, route.Climbs[0].InitKm);
            Assert.AreEqual(200, route.Climbs[0].Elevation);
            Assert.AreEqual(0, route.Climbs[0].InitAltitude);
            Assert.AreEqual(200, route.Climbs[0].MaxAltitude);
            Assert.AreEqual(10, route.Climbs[0].Slope);
            Assert.AreEqual(10, route.Climbs[0].MaxSlope);
        }
    }

    [TestClass]
    public class UpFlat
    {
        Route route = default!;

        [TestInitialize]
        public void SetUp()
        {
            List<SectorInfo> points = new List<SectorInfo>();
            points.Add(new SectorInfo(0, 1, 0, 100, 0));
            points.Add(new SectorInfo(1, 2, 100, 200, 0));
            points.Add(new SectorInfo(2, 3, 200, 200, 0));
            points.Add(new SectorInfo(3, 4, 200, 200, 0));
            points.Add(new SectorInfo(4, 4.01, 200, 200, 0));
            ReaderMock reader = new ReaderMock(points);

            route = RouteRepository.AnalyzeRoute(reader);
        }

        [TestMethod]
        public void Basic()
        {
            Assert.AreEqual(4.01, route.Lenght);
            Assert.AreEqual(200, route.Elevation);
            Assert.AreEqual(1, route.Climbs.Count);
        }

        [TestMethod]
        public void MountInfo()
        {
            Assert.AreEqual(1, route.Climbs[0].Id);
            Assert.AreEqual(2000, route.Climbs[0].Lenght);
            Assert.AreEqual(0, route.Climbs[0].InitKm);
            Assert.AreEqual(200, route.Climbs[0].Elevation);
            Assert.AreEqual(0, route.Climbs[0].InitAltitude);
            Assert.AreEqual(200, route.Climbs[0].MaxAltitude);
            Assert.AreEqual(10, route.Climbs[0].Slope);
            Assert.AreEqual(10, route.Climbs[0].MaxSlope);
        }
    }

    [TestClass]
    public class UpFinish
    {
        Route route = default!;

        [TestInitialize]
        public void SetUp()
        {
            List<SectorInfo> points = new List<SectorInfo>();
            points.Add(new SectorInfo(0, 1, 0, 100, 0));
            points.Add(new SectorInfo(1, 2, 100, 200, 0));
            points.Add(new SectorInfo(2, 3, 200, 300, 0));
            points.Add(new SectorInfo(3, 4, 300, 400, 0));
            points.Add(new SectorInfo(4, 4.01, 400, 401, 0));
            ReaderMock reader = new ReaderMock(points);

            route = RouteRepository.AnalyzeRoute(reader);
        }

        [TestMethod]
        public void Basic()
        {
            Assert.AreEqual(4.01, route.Lenght);
            Assert.AreEqual(401, route.Elevation);
            Assert.AreEqual(1, route.Climbs.Count);
        }

        [TestMethod]
        public void MountInfo()
        {
            Assert.AreEqual(1, route.Climbs[0].Id);
            Assert.AreEqual(4010, route.Climbs[0].Lenght);
            Assert.AreEqual(0, route.Climbs[0].InitKm);
            Assert.AreEqual(401, route.Climbs[0].Elevation);
            Assert.AreEqual(0, route.Climbs[0].InitAltitude);
            Assert.AreEqual(401, route.Climbs[0].MaxAltitude);
            Assert.AreEqual(10, route.Climbs[0].Slope);
            Assert.AreEqual(10, route.Climbs[0].MaxSlope, 0.01);
        }
    }

    [TestClass]
    public class Multiple
    {
        Route route = default!;

        [TestInitialize]
        public void SetUp()
        {
            List<SectorInfo> points = new List<SectorInfo>();
            points.Add(new SectorInfo(0, 1, 0, 100, 0));
            points.Add(new SectorInfo(1, 2, 100, 200, 0));
            points.Add(new SectorInfo(2, 3, 200, 100, 0));
            points.Add(new SectorInfo(3, 5, 100, 100, 0));
            points.Add(new SectorInfo(5, 6, 100, 200, 0));
            ReaderMock reader = new ReaderMock(points);

            route = RouteRepository.AnalyzeRoute(reader);
        }

        [TestMethod]
        public void Basic()
        {
            Assert.AreEqual(6, route.Lenght);
            Assert.AreEqual(300, route.Elevation);
            Assert.AreEqual(2, route.Climbs.Count);
        }

        [TestMethod]
        public void MountInfo()
        {
            Assert.AreEqual(1, route.Climbs[0].Id);
            Assert.AreEqual(2000, route.Climbs[0].Lenght);
            Assert.AreEqual(0, route.Climbs[0].InitKm);
            Assert.AreEqual(200, route.Climbs[0].Elevation);
            Assert.AreEqual(0, route.Climbs[0].InitAltitude);
            Assert.AreEqual(200, route.Climbs[0].MaxAltitude);
            Assert.AreEqual(10, route.Climbs[0].Slope);
            Assert.AreEqual(10, route.Climbs[0].MaxSlope);

            Assert.AreEqual(2, route.Climbs[1].Id);
            Assert.AreEqual(1000, route.Climbs[1].Lenght);
            Assert.AreEqual(5, route.Climbs[1].InitKm);
            Assert.AreEqual(100, route.Climbs[1].Elevation);
            Assert.AreEqual(100, route.Climbs[1].InitAltitude);
            Assert.AreEqual(200, route.Climbs[1].MaxAltitude);
            Assert.AreEqual(10, route.Climbs[1].Slope);
            Assert.AreEqual(10, route.Climbs[1].MaxSlope);
        }
    }
}
