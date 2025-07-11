namespace SessionReader.Core.Models
{
    public class SectorInfo
    {
        public double StartPoint { get; set; }
        public double EndPoint { get; set; }
        public double StartAlt { get; set; }
        public double EndAlt { get; set; }
        public double Slope { get; set; }

        public SectorInfo() { }

        public SectorInfo(double sp, double ep, double sa, double ea, double sl)
        {
            StartPoint = sp;
            EndPoint = ep;
            StartAlt = sa;
            EndAlt = ea;
            Slope = sl;
        }
    }
}
