using DisciplineDashboard.Models;

namespace DisciplineDashboard.Services
{
    public class StreakService
    {
        // =========================================================
        // STRICT STREAK
        // Counts the streak starting from today.
        // =========================================================
        public int CalculateStrictStreak(List<HabitLog> logs)
        {
            int streak = 0;
            DateTime currentDate = DateTime.Today;

            // Keep counting while each day has a completed log.
            while (logs.Any(l => l.Date.Date == currentDate && l.Completed))
            {
                streak++;
                currentDate = currentDate.AddDays(-1);
            }

            return streak;
        }

        // =========================================================
        // COMPLETED TODAY
        // Checks if the habit was completed today.
        // =========================================================
        public bool CompletedToday(List<HabitLog> logs)
        {
            return logs.Any(l => l.Date.Date == DateTime.Today && l.Completed);
        }

        // =========================================================
        // COMPLETED YESTERDAY
        // Checks if the habit was completed yesterday.
        // =========================================================
        public bool CompletedYesterday(List<HabitLog> logs)
        {
            return logs.Any(l => l.Date.Date == DateTime.Today.AddDays(-1) && l.Completed);
        }

        // =========================================================
        // STREAK ENDING YESTERDAY
        // Keeps the streak visible if today's habit is not done yet.
        // =========================================================
        public int CalculateStreakEndingYesterday(List<HabitLog> logs)
        {
            int streak = 0;
            DateTime currentDate = DateTime.Today.AddDays(-1);

            // Count backwards starting from yesterday.
            while (logs.Any(l => l.Date.Date == currentDate && l.Completed))
            {
                streak++;
                currentDate = currentDate.AddDays(-1);
            }

            return streak;
        }
    }
}