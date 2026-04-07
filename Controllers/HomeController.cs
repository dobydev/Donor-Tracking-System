using DonorTrackingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DonorTrackingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// This action method returns the view for the home page of the application. It is the default action that gets called when a user navigates to the root URL of the application.
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }


        // Provided by the default ASP.NET Core MVC template, this action method is responsible for handling errors that occur within the application.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
