using SessionReader.Core.Models;
using SessionReader.Core.Repository;
using SessionReader.Test.Mocks;

namespace SessionReader.Test
{
    [TestClass]
    public class IrregularClimbs
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
        public void UpFinish()
        {
            List<SectorInfo> points = new List<SectorInfo>
            {
                new SectorInfo(0, 1, 0, 100, 0),
                new SectorInfo(1, 2, 100, 200, 0),
                new SectorInfo(2, 2.8, 200, 200, 0),
                new SectorInfo(2.8, 4, 200, 300, 0),
                new SectorInfo(4, 5, 300, 400, 0)
            };
            ReaderMock reader = new ReaderMock(points);
            session = SessionRepository.AnalyzeRoute(reader);

            Assert.AreEqual(5, session.Route.Lenght);
            Assert.AreEqual(400, session.Route.Elevation);
            Assert.AreEqual(1, session.Route.Climbs.Count);
            Assert.AreEqual(1, session.Route.Climbs[0].Id);
            Assert.AreEqual(5000, session.Route.Climbs[0].Lenght);
            Assert.AreEqual(0, session.Route.Climbs[0].InitKm);
            Assert.AreEqual(400, session.Route.Climbs[0].Elevation);
            Assert.AreEqual(0, session.Route.Climbs[0].InitAltitude);
            Assert.AreEqual(400, session.Route.Climbs[0].MaxAltitude);
            Assert.AreEqual(8, session.Route.Climbs[0].Slope);
            Assert.AreEqual(10, session.Route.Climbs[0].MaxSlope);
        }

        [TestMethod]
        public void DoubleWarning()
        {
            List<SectorInfo> points = new List<SectorInfo>
            {
                new SectorInfo(0, 1, 0, 100, 0),
                new SectorInfo(1, 2, 100, 200, 0),
                new SectorInfo(2, 2.8, 200, 200, 0),
                new SectorInfo(2.8, 4, 200, 300, 0),
                new SectorInfo(4, 4.2, 300, 280, 0),
                new SectorInfo(4.2, 5, 280, 350, 0),
                new SectorInfo(5, 6, 350, 300, 0),
                new SectorInfo(6, 7, 300, 250, 0),
                new SectorInfo(7, 8, 250, 200, 0),
                new SectorInfo(8, 9, 200, 150, 0)
            };
            ReaderMock reader = new ReaderMock(points);
            SessionData session = SessionRepository.AnalyzeRoute(reader);

            Assert.AreEqual(9, session.Route.Lenght);
            Assert.AreEqual(370, session.Route.Elevation);
            Assert.AreEqual(1, session.Route.Climbs.Count);
            Assert.AreEqual(1, session.Route.Climbs[0].Id);
            Assert.AreEqual(5000, session.Route.Climbs[0].Lenght);
            Assert.AreEqual(0, session.Route.Climbs[0].InitKm);
            Assert.AreEqual(370, session.Route.Climbs[0].Elevation);
            Assert.AreEqual(0, session.Route.Climbs[0].InitAltitude);
            Assert.AreEqual(350, session.Route.Climbs[0].MaxAltitude);
        }
    }
}
