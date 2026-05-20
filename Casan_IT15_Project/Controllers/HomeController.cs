using Casan_IT15_Project.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Casan_IT15_Project.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Redirect("http://localhost:5173");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
