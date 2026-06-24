using DisciplineDashboard.Models;
using DisciplineDashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DisciplineDashboard.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly DashboardService _dashboardService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
            DashboardService dashboardService,
            UserManager<ApplicationUser> userManager)
        {
            _dashboardService = dashboardService;
            _userManager = userManager;
        }

        // =========================================================
        // DASHBOARD HOME
        // Loads the user's personalized dashboard.
        // =========================================================
        public async Task<IActionResult> Index()
        {
            // Get the currently logged in user.
            var userID = _userManager.GetUserId(User);

            // Build the dashboard view model.
            var dashboard = await _dashboardService.GetDashboardAsync(userID);

            // Display the dashboard.
            return View(dashboard);
        }
    }
}