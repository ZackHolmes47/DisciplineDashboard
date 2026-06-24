using DisciplineDashboard.Data;
using DisciplineDashboard.Models;
using DisciplineDashboard.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DisciplineDashboard.Controllers
{
    [Authorize]
    public class HabitsController : Controller
    {
        private readonly DisciplineDashboardDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public HabitsController(DisciplineDashboardDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        // =========================================================
        // HABIT LIST
        // Shows all habits for the logged in user.
        // =========================================================
        public async Task<IActionResult> Index()
        {
            var userID = _userManager.GetUserId(User);

            // Load this user's habits by category and name.
            var habits = await _dbContext.Habits
                .Where(h => h.UserID == userID)
                .OrderBy(h => h.Category)
                .ThenBy(h => h.Name)
                .ToListAsync();

            return View(habits);
        }

        // =========================================================
        // CREATE HABIT PAGE
        // Opens the form for adding a new habit.
        // =========================================================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // =========================================================
        // CREATE HABIT
        // Saves a new habit for the logged in user.
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Habit habit)
        {
            // UserID is added manually from the logged in user.
            ModelState.Remove(nameof(Habit.UserID));

            if (ModelState.IsValid)
            {
                // Set default values before saving.
                habit.UserID = _userManager.GetUserId(User);
                habit.CreatedAt = DateTime.Now;
                habit.IsActive = true;

                _dbContext.Add(habit);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(habit);
        }

        // =========================================================
        // EDIT HABIT PAGE
        // Opens the edit form for one of the user's habits.
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            // Make sure the habit belongs to this user.
            var habit = await _dbContext.Habits
                .FirstOrDefaultAsync(h => h.HabitID == id && h.UserID == userId);

            if (habit == null)
            {
                return NotFound();
            }

            return View(habit);
        }

        // =========================================================
        // EDIT HABIT
        // Updates an existing habit for the logged in user.
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Habit habit)
        {
            // UserID stays tied to the logged in user.
            ModelState.Remove(nameof(Habit.UserID));

            if (id != habit.HabitID)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            // Load the existing habit safely from the database.
            var habitFromDb = await _dbContext.Habits
                .FirstOrDefaultAsync(h => h.HabitID == id && h.UserID == userId);

            if (habitFromDb == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Update only the editable habit fields.
                habitFromDb.Name = habit.Name;
                habitFromDb.Category = habit.Category;
                habitFromDb.Description = habit.Description;
                habitFromDb.TargetValue = habit.TargetValue;
                habitFromDb.Unit = habit.Unit;
                habitFromDb.IsActive = habit.IsActive;

                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(habit);
        }

        // =========================================================
        // DELETE HABIT
        // Removes the habit after confirmation.
        // =========================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userID = _userManager.GetUserId(User);

            // Find the habit before removing it.
            var habit = await _dbContext.Habits
                .FirstOrDefaultAsync(h => h.HabitID == id && h.UserID == userID);

            if (habit != null)
            {
                _dbContext.Habits.Remove(habit);
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // CHECK-IN PAGE
        // Loads today's habit check-in form.
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> CheckIn()
        {
            var userID = _userManager.GetUserId(User);
            var today = DateTime.Today;

            // Load active habits for today's check-in.
            var habits = await _dbContext.Habits
                .Where(h => h.UserID == userID && h.IsActive)
                .OrderBy(h => h.Category)
                .ThenBy(h => h.Name)
                .ToListAsync();

            // Load any logs already created for today.
            var logs = await _dbContext.HabitLogs
                .Where(l => l.UserID == userID && l.Date == today)
                .ToListAsync();

            // Build the check-in view model.
            var viewModel = new HabitCheckInViewModel
            {
                Habits = habits.Select(h =>
                {
                    var log = logs.FirstOrDefault(l => l.HabitID == h.HabitID);

                    return new HabitCheckInItemViewModel
                    {
                        HabitID = h.HabitID,
                        Name = h.Name,
                        Category = h.Category,
                        TargetValue = h.TargetValue,
                        Unit = h.Unit,
                        ActualValue = log?.ActualValue,
                        Completed = log?.Completed ?? false
                    };
                }).ToList()
            };

            return View(viewModel);
        }

        // =========================================================
        // SAVE CHECK-IN
        // Creates or updates today's habit logs.
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(HabitCheckInViewModel viewModel)
        {
            Console.WriteLine("POST CHECKIN HIT");

            var userID = _userManager.GetUserId(User);
            var today = DateTime.Today;

            foreach (var item in viewModel.Habits)
            {
                // Make sure the habit belongs to this user.
                var habit = await _dbContext.Habits
                    .FirstOrDefaultAsync(h => h.HabitID == item.HabitID && h.UserID == userID);

                if (habit == null)
                {
                    continue;
                }

                // Look for an existing log for today.
                var log = await _dbContext.HabitLogs
                    .FirstOrDefaultAsync(l => l.HabitID == item.HabitID &&
                                              l.UserID == userID &&
                                              l.Date == today);

                var completed = item.Completed;

                // For number-based habits, compare actual value to the target.
                if (habit.TargetValue.HasValue && item.ActualValue.HasValue)
                {
                    completed = item.ActualValue.Value >= habit.TargetValue.Value;
                }

                if (log == null)
                {
                    // Create a new log if one does not exist yet.
                    log = new HabitLog
                    {
                        HabitID = habit.HabitID,
                        UserID = userID,
                        Date = today,
                        ActualValue = item.ActualValue,
                        Completed = completed,
                        CreatedAt = DateTime.Now
                    };

                    _dbContext.HabitLogs.Add(log);
                }
                else
                {
                    // Update today's existing log.
                    log.ActualValue = item.ActualValue;
                    log.Completed = completed;
                }
            }

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index", "Dashboard");
        }
    }
}