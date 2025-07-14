namespace SessionReader.Core.Models
{
    public class RouteData
    {
        public double Lenght { get; set; } = default!;
        public double Elevation { get; set; } = default!;
        public List<Climb> Climbs { get; set; } = default!;
        public List<SectorInfo> Sectors { get; set; } = default!;
    }
}
