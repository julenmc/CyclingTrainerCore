using SessionReader.Core.Models;
using SessionReader.Core.Repository;
using SessionReader.Test.Mocks;

namespace SessionReader.Test
{
    [TestClass]
    public class RegularClimbs
    {
        SessionData session = default!;

        [TestInitialize]
        public void SetUp()
        {
            Monitor.Enter(LockClass.LockObject);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Monitor.Exit(LockClass.LockObject);
        }

        [TestMethod]
        public void Flat()
        {
            List<SectorInfo> points = new List<SectorInfo>();
            points.Add(new SectorInfo(0, 1, 0, 0, 0));
            points.Add(new SectorInfo(1, 2, 0, 0, 0));
            points.Add(new SectorInfo(2, 3, 0, 0, 0));
            points.Add(new SectorInfo(3, 4, 0, 0, 0));
            ReaderMock reader = new ReaderMock(points);

            session = SessionRepository.AnalyzeRoute(reader);
            Assert.AreEqual(4, session.Route.Lenght);
            Assert.AreEqual(0, session.Route.Elevation);
            Assert.AreEqual(0, session.Route.Climbs.Count);
        }

        [TestMethod]
        public void UpDown()
        {
            List<SectorInfo> points = new List<SectorInfo>();
            points.Add(new SectorInfo(0, 1, 0, 100, 0));
            points.Add(new SectorInfo(1, 2, 100, 200, 0));
            points.Add(new SectorInfo(2, 3, 200, 100, 0));
            points.Add(new SectorInfo(3, 4, 100, 100, 0));
            points.Add(new SectorInfo(4, 4.01, 100, 100, 0));
            ReaderMock reader = new ReaderMock(points);
            session = SessionRepository.AnalyzeRoute(reader);

            Assert.AreEqual(4.01, session.Route.Lenght);
            Assert.AreEqual(200, session.Route.Elevation);
            Assert.AreEqual(1, session.Route.Climbs.Count);
            Assert.AreEqual(1, session.Route.Climbs[0].Id);
            Assert.AreEqual(2000, session.Route.Climbs[0].Lenght);
            Assert.AreEqual(0, session.Route.Climbs[0].InitKm);
            Assert.AreEqual(200, session.Route.Climbs[0].Elevation);
            Assert.AreEqual(0, session.Route.Climbs[0].InitAltitude);
            Assert.AreEqual(200, session.Route.Climbs[0].MaxAltitude);
            Assert.AreEqual(10, session.Route.Climbs[0].Slope);
            Assert.AreEqual(10, session.Route.Climbs[0].MaxSlope);
        }

        [TestMethod]
        public void UpFlat()
        {
            List<SectorInfo> points = new List<SectorInfo>();
            points.Add(new SectorInfo(0, 1, 0, 100, 0));
            points.Add(new SectorInfo(1, 2, 100, 200, 0));
            points.Add(new SectorInfo(2, 3, 200, 200, 0));
            points.Add(new SectorInfo(3, 4, 200, 200, 0));
            points.Add(new SectorInfo(4, 4.01, 200, 200, 0));
            ReaderMock reader = new ReaderMock(points);
            session = SessionRepository.AnalyzeRoute(reader);

            Assert.AreEqual(4.01, session.Route.Lenght);
            Assert.AreEqual(200, session.Route.Elevation);
            Assert.AreEqual(1, session.Route.Climbs.Count);
            Assert.AreEqual(1, session.Route.Climbs[0].Id);
            Assert.AreEqual(2000, session.Route.Climbs[0].Lenght);
            Assert.AreEqual(0, session.Route.Climbs[0].InitKm);
            Assert.AreEqual(200, session.Route.Climbs[0].Elevation);
            Assert.AreEqual(0, session.Route.Climbs[0].InitAltitude);
            Assert.AreEqual(200, session.Route.Climbs[0].MaxAltitude);
            Assert.AreEqual(10, session.Route.Climbs[0].Slope);
            Assert.AreEqual(10, session.Route.Climbs[0].MaxSlope);
        }

