namespace TrainingDatabase.Core.Models
{
    public class Cyclist
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? FullName => $"{Name} {LastName}";
        public DateTime BirthDate { get; set; }
        public int Age => DateTime.Now.Year - BirthDate.Year - (DateTime.Now.DayOfYear < BirthDate.DayOfYear ? 1 : 0);
        public CyclistEvolution? Details { get; set; }
        public Dictionary<int, int>? MaxPowerCurve { get; set; }
    }
}