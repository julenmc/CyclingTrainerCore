using CommonModels;
using SessionReader.Core.Models;
using SessionReader.Core.Repository;
using SessionReader.Test.Mocks;

namespace SessionReader.Test
{
    [TestClass]
    public class IrregularClimbs
    {
        Session session = default!;

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
                new SectorInfo(0, 1000, 0, 100, 0),
                new SectorInfo(1000, 2000, 100, 200, 0),
                new SectorInfo(2000, 2800, 200, 200, 0),
                new SectorInfo(2800, 4000, 200, 300, 0),
                new SectorInfo(4000, 5000, 300, 400, 0)
            };
            ReaderMock reader = new ReaderMock(points);
            session = SessionRepository.AnalyzeRoute(reader);
            RouteSections routeData = SessionRepository.GetRouteData();

            Assert.AreEqual(5000, session.Distance);
            Assert.AreEqual(400, session.HeightDiff);
            Assert.AreEqual(1, routeData.Climbs.Count);
            Assert.AreEqual(1, routeData.Climbs[0].Id);
            Assert.AreEqual(5000, routeData.Climbs[0].Distance);
            Assert.AreEqual(0, routeData.Climbs[0].InitRouteDistance);
            Assert.AreEqual(400, routeData.Climbs[0].HeightDiff);
            Assert.AreEqual(0, routeData.Climbs[0].AltitudeInit);
            Assert.AreEqual(400, routeData.Climbs[0].AltitudeEnd);
            Assert.AreEqual(8, routeData.Climbs[0].AverageSlope);
            Assert.AreEqual(10, routeData.Climbs[0].MaxSlope);
        }

        [TestMethod]
        public void DoubleWarning()
        {
            List<SectorInfo> points = new List<SectorInfo>
            {
                new SectorInfo(0, 1000, 0, 100, 0),
                new SectorInfo(1000, 2000, 100, 200, 0),
                new SectorInfo(2000, 2800, 200, 200, 0),
                new SectorInfo(2800, 4000, 200, 300, 0),
                new SectorInfo(4000, 4200, 300, 280, 0),
                new SectorInfo(4200, 5000, 280, 350, 0),
                new SectorInfo(5000, 6000, 350, 300, 0),
                new SectorInfo(6000, 7000, 300, 250, 0),
                new SectorInfo(7000, 8000, 250, 200, 0),
                new SectorInfo(8000, 9000, 200, 150, 0)
            };
            ReaderMock reader = new ReaderMock(points);
            Session session = SessionRepository.AnalyzeRoute(reader);
            RouteSections routeSections = SessionRepository.GetRouteData();

            Assert.AreEqual(9000, session.Distance);
            Assert.AreEqual(370, session.HeightDiff);
            Assert.AreEqual(1, routeSections.Climbs.Count);
            Assert.AreEqual(1, routeSections.Climbs[0].Id);
            Assert.AreEqual(5000, routeSections.Climbs[0].Distance);
            Assert.AreEqual(0, routeSections.Climbs[0].InitRouteDistance);
            Assert.AreEqual(370, routeSections.Climbs[0].HeightDiff);
            Assert.AreEqual(0, routeSections.Climbs[0].AltitudeInit);
            Assert.AreEqual(350, routeSections.Climbs[0].AltitudeEnd);
        }
    }
}