        [TestMethod]
        public void UpFinish()
        {
            List<SectorInfo> points = new List<SectorInfo>();
            points.Add(new SectorInfo(0, 1, 0, 100, 0));
            points.Add(new SectorInfo(1, 2, 100, 200, 0));
            points.Add(new SectorInfo(2, 3, 200, 300, 0));
            points.Add(new SectorInfo(3, 4, 300, 400, 0));
            points.Add(new SectorInfo(4, 4.01, 400, 401, 0));
            ReaderMock reader = new ReaderMock(points);
            session = SessionRepository.AnalyzeRoute(reader);

            Assert.AreEqual(4.01, session.Route.Lenght);
            Assert.AreEqual(401, session.Route.Elevation);
            Assert.AreEqual(1, session.Route.Climbs.Count);
            Assert.AreEqual(1, session.Route.Climbs[0].Id);
            Assert.AreEqual(4010, session.Route.Climbs[0].Lenght);
            Assert.AreEqual(0, session.Route.Climbs[0].InitKm);
            Assert.AreEqual(401, session.Route.Climbs[0].Elevation);
            Assert.AreEqual(0, session.Route.Climbs[0].InitAltitude);
            Assert.AreEqual(401, session.Route.Climbs[0].MaxAltitude);
            Assert.AreEqual(10, session.Route.Climbs[0].Slope);
            Assert.AreEqual(10, session.Route.Climbs[0].MaxSlope, 0.01);
        }

        [TestMethod]
        public void Multiple()
        {
            List<SectorInfo> points = new List<SectorInfo>();
            points.Add(new SectorInfo(0, 1, 0, 100, 0));
            points.Add(new SectorInfo(1, 2, 100, 200, 0));
            points.Add(new SectorInfo(2, 3, 200, 100, 0));
            points.Add(new SectorInfo(3, 5, 100, 100, 0));
            points.Add(new SectorInfo(5, 6, 100, 200, 0));
            ReaderMock reader = new ReaderMock(points);
            session = SessionRepository.AnalyzeRoute(reader);

            Assert.AreEqual(6, session.Route.Lenght);
            Assert.AreEqual(300, session.Route.Elevation);
            Assert.AreEqual(2, session.Route.Climbs.Count);

            Assert.AreEqual(1, session.Route.Climbs[0].Id);
            Assert.AreEqual(2000, session.Route.Climbs[0].Lenght);
            Assert.AreEqual(0, session.Route.Climbs[0].InitKm);
            Assert.AreEqual(200, session.Route.Climbs[0].Elevation);
            Assert.AreEqual(0, session.Route.Climbs[0].InitAltitude);
            Assert.AreEqual(200, session.Route.Climbs[0].MaxAltitude);
            Assert.AreEqual(10, session.Route.Climbs[0].Slope);
            Assert.AreEqual(10, session.Route.Climbs[0].MaxSlope);

            Assert.AreEqual(2, session.Route.Climbs[1].Id);
            Assert.AreEqual(1000, session.Route.Climbs[1].Lenght);
            Assert.AreEqual(5, session.Route.Climbs[1].InitKm);
            Assert.AreEqual(100, session.Route.Climbs[1].Elevation);
            Assert.AreEqual(100, session.Route.Climbs[1].InitAltitude);
            Assert.AreEqual(200, session.Route.Climbs[1].MaxAltitude);
            Assert.AreEqual(10, session.Route.Climbs[1].Slope);
            Assert.AreEqual(10, session.Route.Climbs[1].MaxSlope);
        }

        [TestMethod]
        public void FinishWithWarning()
        {
            List<SectorInfo> points = new List<SectorInfo>();
            points.Add(new SectorInfo(0, 1, 0, 100, 0));
            points.Add(new SectorInfo(1, 2, 100, 200, 0));
            points.Add(new SectorInfo(2, 2.1, 200, 200, 0));
            ReaderMock reader = new ReaderMock(points);

            session = SessionRepository.AnalyzeRoute(reader);
            Assert.AreEqual(2.1, session.Route.Lenght);
            Assert.AreEqual(200, session.Route.Elevation);
            Assert.AreEqual(1, session.Route.Climbs.Count);
            Assert.AreEqual(1, session.Route.Climbs[0].Id);
            Assert.AreEqual(2000, session.Route.Climbs[0].Lenght);
            Assert.AreEqual(0, session.Route.Climbs[0].InitKm);
            Assert.AreEqual(200, session.Route.Climbs[0].Elevation);
            Assert.AreEqual(0, session.Route.Climbs[0].InitAltitude);
            Assert.AreEqual(200, session.Route.Climbs[0].MaxAltitude);
            Assert.AreEqual(10, session.Route.Climbs[0].Slope);
            Assert.AreEqual(10, session.Route.Climbs[0].MaxSlope, 0.01);
        }
    }
}
