using DisciplineDashboard.Data;
using DisciplineDashboard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DisciplineDashboard.Controllers
{
    [Authorize]
    public class ChallengesController : Controller
    {
        private readonly DisciplineDashboardDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChallengesController(DisciplineDashboardDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        // =========================================================
        // CHALLENGE LIST
        // Shows all challenges for the logged in user.
        // =========================================================
        public async Task<IActionResult> Index()
        {
            var userID = _userManager.GetUserId(User);

            // Load this user's challenges.
            var challenges = await _dbContext.Challenges
                .Where(c => c.UserID == userID)
                .OrderBy(c => c.IsCompleted)
                .ThenBy(c => c.EndDate)
                .ToListAsync();

            return View(challenges);
        }

        // =========================================================
        // CREATE CHALLENGE PAGE
        // Opens the form for adding a new challenge.
        // =========================================================

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // =========================================================
        // CREATE CHALLENGE
        // Saves a new challenge for the logged in user.
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Challenge challenge)
        {
            // UserID is added manually from the logged in user.
            ModelState.Remove("UserID");
            if (ModelState.IsValid)
            {
                // Set default values before saving.
                challenge.UserID = _userManager.GetUserId(User);
                challenge.CurrentValue = 0;
                challenge.StartDate = DateTime.Today;
                challenge.IsCompleted = false;
                challenge.CreatedAt = DateTime.Now;

                _dbContext.Challenges.Add(challenge);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(challenge);
        }
    }
}