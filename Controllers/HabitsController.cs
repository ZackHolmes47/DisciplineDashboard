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

        public async Task<IActionResult> Index()
        {
            var userID = _userManager.GetUserId(User);

            var habits = await _dbContext.Habits
                .Where(h => h.UserID == userID)
                .OrderBy(h => h.Category)
                .ThenBy(h => h.Name)
                .ToListAsync();

            return View(habits);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Habit habit)
        {
            ModelState.Remove(nameof(Habit.UserID));

            if (ModelState.IsValid)
            {
                habit.UserID = _userManager.GetUserId(User);
                habit.CreatedAt = DateTime.Now;
                habit.IsActive = true;

                _dbContext.Add(habit);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(habit);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            var habit = await _dbContext.Habits
                .FirstOrDefaultAsync(h => h.HabitID == id && h.UserID == userId);

            if (habit == null)
            {
                return NotFound();
            }

            return View(habit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Habit habit)
        {
            ModelState.Remove(nameof(Habit.UserID));

            if (id != habit.HabitID)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            var habitFromDb = await _dbContext.Habits
                .FirstOrDefaultAsync(h => h.HabitID == id && h.UserID == userId);

            if (habitFromDb == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
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

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var userID = _userManager.GetUserId(User);

            var habit = await _dbContext.Habits
                .FirstOrDefaultAsync(h => h.HabitID == id && h.UserID == userID);

            if (habit == null)
            {
                return NotFound();
            }

            return View(habit);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userID = _userManager.GetUserId(User);

            var habit = await _dbContext.Habits
                .FirstOrDefaultAsync(h => h.HabitID == id && h.UserID == userID);

            if (habit != null)
            {
                _dbContext.Habits.Remove(habit);
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> CheckIn()
        {
            var userID = _userManager.GetUserId(User);
            var today = DateTime.Today;

            var habits = await _dbContext.Habits
                .Where(h => h.UserID == userID && h.IsActive)
                .OrderBy(h => h.Category)
                .ThenBy(h => h.Name)
                .ToListAsync();

            var logs = await _dbContext.HabitLogs
                .Where(l => l.UserID == userID && l.Date == today)
                .ToListAsync();

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn(HabitCheckInViewModel viewModel)
        {
            Console.WriteLine("POST CHECKIN HIT");

            var userID = _userManager.GetUserId(User);
            var today = DateTime.Today;

            foreach (var item in viewModel.Habits)
            {
                var habit = await _dbContext.Habits
                    .FirstOrDefaultAsync(h => h.HabitID == item.HabitID && h.UserID == userID);

                if (habit == null)
                {
                    continue;
                }

                var log = await _dbContext.HabitLogs
                    .FirstOrDefaultAsync(l => l.HabitID == item.HabitID &&
                                              l.UserID == userID &&
                                              l.Date == today);

                var completed = item.Completed;

                if (habit.TargetValue.HasValue && item.ActualValue.HasValue)
                {
                    completed = item.ActualValue.Value >= habit.TargetValue.Value;
                }

                if (log == null)
                {
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
                    log.ActualValue = item.ActualValue;
                    log.Completed = completed;
                }
            }

            await _dbContext.SaveChangesAsync();

            return RedirectToAction("Index", "Dashboard");
        }
    }
}
