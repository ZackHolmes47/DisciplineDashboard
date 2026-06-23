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

        public async Task<IActionResult> Index()
        {
            var userID = _userManager.GetUserId(User);

            var dashboard = await _dashboardService.GetDashboardAsync(userID);

            return View(dashboard);
        }
    }
}