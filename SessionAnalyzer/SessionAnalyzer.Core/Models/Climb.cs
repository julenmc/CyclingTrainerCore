namespace SessionAnalyzer.Core.Models
{
    public class Climb
    {
        public int Id { get; set; }
        public double Lenght { get; set; }
        public double InitKm { get; set; }
        public double Elevation { get; set; }
        public double InitAltitude { get; set; }
        public double MaxAltitude { get; set; }
        public double Slope { get; set; }
        public double MaxSlope { get; set; }

        public Climb()
        {
            Id = 0;
            Lenght = 0;
            InitKm = 0;
            Elevation = 0;
            InitAltitude = 0;
            MaxAltitude = 0;
            Slope = 0;
            MaxSlope = 0;
        }
    }
}
