using DisciplineDashboard.Data;
using DisciplineDashboard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DisciplineDashboard.Controllers
{
    [Authorize]
    public class GoalsController : Controller
    {
        private readonly DisciplineDashboardDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public GoalsController(DisciplineDashboardDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        // =========================================================
        // GOAL LIST
        // Shows all goals for the logged in user.
        // =========================================================
        public async Task<IActionResult> Index()
        {
            var userID = _userManager.GetUserId(User);

            // Load this user's goals.
            var goals = await _dbContext.Goals
                .Where(g => g.UserID == userID)
                .OrderBy(g => g.IsCompleted)
                .ThenBy(g => g.TargetDate)
                .ToListAsync();

            return View(goals);
        }

        // =========================================================
        // GOAL DETAILS
        // Shows more information about one goal.
        // =========================================================
        public async Task<IActionResult> Details(int id)
        {
            var userID = _userManager.GetUserId(User);

            // Load only the current user's goal.
            var goal = await _dbContext.Goals
                .FirstOrDefaultAsync(g => g.GoalID == id && g.UserID == userID);

            if (goal == null)
            {
                return NotFound();
            }

            return View(goal);
        }

        // =========================================================
        // CREATE GOAL PAGE
        // Opens the form for adding a new goal.
        // =========================================================
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // =========================================================
        // CREATE GOAL
        // Saves a new goal for the logged in user.
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Goal goal)
        {
            // UserID is added manually from the logged in user.
            ModelState.Remove(nameof(Goal.UserID));

            if (ModelState.IsValid)
            {
                // Set default values before saving.
                goal.UserID = _userManager.GetUserId(User);
                goal.CreatedAt = DateTime.UtcNow;
                goal.IsCompleted = false;
                goal.CompletedAt = null;

                _dbContext.Goals.Add(goal);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(goal);
        }

        // =========================================================
        // EDIT GOAL PAGE
        // Opens the edit form for one of the user's goals.
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userID = _userManager.GetUserId(User);

            // Make sure the goal belongs to this user.
            var goal = await _dbContext.Goals
                .FirstOrDefaultAsync(g => g.GoalID == id && g.UserID == userID);

            if (goal == null)
            {
                return NotFound();
            }

            return View(goal);
        }

        // =========================================================
        // EDIT GOAL
        // Updates an existing goal for the logged in user.
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Goal goal)
        {
            // UserID stays tied to the logged in user.
            ModelState.Remove(nameof(Goal.UserID));

            if (id != goal.GoalID)
            {
                return NotFound();
            }

            var userID = _userManager.GetUserId(User);

            // Load the existing goal safely from the database.
            var goalFromDb = await _dbContext.Goals
                .FirstOrDefaultAsync(g => g.GoalID == id && g.UserID == userID);

            if (goalFromDb == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Update only the editable goal fields.
                goalFromDb.Name = goal.Name;
                goalFromDb.Category = goal.Category;
                goalFromDb.Description = goal.Description;
                goalFromDb.TargetDate = goal.TargetDate;
                if (goal.IsCompleted && !goalFromDb.IsCompleted)
                {
                    goalFromDb.CompletedAt = DateTime.UtcNow;
                }
                else if (!goal.IsCompleted)
                {
                    goalFromDb.CompletedAt = null;
                }

                goalFromDb.IsCompleted = goal.IsCompleted;

                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(goal);
        }

        // =========================================================
        // DELETE GOAL
        // Removes the goal after confirmation.
        // =========================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userID = _userManager.GetUserId(User);

            // Find the goal before removing it.
            var goal = await _dbContext.Goals
                .FirstOrDefaultAsync(g => g.GoalID == id && g.UserID == userID);

            if (goal != null)
            {
                _dbContext.Goals.Remove(goal);
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}