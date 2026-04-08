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

        /// <summary>
        /// Displays the edit form for a specific congregant.
        /// </summary>
        /// <param name="id">The ID of the congregant to edit.</param>
        /// <returns>A view with the congregant's current information for editing, or NotFound if the congregant doesn't exist.</returns>
        public async Task<IActionResult> EditCongregant(int? id)
        {
            // Check if the ID is null and return NotFound if it is
            if (id == null)
            {
                return NotFound();
            }

            // Retrieve the congregant from the database using the provided ID
            var congregant = await _context.Congregants.FindAsync(id);
            if (congregant == null)
            {
                return NotFound();
            }

            return View(congregant);
        }

        /// <summary>
        /// Handles the POST request to update a congregant's information in the database.
        /// </summary>
        /// <param name="id">The ID of the congregant to update.</param>
        /// <param name="congregant">The updated congregant information.</param>
        /// <returns>A redirect to the list of congregants if the update is successful; otherwise, returns the view with validation errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCongregant(int id, Congregant congregant)
        {
            // Check if the ID in the route matches the ID of the congregant
            if (id != congregant.ID)
            {
                return NotFound();
            }

            // Validate the model state before attempting to update the congregant
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(congregant);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Congregant '{congregant.Name}' updated successfully!";
                    return RedirectToAction(nameof(ViewCongregants));
                }
                // Handle concurrency exceptions that may occur if the congregant was modified by another user
                catch (DbUpdateConcurrencyException)
                {
                    if (!CongregantExists(congregant.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(congregant);
        }

        /// <summary>
        /// Deletes (archives) a congregant from the system by setting their status to LeftChurch.
        /// </summary>
        /// <param name="id">The ID of the congregant to archive.</param>
        /// <returns>A redirect to the list of congregants with a success message.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveCongregant(int id)
        {
            // Retrieve the congregant from the database using the provided ID
            var congregant = await _context.Congregants.FindAsync(id);
            if (congregant != null)
            {
                // Archive by setting status to LeftChurch instead of deleting
                congregant.ActiveStatus = ActiveStatus.LeftChurch;
                _context.Update(congregant);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Congregant '{congregant.Name}' has been marked as having left the church.";
            }

            return RedirectToAction(nameof(ViewCongregants));
        }

        /// <summary>
        /// Restores an archived congregant by setting their status back to CurrentMember.
        /// </summary>
        /// <param name="id">The ID of the congregant to restore.</param>
        /// <returns>A redirect to the list of congregants with a success message.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreCongregant(int id)
        {
            // Retrieve the congregant from the database using the provided ID
            var congregant = await _context.Congregants.FindAsync(id);
            if (congregant != null)
            {
                // Restore by setting status to CurrentMember
                congregant.ActiveStatus = ActiveStatus.CurrentMember;
                _context.Update(congregant);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Congregant '{congregant.Name}' has been restored as a current member.";
            }

            return RedirectToAction(nameof(ViewCongregants));
        }

        /// <summary>
        /// Checks if a congregant exists in the database.
        /// </summary>
        /// <param name="id">The ID of the congregant to check.</param>
        /// <returns>True if the congregant exists; otherwise, false.</returns>
        private bool CongregantExists(int id)
        {
            return _context.Congregants.Any(e => e.ID == id);
        }

    }
}
