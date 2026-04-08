using DonorTrackingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DonorTrackingSystem.Controllers
{
    [Authorize(Roles = "Office Manager")]
    public class OfficeManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OfficeManagerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Returns the view for adding a new congregant.
        /// </summary>
        /// <returns>A view that displays the form for entering a new congregant's details.</returns>
        public IActionResult AddCongregant()
        {
            return View();
        }

        /// <summary>
        /// Handles the POST request to add a new congregant to the database. Validates the input and saves the new congregant if valid, then redirects to the list of congregants with a success message.
        /// </summary>
        /// <param name="congregant">The congregant to be added. Must contain valid congregant details.</param>
        /// <returns>A redirect to the list of congregants if the addition is successful; otherwise, returns the view with validation errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCongregant(Congregant congregant)
        {
            if (ModelState.IsValid)
            {
                _context.Congregants.Add(congregant);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Congregant added successfully!";
                return RedirectToAction(nameof(ViewCongregants));
            }

            return View(congregant);
        }

        /// <summary>
        /// Retrieves a list of congregants ordered by name and displays them in the view.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/>
        /// that renders the view with the list of congregants.</returns>
        public async Task<IActionResult> ViewCongregants()
        {
            var congregants = await _context.Congregants
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(congregants);
        }

    }
}
