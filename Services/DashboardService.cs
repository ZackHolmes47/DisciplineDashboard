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

        public async Task<DashboardViewModel> GetDashboardAsync(string userID)
        {
            var today = DateTime.Today;

            var habits = await _dbContext.Habits
                .Where(h => h.UserID == userID && h.IsActive)
                .OrderBy(h => h.Category)
                .ThenBy(h => h.Name)
                .ToListAsync();

            var todayLogs = await _dbContext.HabitLogs
                .Where(l => l.UserID == userID && l.Date == today)
                .ToListAsync();

            var allLogs = await _dbContext.HabitLogs
                .Where(l => l.UserID == userID)
                .ToListAsync();

            var yesterday = today.AddDays(-1);

            var yesterdayJournal = await _dbContext.JournalEntries
                .FirstOrDefaultAsync(j => j.UserID == userID && j.Date == yesterday);

            var todaysMission = yesterdayJournal?.TomorrowMission;

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

            var faithHabits = checkInModel.Habits
                .Where(h => h.Category == "Faith")
                .ToList();

            var healthHabits = checkInModel.Habits
                .Where(h => h.Category == "Health")
                .ToList();

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

            var currentStreak = streaks.Any()
                ? streaks.Max(s => s.CurrentStreak)
                : 0;

            var habitsCompletedToday = checkInModel.Habits
                .Count(h => h.Completed);

            var totalHabitsToday = checkInModel.Habits.Count;

            //May use later for a dashboard goal count, but not currently used in the dashboard view model
            //var activeGoals = await _dbContext.Goals
            //    .CountAsync(g => g.UserID == userID && !g.IsCompleted);

            var activeGoals = 0;
            var activeChallenges = 0;

            var journalEntries = await _dbContext.JournalEntries
                .CountAsync(j => j.UserID == userID);


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
                JournalEntries = journalEntries
            };
        }
    }
}