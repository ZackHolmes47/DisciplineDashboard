using DisciplineDashboard.Models;

namespace DisciplineDashboard.Models.ViewModels
{
    public class DashboardViewModel
    {
        public HabitCheckInViewModel CheckIn { get; set; } = new();
        public List<DashboardStreakViewModel> Streaks { get; set; } = new();
        public List<HabitCheckInItemViewModel> FaithHabits { get; set; } = new();
        public List<HabitCheckInItemViewModel> HealthHabits { get; set; } = new();
        public List<Goal> ActiveGoalsList { get; set; } = new();
        public List<Challenge> ActiveChallengesList { get; set; } = new();
        public string? TodaysMission { get; set; }
        public JournalEntry? YesterdayJournal { get; set; }
        public int CurrentStreak { get; set; }
        public int HabitsCompletedToday { get; set; }
        public int TotalHabitsToday { get; set; }
        public int ActiveGoals { get; set; }
        public int JournalEntries { get; set; }
        public int ActiveChallenges { get; set; }
        public string DailyVerseText { get; set; }
        public string DailyVerseReference { get; set; }
    }

    public class DashboardStreakViewModel
    {
        public int HabitID { get; set; }

        public string Name { get; set; }

        public string Category { get; set; }

        public int CurrentStreak { get; set; }

        public bool CompletedToday { get; set; }

        public bool AtRisk { get; set; }

        public bool Broken { get; set; }
    }
}