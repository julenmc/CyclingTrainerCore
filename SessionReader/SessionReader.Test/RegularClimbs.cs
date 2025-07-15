using CommonModels;
using SessionReader.Core.Models;
using SessionReader.Core.Repository;
using SessionReader.Test.Mocks;

namespace SessionReader.Test
{
    [TestClass]
    public class RegularClimbs
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
        public void Flat()
        {
            List<SectorInfo> points = new List<SectorInfo>();
            points.Add(new SectorInfo(0, 1000, 0, 0, 0));
            points.Add(new SectorInfo(1000, 2000, 0, 0, 0));
            points.Add(new SectorInfo(2000, 3000, 0, 0, 0));
            points.Add(new SectorInfo(3000, 4000, 0, 0, 0));
            ReaderMock reader = new ReaderMock(points);

            session = SessionRepository.AnalyzeRoute(reader);
            RouteSections routeSections = SessionRepository.GetRouteData();
            Assert.AreEqual(4000, session.Distance);
            Assert.AreEqual(0, session.HeightDiff);
            Assert.AreEqual(0, routeSections.Climbs.Count);
        }

        [TestMethod]
        public void UpDown()
        {
            List<SectorInfo> points = new List<SectorInfo>();
            points.Add(new SectorInfo(0, 1000, 0, 100, 0));
            points.Add(new SectorInfo(1000, 2000, 100, 200, 0));
            points.Add(new SectorInfo(2000, 3000, 200, 100, 0));
            points.Add(new SectorInfo(3000, 4000, 100, 100, 0));
            points.Add(new SectorInfo(4000, 4010, 100, 100, 0));
            ReaderMock reader = new ReaderMock(points);
            session = SessionRepository.AnalyzeRoute(reader);
            RouteSections routeSections = SessionRepository.GetRouteData();

            Assert.AreEqual(4010, session.Distance);
            Assert.AreEqual(200, session.HeightDiff);
            Assert.AreEqual(1, routeSections.Climbs.Count);
            Assert.AreEqual(1, routeSections.Climbs[0].Id);
            Assert.AreEqual(2000, routeSections.Climbs[0].Distance);
            Assert.AreEqual(0, routeSections.Climbs[0].InitRouteDistance);
            Assert.AreEqual(200, routeSections.Climbs[0].HeightDiff);
            Assert.AreEqual(0, routeSections.Climbs[0].AltitudeInit);
            Assert.AreEqual(200, routeSections.Climbs[0].AltitudeEnd);
            Assert.AreEqual(10, routeSections.Climbs[0].AverageSlope);
            Assert.AreEqual(10, routeSections.Climbs[0].MaxSlope);
        }

        [TestMethod]
        public void UpFlat()
        {
            List<SectorInfo> points = new List<SectorInfo>();
            points.Add(new SectorInfo(0, 1000, 0, 100, 0));
            points.Add(new SectorInfo(1000, 2000, 100, 200, 0));
            points.Add(new SectorInfo(2000, 3000, 200, 200, 0));
            points.Add(new SectorInfo(3000, 4000, 200, 200, 0));
            points.Add(new SectorInfo(4000, 4010, 200, 200, 0));
            ReaderMock reader = new ReaderMock(points);
            session = SessionRepository.AnalyzeRoute(reader);
            RouteSections routeSections = SessionRepository.GetRouteData();

            Assert.AreEqual(4010, session.Distance);
            Assert.AreEqual(200, session.HeightDiff);
            Assert.AreEqual(1, routeSections.Climbs.Count);
            Assert.AreEqual(1, routeSections.Climbs[0].Id);
            Assert.AreEqual(2000, routeSections.Climbs[0].Distance);
            Assert.AreEqual(0, routeSections.Climbs[0].InitRouteDistance);
            Assert.AreEqual(200, routeSections.Climbs[0].HeightDiff);
            Assert.AreEqual(0, routeSections.Climbs[0].AltitudeInit);
            Assert.AreEqual(200, routeSections.Climbs[0].AltitudeEnd);
            Assert.AreEqual(10, routeSections.Climbs[0].AverageSlope);
            Assert.AreEqual(10, routeSections.Climbs[0].MaxSlope);
        }

