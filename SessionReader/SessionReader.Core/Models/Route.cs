namespace SessionReader.Core.Models
{
    public class Route
    {
        public double Lenght { get; set; } = default!;
        public double Elevation { get; set; } = default!;
        public List<Climb> Climbs { get; set; } = default!;
        public string Name { get; set; } = default!;
        public List<SectorInfo> Sectors { get; set; } = default!;

        internal Route() { }
    }
}