using Route.Reader;

namespace Route.Test.Reader
{
    [TestClass]
    public class ReaderTest
    {
        [TestMethod]
        public void Read()
        {
            Gpx reader = new Gpx("TestFiles/test.gpx");

            Assert.IsTrue(reader.Read());

            Assert.AreEqual(5, reader.GetLenght(), 0.01);
            Assert.AreEqual(200, reader.GetElevation());

            Assert.AreEqual(5, reader.GetAllSectors().Last().EndPoint, 0.01);
            Assert.AreEqual(300, reader.GetAllSectors().Last().EndAlt, 0.1);
        }
    }
}