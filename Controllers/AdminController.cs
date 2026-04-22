using DonorTrackingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DonorTrackingSystem.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Displays all fund designations ordered by active status then name.
        /// </summary>
        public async Task<IActionResult> ManageFunds()
        {
            var funds = await _context.FundDesignations
                .OrderBy(f => f.ActiveStatus ? 0 : 1)
                .ThenBy(f => f.Name)
                .ToListAsync();

            return View(funds);
        }

        /// <summary>
        /// Returns the form for creating a new fund designation.
        /// </summary>
        public IActionResult AddFund()
        {
            return View();
        }

        /// <summary>
        /// Handles POST to create a new fund designation.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFund(FundDesignation fund)
        {
            if (ModelState.IsValid)
            {
                // Ensure the new fund is active by default
                _context.FundDesignations.Add(fund);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Fund '{fund.Name}' created successfully!";
                return RedirectToAction(nameof(ManageFunds));
            }

            return View(fund);
        }

        /// <summary>
        /// Returns the form for editing an existing fund designation.
        /// </summary>
        public async Task<IActionResult> EditFund(int? id)
        {
            // Validate that an ID was provided
            if (id == null) return NotFound();

            // Retrieve the fund designation to edit
            var fund = await _context.FundDesignations.FindAsync(id);
            if (fund == null) return NotFound();

            return View(fund);
        }

        /// <summary>
        /// Handles POST to update a fund designation.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFund(int id, FundDesignation fund)
        {
            if (id != fund.ID) return NotFound();

            if (ModelState.IsValid)
            {
                // Update the fund designation in the database
                try
                {
                    _context.Update(fund);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Fund '{fund.Name}' updated successfully!";
                    return RedirectToAction(nameof(ManageFunds));
                }
                // Handle concurrency issues if the record was modified by another user
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.FundDesignations.Any(f => f.ID == id)) return NotFound();
                    throw;
                }
            }

            return View(fund);
        }

        /// <summary>
        /// Deactivates a fund designation so it is no longer available for new donations.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateFund(int id)
        {
            // Retrieve the fund designation to deactivate
            var fund = await _context.FundDesignations.FindAsync(id);
            if (fund != null)
            {
                fund.ActiveStatus = false;
                _context.Update(fund);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Fund '{fund.Name}' has been deactivated.";
            }

            return RedirectToAction(nameof(ManageFunds));
        }

        /// <summary>
        /// Reactivates a previously deactivated fund designation.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReactivateFund(int id)
        {
            // Retrieve the fund designation to reactivate
            var fund = await _context.FundDesignations.FindAsync(id);
            if (fund != null)
            {
                fund.ActiveStatus = true;
                _context.Update(fund);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Fund '{fund.Name}' has been reactivated.";
            }

            return RedirectToAction(nameof(ManageFunds));
        }
    }
}
