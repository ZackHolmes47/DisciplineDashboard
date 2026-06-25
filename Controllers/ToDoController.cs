using DisciplineDashboard.Data;
using DisciplineDashboard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DisciplineDashboard.Controllers
{
    [Authorize]
    public class TodoController : Controller
    {
        private readonly DisciplineDashboardDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public TodoController(
            DisciplineDashboardDbContext dbContext,
            UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        // =========================================================
        // TODO LIST
        // Displays the current user's to-do items.
        // =========================================================
        public async Task<IActionResult> Index()
        {
            var userID = _userManager.GetUserId(User);

            // Load incomplete tasks first, then completed ones.
            var todos = await _dbContext.TodoItems
                .Where(t => t.UserID == userID)
                .OrderBy(t => t.IsCompleted)
                .ThenBy(t => t.CreatedAt)
                .ToListAsync();

            return View(todos);
        }

        // =========================================================
        // ADD TODO ITEM
        // Creates a new to-do item without leaving the page.
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(string text)
        {
            var userID = _userManager.GetUserId(User);

            if (!string.IsNullOrWhiteSpace(text))
            {
                var todo = new TodoItem
                {
                    UserID = userID,
                    Text = text.Trim(),
                    IsCompleted = false,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.TodoItems.Add(todo);
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // TOGGLE TODO ITEM
        // Marks a to-do item complete or incomplete.
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            var userID = _userManager.GetUserId(User);

            var todo = await _dbContext.TodoItems
                .FirstOrDefaultAsync(t => t.TodoItemID == id && t.UserID == userID);

            if (todo != null)
            {
                todo.IsCompleted = !todo.IsCompleted;
                todo.CompletedAt = todo.IsCompleted ? DateTime.UtcNow : null;

                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // DELETE TODO ITEM
        // Removes a to-do item from the list.
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userID = _userManager.GetUserId(User);

            var todo = await _dbContext.TodoItems
                .FirstOrDefaultAsync(t => t.TodoItemID == id && t.UserID == userID);

            if (todo != null)
            {
                _dbContext.TodoItems.Remove(todo);
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}