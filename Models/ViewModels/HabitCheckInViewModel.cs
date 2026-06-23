namespace DisciplineDashboard.Models.ViewModels
{
    public class HabitCheckInViewModel
    {
        public List<HabitCheckInItemViewModel> Habits { get; set; } = new();
    }

    public class HabitCheckInItemViewModel
    {
        public int HabitID { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public double? TargetValue { get; set; }
        public string? Unit { get; set; }
        public double? ActualValue { get; set; }
        public bool Completed { get; set; }
        public double ProgressPercent { get; set; }
        public string ProgressText => TargetValue != null
            ? $"{ActualValue ?? 0} / {TargetValue} {Unit}"
            : Completed ? "Completed" : "Not Completed";
    }
}