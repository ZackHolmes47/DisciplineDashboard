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

        public async Task<IActionResult> Index()
        {
            var userID = _userManager.GetUserId(User);
            var today = DateTime.Today;

            var todayEntry = await _dbContext.JournalEntries
                .FirstOrDefaultAsync(j => j.UserID == userID && j.Date == today);

            if (todayEntry == null)
            {
                todayEntry = new JournalEntry
                {
                    UserID = userID,
                    Date = today,
                    CreatedAt = DateTime.Now
                };

                _dbContext.JournalEntries.Add(todayEntry);
                await _dbContext.SaveChangesAsync();
            }

            var pastEntries = await _dbContext.JournalEntries
                .Where(j => j.UserID == userID && j.Date < today)
                .OrderByDescending(j => j.Date)
                .Take(10)
                .ToListAsync();

            ViewBag.PastEntries = pastEntries;

            return View(todayEntry);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userID = _userManager.GetUserId(User);

            var entry = await _dbContext.JournalEntries
                .FirstOrDefaultAsync(j => j.JournalEntryID == id && j.UserID == userID);

            if (entry == null)
            {
                return NotFound();
            }

            return View(entry);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, JournalEntry entry)
        {
            ModelState.Remove(nameof(JournalEntry.UserID));

            if (id != entry.JournalEntryID)
            {
                return NotFound();
            }

            var userID = _userManager.GetUserId(User);

            var entryFromDb = await _dbContext.JournalEntries
                .FirstOrDefaultAsync(j => j.JournalEntryID == id && j.UserID == userID);

            if (entryFromDb == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                entryFromDb.MorningMission = entry.MorningMission;
                entryFromDb.Gratitude = entry.Gratitude;
                entryFromDb.Reflection = entry.Reflection;
                entryFromDb.WhatToImprove = entry.WhatToImprove;
                entryFromDb.TomorrowMission = entry.TomorrowMission;
                entryFromDb.Mood = entry.Mood;
                entryFromDb.UpdatedAt = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(entry);
        }
    }
}