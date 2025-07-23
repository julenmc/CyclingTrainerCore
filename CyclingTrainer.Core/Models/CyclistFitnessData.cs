using CyclingTrainer.Core.Models;

namespace CyclingTrainer.Core.Models
{
    public class CyclistFitnessData
    {
        public DateTime UpdateDate { get; set; }
        public int Height { get; set; }
        public float Weight { get; set; }
        public float Vo2Max { get; set; }
        public string? MaxPowerCurveRaw { get; set; }
        public Dictionary<int, PowerCurveData>? MaxPowerCurve { get; set; }
        public List<Zone>? HrZones { get; set; }
        public List<Zone>? PowerZones { get; set; }
    }

    public class Zone
    {
        public int Id { get; set; }
        public int LowLimit { get; set; }
        public int HighLimit { get; set; }
    }
}