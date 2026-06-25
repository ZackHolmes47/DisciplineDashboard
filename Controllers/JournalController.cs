using DisciplineDashboard.Data;
using DisciplineDashboard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DisciplineDashboard.Controllers
{
    [Authorize]
    public class JournalController : Controller
    {
        private readonly DisciplineDashboardDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public JournalController(
            DisciplineDashboardDbContext dbContext,
            UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        // =========================================================
        // JOURNAL HOME
        // Loads today's journal entry and recent journal history.
        // =========================================================
        public async Task<IActionResult> Index()
        {
            var userID = _userManager.GetUserId(User);
            var today = DateTime.Today;

            // Look for today's journal entry.
            var todayEntry = await _dbContext.JournalEntries
                .FirstOrDefaultAsync(j => j.UserID == userID && j.Date == today);

            // Create today's entry if one doesn't exist.
            if (todayEntry == null)
            {
                todayEntry = new JournalEntry
                {
                    UserID = userID,
                    Date = today,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.JournalEntries.Add(todayEntry);
                await _dbContext.SaveChangesAsync();
            }

            // Load the 10 most recent past journal entries.
            var pastEntries = await _dbContext.JournalEntries
                .Where(j => j.UserID == userID && j.Date < today)
                .OrderByDescending(j => j.Date)
                .Take(10)
                .ToListAsync();

            // Pass previous entries to the view.
            ViewBag.PastEntries = pastEntries;

            return View(todayEntry);
        }

        // =========================================================
        // EDIT JOURNAL PAGE
        // Opens an existing journal entry for editing.
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userID = _userManager.GetUserId(User);

            // Load the selected journal entry.
            var entry = await _dbContext.JournalEntries
                .FirstOrDefaultAsync(j => j.JournalEntryID == id && j.UserID == userID);

            if (entry == null)
            {
                return NotFound();
            }

            return View(entry);
        }

        // =========================================================
        // SAVE JOURNAL CHANGES
        // Updates an existing journal entry.
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, JournalEntry entry)
        {
            // UserID is assigned from the logged in user.
            ModelState.Remove(nameof(JournalEntry.UserID));

            if (id != entry.JournalEntryID)
            {
                return NotFound();
            }

            var userID = _userManager.GetUserId(User);

            // Load the existing journal entry.
            var entryFromDb = await _dbContext.JournalEntries
                .FirstOrDefaultAsync(j => j.JournalEntryID == id && j.UserID == userID);

            if (entryFromDb == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Update the editable journal fields.
                entryFromDb.MorningMission = entry.MorningMission;
                entryFromDb.Gratitude = entry.Gratitude;
                entryFromDb.Reflection = entry.Reflection;
                entryFromDb.WhatToImprove = entry.WhatToImprove;
                entryFromDb.TomorrowMission = entry.TomorrowMission;
                entryFromDb.Mood = entry.Mood;
                entryFromDb.UpdatedAt = DateTime.UtcNow;

                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(entry);
        }
    }
}