        [TestMethod]
        public void UpFinish()
        {
            List<SectorInfo> points = new List<SectorInfo>();
            points.Add(new SectorInfo(0, 1000, 0, 100, 0));
            points.Add(new SectorInfo(1000, 2000, 100, 200, 0));
            points.Add(new SectorInfo(2000, 3000, 200, 300, 0));
            points.Add(new SectorInfo(3000, 4000, 300, 400, 0));
            points.Add(new SectorInfo(4000, 4010, 400, 401, 0));
            ReaderMock reader = new ReaderMock(points);
            session = SessionRepository.AnalyzeRoute(reader);
            RouteSections routeSections = SessionRepository.GetRouteData();

            Assert.AreEqual(4010, session.Distance);
            Assert.AreEqual(401, session.HeightDiff);
            Assert.AreEqual(1, routeSections.Climbs.Count);
            Assert.AreEqual(1, routeSections.Climbs[0].Id);
            Assert.AreEqual(4010, routeSections.Climbs[0].Distance);
            Assert.AreEqual(0, routeSections.Climbs[0].InitRouteDistance);
            Assert.AreEqual(401, routeSections.Climbs[0].HeightDiff);
            Assert.AreEqual(0, routeSections.Climbs[0].AltitudeInit);
            Assert.AreEqual(401, routeSections.Climbs[0].AltitudeEnd);
            Assert.AreEqual(10, routeSections.Climbs[0].AverageSlope);
            Assert.AreEqual(10, routeSections.Climbs[0].MaxSlope, 0.01);
        }

        [TestMethod]
        public void Multiple()
        {
            List<SectorInfo> points = new List<SectorInfo>();
            points.Add(new SectorInfo(0, 1000, 0, 100, 0));
            points.Add(new SectorInfo(1000, 2000, 100, 200, 0));
            points.Add(new SectorInfo(2000, 3000, 200, 100, 0));
            points.Add(new SectorInfo(3000, 5000, 100, 100, 0));
            points.Add(new SectorInfo(5000, 6000, 100, 200, 0));
            ReaderMock reader = new ReaderMock(points);
            session = SessionRepository.AnalyzeRoute(reader);
            RouteSections routeSections = SessionRepository.GetRouteData();

            Assert.AreEqual(6000, session.Distance);
            Assert.AreEqual(300, session.HeightDiff);
            Assert.AreEqual(2, routeSections.Climbs.Count);

            Assert.AreEqual(1, routeSections.Climbs[0].Id);
            Assert.AreEqual(2000, routeSections.Climbs[0].Distance);
            Assert.AreEqual(0, routeSections.Climbs[0].InitRouteDistance);
            Assert.AreEqual(200, routeSections.Climbs[0].HeightDiff);
            Assert.AreEqual(0, routeSections.Climbs[0].AltitudeInit);
            Assert.AreEqual(200, routeSections.Climbs[0].AltitudeEnd);
            Assert.AreEqual(10, routeSections.Climbs[0].AverageSlope);
            Assert.AreEqual(10, routeSections.Climbs[0].MaxSlope);

            Assert.AreEqual(2, routeSections.Climbs[1].Id);
            Assert.AreEqual(1000, routeSections.Climbs[1].Distance);
            Assert.AreEqual(5000, routeSections.Climbs[1].InitRouteDistance);
            Assert.AreEqual(100, routeSections.Climbs[1].HeightDiff);
            Assert.AreEqual(100, routeSections.Climbs[1].AltitudeInit);
            Assert.AreEqual(200, routeSections.Climbs[1].AltitudeEnd);
            Assert.AreEqual(10, routeSections.Climbs[1].AverageSlope);
            Assert.AreEqual(10, routeSections.Climbs[1].MaxSlope);
        }

        [TestMethod]
        public void FinishWithWarning()
        {
            List<SectorInfo> points = new List<SectorInfo>();
            points.Add(new SectorInfo(0, 1000, 0, 100, 0));
            points.Add(new SectorInfo(1000, 2000, 100, 200, 0));
            points.Add(new SectorInfo(2000, 2100, 200, 200, 0));
            ReaderMock reader = new ReaderMock(points);

            session = SessionRepository.AnalyzeRoute(reader);
            RouteSections routeSections = SessionRepository.GetRouteData();
            Assert.AreEqual(2100, session.Distance);
            Assert.AreEqual(200, session.HeightDiff);
            Assert.AreEqual(1, routeSections.Climbs.Count);
            Assert.AreEqual(1, routeSections.Climbs[0].Id);
            Assert.AreEqual(2000, routeSections.Climbs[0].Distance);
            Assert.AreEqual(0, routeSections.Climbs[0].InitRouteDistance);
            Assert.AreEqual(200, routeSections.Climbs[0].HeightDiff);
            Assert.AreEqual(0, routeSections.Climbs[0].AltitudeInit);
            Assert.AreEqual(200, routeSections.Climbs[0].AltitudeEnd);
            Assert.AreEqual(10, routeSections.Climbs[0].AverageSlope);
            Assert.AreEqual(10, routeSections.Climbs[0].MaxSlope, 0.01);
        }
    }
}
