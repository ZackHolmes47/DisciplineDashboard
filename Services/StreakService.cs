using DisciplineDashboard.Models;

namespace DisciplineDashboard.Services
{
    public class StreakService
    {
        public int CalculateStrictStreak(List<HabitLog> logs)
        {
            int streak = 0;
            DateTime currentDate = DateTime.Today;

            while (logs.Any(l => l.Date.Date == currentDate && l.Completed))
            {
                streak++;
                currentDate = currentDate.AddDays(-1);
            }

            return streak;
        }

        public bool CompletedToday(List<HabitLog> logs)
        {
            return logs.Any(l => l.Date.Date == DateTime.Today && l.Completed);
        }

        public bool CompletedYesterday(List<HabitLog> logs)
        {
            return logs.Any(l => l.Date.Date == DateTime.Today.AddDays(-1) && l.Completed);
        }

        public int CalculateStreakEndingYesterday(List<HabitLog> logs)
        {
            int streak = 0;
            DateTime currentDate = DateTime.Today.AddDays(-1);

            while (logs.Any(l => l.Date.Date == currentDate && l.Completed))
            {
                streak++;
                currentDate = currentDate.AddDays(-1);
            }

            return streak;
        }
    }
}