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

            // Pick a daily encouragement verse based on today's date.
            var dailyVerses = new[]
            {
                new { Text = "Commit your deeds to Yahweh, and your plans shall succeed.", Reference = "Proverbs 16:3" },
                new { Text = "Whatever you do, work heartily, as for the Lord and not for men.", Reference = "Colossians 3:23" },
                new { Text = "Let’s not be weary in doing good, for we will reap in due season if we don’t give up.", Reference = "Galatians 6:9" },
                new { Text = "I can do all things through Christ, who strengthens me.", Reference = "Philippians 4:13" },
                new { Text = "Be strong and courageous. Don’t be afraid or scared of them; for Yahweh your God himself is who goes with you.", Reference = "Deuteronomy 31:6" },
                new { Text = "For God didn’t give us a spirit of fear, but of power, love, and self-control.", Reference = "2 Timothy 1:7" },
                new { Text = "But those who wait for Yahweh will renew their strength.", Reference = "Isaiah 40:31" },
                new { Text = "Trust in Yahweh with all your heart, and don’t lean on your own understanding.", Reference = "Proverbs 3:5" },
                new { Text = "In all hard work there is profit, but the talk of the lips leads only to poverty.", Reference = "Proverbs 14:23" },
                new { Text = "Whatever your hand finds to do, do it with your might.", Reference = "Ecclesiastes 9:10" },
                new { Text = "No discipline seems pleasant at the time, but painful. Yet afterward it yields the peaceful fruit of righteousness.", Reference = "Hebrews 12:11" },
                new { Text = "Blessed is the man who perseveres under trial, for when he has stood the test, he will receive the crown of life.", Reference = "James 1:12" }
            };

            var verseIndex = DateTime.UtcNow.DayOfYear % dailyVerses.Length;
            var dailyVerse = dailyVerses[verseIndex];

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
                ActiveChallengesList = activeChallengesList,
                DailyVerseText = dailyVerse.Text,
                DailyVerseReference = dailyVerse.Reference
            };
        }
    }
}