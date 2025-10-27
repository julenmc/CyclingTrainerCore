namespace CyclingTrainer.Core.Models
{
    public class Session
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Path { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double Distance { get; set; }
        public double HeightDiff { get; set; }
        public bool IsIndoor { get; set; }
        public AnalyzedData AnalyzedData { get; set; } = default!;
    }
}