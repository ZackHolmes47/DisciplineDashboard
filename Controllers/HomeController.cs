using DisciplineDashboard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DisciplineDashboard.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        // =========================================================
        // HOME PAGE
        // Shows the public landing page or redirects logged in users.
        // =========================================================
        [AllowAnonymous]
        public IActionResult Index()
        {
            // Logged in users go straight to their dashboard.
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        // =========================================================
        // ABOUT PAGE
        // Displays information about the application.
        // =========================================================
        [AllowAnonymous]
        public IActionResult About()
        {
            return View();
        }

        // =========================================================
        // FEATURES PAGE
        // Highlights the main features of the application.
        // =========================================================
        [AllowAnonymous]
        public IActionResult Features()
        {
            return View();
        }

        // =========================================================
        // PRIVACY PAGE
        // Displays the application's privacy policy.
        // =========================================================
        public IActionResult Privacy()
        {
            return View();
        }

        // =========================================================
        // ERROR PAGE
        // Displays application errors and request information.
        // =========================================================
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}