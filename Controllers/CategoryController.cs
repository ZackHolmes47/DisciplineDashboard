using DisciplineDashboard.Data;
using DisciplineDashboard.Models;
using DisciplineDashboard.Models.ViewModels;
using DisciplineDashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DisciplineDashboard.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly DisciplineDashboardDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly StreakService _streakService;

        public CategoryController(
            DisciplineDashboardDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            StreakService streakService)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _streakService = streakService;
        }

        // =========================================================
        // CATEGORY DETAILS
        // Shows all habits, check-ins, and streaks for one category.
        // =========================================================
        public async Task<IActionResult> Details(string category)
        {
            var userID = _userManager.GetUserId(User);
            var today = DateTime.Today;

            // Make sure a category was selected.
            if (string.IsNullOrWhiteSpace(category))
            {
                return RedirectToAction("Index", "Habits");
            }

            // Verify the category belongs to this user.
            var categoryExists = await _dbContext.Habits
                .AnyAsync(h => h.UserID == userID &&
                               h.IsActive &&
                               h.Category == category);

            if (!categoryExists)
            {
                return NotFound();
            }

            // Load the active habits in this category.
            var habits = await _dbContext.Habits
                .Where(h => h.UserID == userID &&
                            h.IsActive &&
                            h.Category == category)
                .OrderBy(h => h.Name)
                .ToListAsync();

            // Get today's logs for progress tracking.
            var todayLogs = await _dbContext.HabitLogs
                .Where(l => l.UserID == userID && l.Date == today)
                .ToListAsync();

            // Get all logs for streak calculations.
            var allLogs = await _dbContext.HabitLogs
                .Where(l => l.UserID == userID)
                .ToListAsync();

            // Pass the category name to the view.
            ViewBag.Category = category;

            // Build the check-in cards.
            ViewBag.HabitItems = habits.Select(h =>
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
                        : (log?.Completed ?? false ? 100 : 0)
                };
            }).ToList();

            // Build streak information for each habit.
            ViewBag.Streaks = habits.Select(h =>
            {
                var habitLogs = allLogs.Where(l => l.HabitID == h.HabitID).ToList();

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
            }).ToList();

            // Display the category page.
            return View();
        }
    }
}