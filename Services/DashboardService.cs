using DisciplineDashboard.Data;
using DisciplineDashboard.Models;
using DisciplineDashboard.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DisciplineDashboard.Services
{
    public class DashboardService
    {
        private readonly DisciplineDashboardDbContext _dbContext;
        private readonly StreakService _streakService;

        public DashboardService(DisciplineDashboardDbContext dbContext, StreakService streakService)
        {
            _dbContext = dbContext;
            _streakService = streakService;
        }

        // =========================================================
        // BUILD DASHBOARD
        // Gathers the habits, logs, journal info, streaks, and stats for the dashboard.
        // =========================================================
        public async Task<DashboardViewModel> GetDashboardAsync(string userID)
        {
            var today = DateTime.UtcNow.Date;

            // Load the user's active habits.
            var habits = await _dbContext.Habits
                .Where(h => h.UserID == userID && h.IsActive)
                .OrderBy(h => h.Category)
                .ThenBy(h => h.Name)
                .ToListAsync();

            // Load today's habit logs.
            var todayLogs = await _dbContext.HabitLogs
                .Where(l => l.UserID == userID && l.Date == today)
                .ToListAsync();

            // Load all logs for streak calculations.
            var allLogs = await _dbContext.HabitLogs
                .Where(l => l.UserID == userID)
                .ToListAsync();

            var yesterday = today.AddDays(-1);

            // Pull yesterday's journal to get today's mission.
            var yesterdayJournal = await _dbContext.JournalEntries
                .FirstOrDefaultAsync(j => j.UserID == userID && j.Date == yesterday);

            var todaysMission = yesterdayJournal?.TomorrowMission;

            // Build the check-in model for today's habits.
            var checkInModel = new HabitCheckInViewModel
            {
                Habits = habits.Select(h =>
                {
                    var log = todayLogs.FirstOrDefault(l => l.HabitID == h.HabitID);

                    return new HabitCheckInItemViewModel
                    {
                        HabitID = h.HabitID,
                        Name = h.Name,
                        Category = h.Category,
                        TargetValue = h.TargetValue,
                        Unit = h.Unit,
                        ActualValue = log?.ActualValue,
                        Completed = log?.Completed ?? false,
                        ProgressPercent = h.TargetValue.HasValue && h.TargetValue > 0
                            ? Math.Min(((log?.ActualValue ?? 0) / h.TargetValue.Value) * 100, 100)
                            : (log?.Completed ?? false ? 100 : 0),

                    };
                }).ToList()
            };

            // Separate habits into the dashboard category cards.
            var faithHabits = checkInModel.Habits
                .Where(h => h.Category == "Faith")
                .ToList();

            var healthHabits = checkInModel.Habits
                .Where(h => h.Category == "Health")
                .ToList();

            // Build streak data for each active habit.
            var streaks = habits.Select(h =>
            {
                var habitLogs = allLogs
                    .Where(l => l.HabitID == h.HabitID)
                    .ToList();

                var completedToday = _streakService.CompletedToday(habitLogs);
                var completedYesterday = _streakService.CompletedYesterday(habitLogs);

                var currentStreak = completedToday
                    ? _streakService.CalculateStrictStreak(habitLogs)
                    : _streakService.CalculateStreakEndingYesterday(habitLogs);

                return new DashboardStreakViewModel
                {
                    HabitID = h.HabitID,
                    Name = h.Name,
                    Category = h.Category,
                    CurrentStreak = currentStreak,
                    CompletedToday = completedToday,
                    AtRisk = !completedToday && completedYesterday,
                    Broken = !completedToday && !completedYesterday
                };
            })
            .OrderByDescending(s => s.CurrentStreak)
            .ThenBy(s => s.Name)
            .ToList();

            // Use the highest individual habit streak for now.
            var currentStreak = streaks.Any()
                ? streaks.Max(s => s.CurrentStreak)
                : 0;

            // Count how many habits were completed today.
            var habitsCompletedToday = checkInModel.Habits
                .Count(h => h.Completed);

            var totalHabitsToday = checkInModel.Habits.Count;

            // Count active goals for the stat card.
            var activeGoals = await _dbContext.Goals
                .CountAsync(g => g.UserID == userID && !g.IsCompleted);

            // Count active challenges for the stat card.
            var activeChallenges = await _dbContext.Challenges
                .CountAsync(c => c.UserID == userID && !c.IsCompleted);

            // Count all journal entries for this user.
            var journalEntries = await _dbContext.JournalEntries
                .CountAsync(j => j.UserID == userID);

            // Load active goals for the dashboard card.
            var activeGoalsList = await _dbContext.Goals
                .Where(g => g.UserID == userID && !g.IsCompleted)
                .OrderBy(g => g.TargetDate)
                .Take(3)
                .ToListAsync();

            // Load active challenges for the dashboard card.
            var activeChallengesList = await _dbContext.Challenges
                .Where(c => c.UserID == userID && !c.IsCompleted)
                .OrderBy(c => c.EndDate)
                .Take(3)
                .ToListAsync();

            // Send everything to the dashboard view.
            return new DashboardViewModel
            {
                CheckIn = checkInModel,
                Streaks = streaks,
                FaithHabits = faithHabits,
                HealthHabits = healthHabits,
                TodaysMission = todaysMission,
                YesterdayJournal = yesterdayJournal,
                CurrentStreak = currentStreak,
                HabitsCompletedToday = habitsCompletedToday,
                TotalHabitsToday = totalHabitsToday,
                ActiveGoals = activeGoals,
                JournalEntries = journalEntries,
                ActiveGoalsList = activeGoalsList,
                ActiveChallenges = activeChallenges,
                ActiveChallengesList = activeChallengesList
            };
        }
    }
}