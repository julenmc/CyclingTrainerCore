using Route.Reader;

namespace Route.Test.Reader
{
    [TestClass]
    public class ReaderTest
    {
        [TestMethod]
        public void ShortRoute()
        {
            Gpx reader = new Gpx("TestFiles/test.gpx");

            Assert.IsTrue(reader.Read());

            Assert.AreEqual(4, reader.GetLenght(), 0.01);
            Assert.AreEqual(200, reader.GetElevation());

            Assert.AreEqual(4, reader.GetAllSectors().Last().EndPoint, 0.01);
            Assert.AreEqual(300, reader.GetAllSectors().Last().EndAlt, 0.1);
        }

        [TestMethod]
        public void LongRoute()
        {
            Gpx reader = new Gpx("TestFiles/test_long.gpx");

            Assert.IsTrue(reader.Read());

            Assert.AreEqual(80, reader.GetLenght(), 0.1);
            Assert.AreEqual(1500, reader.GetElevation());

            Assert.AreEqual(80, reader.GetAllSectors().Last().EndPoint, 0.1);
            Assert.AreEqual(1000, reader.GetAllSectors().Last().EndAlt, 0.1);
        }
    }
}