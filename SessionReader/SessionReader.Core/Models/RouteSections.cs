using CyclingTrainer.Core.Models;

namespace CyclingTrainer.SessionReader.Core.Models
{
    public class RouteSections
    {
        public List<Climb> Climbs { get; set; } = default!;
        public List<SectorInfo> Sectors { get; set; } = default!;
    }
}
