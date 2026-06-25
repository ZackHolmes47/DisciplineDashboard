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
        // SAVE CHALLENGE PROGRESS
        // Updates progress for all active challenges.
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProgress(Dictionary<int, decimal> progressValues)
        {
            var userID = _userManager.GetUserId(User);

            // Load this user's active challenges.
            var challenges = await _dbContext.Challenges
                .Where(c => c.UserID == userID && !c.IsCompleted)
                .ToListAsync();

            foreach (var challenge in challenges)
            {
                if (progressValues.TryGetValue(challenge.ChallengeID, out var newValue))
                {
                    challenge.CurrentValue = newValue;

                    if (challenge.CurrentValue >= challenge.TargetValue)
                    {
                        challenge.IsCompleted = true;
                        challenge.CompletedAt = DateTime.Now;
                    }
                }
            }

            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // =========================================================
        // CHALLENGE DETAILS
        // Shows more information about one challenge.
        // =========================================================
        public async Task<IActionResult> Details(int id)
        {
            var userID = _userManager.GetUserId(User);

            // Load only the current user's challenge.
            var challenge = await _dbContext.Challenges
                .FirstOrDefaultAsync(c => c.ChallengeID == id && c.UserID == userID);

            if (challenge == null)
            {
                return NotFound();
            }

            return View(challenge);
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

        // =========================================================
        // EDIT CHALLENGE PAGE
        // Opens the edit form for one of the user's challenges.
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userID = _userManager.GetUserId(User);

            // Make sure the challenge belongs to this user.
            var challenge = await _dbContext.Challenges
                .FirstOrDefaultAsync(c => c.ChallengeID == id && c.UserID == userID);

            if (challenge == null)
            {
                return NotFound();
            }

            return View(challenge);
        }

        // =========================================================
        // EDIT CHALLENGE
        // Updates challenge details and progress.
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Challenge challenge)
        {
            // UserID stays tied to the logged in user.
            ModelState.Remove("UserID");

            if (id != challenge.ChallengeID)
            {
                return NotFound();
            }

            var userID = _userManager.GetUserId(User);

            // Load the existing challenge safely from the database.
            var challengeFromDb = await _dbContext.Challenges
                .FirstOrDefaultAsync(c => c.ChallengeID == id && c.UserID == userID);

            if (challengeFromDb == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Update only the editable challenge fields.
                challengeFromDb.Name = challenge.Name;
                challengeFromDb.Category = challenge.Category;
                challengeFromDb.Description = challenge.Description;
                challengeFromDb.TargetValue = challenge.TargetValue;
                challengeFromDb.CurrentValue = challenge.CurrentValue;
                challengeFromDb.Unit = challenge.Unit;
                challengeFromDb.EndDate = challenge.EndDate;

                if (challenge.CurrentValue >= challenge.TargetValue && !challengeFromDb.IsCompleted)
                {
                    challengeFromDb.IsCompleted = true;
                    challengeFromDb.CompletedAt = DateTime.Now;
                }
                else if (challenge.CurrentValue < challenge.TargetValue)
                {
                    challengeFromDb.IsCompleted = false;
                    challengeFromDb.CompletedAt = null;
                }

                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Details), new { id = challengeFromDb.ChallengeID });
            }

            return View(challenge);
        }

        // =========================================================
        // DELETE CHALLENGE
        // Removes the challenge after confirmation.
        // =========================================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userID = _userManager.GetUserId(User);

            // Find the challenge before removing it.
            var challenge = await _dbContext.Challenges
                .FirstOrDefaultAsync(c => c.ChallengeID == id && c.UserID == userID);

            if (challenge != null)
            {
                _dbContext.Challenges.Remove(challenge);
                await _dbContext.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}