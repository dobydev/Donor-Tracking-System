using DonorTrackingSystem.Models;
using DonorTrackingSystem.ViewModels;
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

        // ── Merge Congregants ─────────────────────────────────────────────────

        /// <summary>
        /// Displays the merge congregant records page.
        /// </summary>
        public async Task<IActionResult> MergeCongregants()
        {
            var vm = new MergeCongregantViewModel
            {
                Congregants = await _context.Congregants.OrderBy(c => c.Name).ToListAsync()
            };
            return View(vm);
        }

        /// <summary>
        /// Merges the source congregant into the target: re-assigns all donations then deletes the source.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MergeCongregants(MergeCongregantViewModel vm)
        {
            if (vm.SourceId == vm.TargetId)
            {
                TempData["ErrorMessage"] = "Source and target records must be different.";
                vm.Congregants = await _context.Congregants.OrderBy(c => c.Name).ToListAsync();
                return View(vm);
            }

            var source = await _context.Congregants.FindAsync(vm.SourceId);
            var target = await _context.Congregants.FindAsync(vm.TargetId);

            if (source == null || target == null)
            {
                TempData["ErrorMessage"] = "One or both selected records could not be found.";
                vm.Congregants = await _context.Congregants.OrderBy(c => c.Name).ToListAsync();
                return View(vm);
            }

            // Re-assign all donations from source to target
            var donations = await _context.Donations
                .Where(d => d.CongregantID == vm.SourceId)
                .ToListAsync();

            foreach (var d in donations)
                d.CongregantID = vm.TargetId;

            // Remove source record and save all changes atomically
            _context.Congregants.Remove(source);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Merged '{source.Name}' into '{target.Name}'. {donations.Count} donation(s) transferred. No data was lost.";
            return RedirectToAction(nameof(MergeCongregants));
        }

        // ── Merge Non-Congregants ─────────────────────────────────────────────

        /// <summary>
        /// Displays the merge non-congregant records page.
        /// </summary>
        public async Task<IActionResult> MergeNonCongregants()
        {
            var vm = new MergeNonCongregantViewModel
            {
                NonCongregants = await _context.NonCongregants
                    .OrderBy(n => n.LastName).ThenBy(n => n.FirstName).ThenBy(n => n.CompanyOrganization)
                    .ToListAsync()
            };
            return View(vm);
        }

        /// <summary>
        /// Merges the source non-congregant into the target: re-assigns all donations then deletes the source.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MergeNonCongregants(MergeNonCongregantViewModel vm)
        {
            // Validate that source and target are different records
            if (vm.SourceId == vm.TargetId)
            {
                TempData["ErrorMessage"] = "Source and target records must be different.";
                vm.NonCongregants = await _context.NonCongregants
                    .OrderBy(n => n.LastName).ThenBy(n => n.FirstName).ThenBy(n => n.CompanyOrganization)
                    .ToListAsync();
                return View(vm);
            }

            // Retrieve source and target records from the database
            var source = await _context.NonCongregants.FindAsync(vm.SourceId);
            var target = await _context.NonCongregants.FindAsync(vm.TargetId);

            // Validate that both records exist
            if (source == null || target == null)
            {
                TempData["ErrorMessage"] = "One or both selected records could not be found.";
                vm.NonCongregants = await _context.NonCongregants
                    .OrderBy(n => n.LastName).ThenBy(n => n.FirstName).ThenBy(n => n.CompanyOrganization)
                    .ToListAsync();
                return View(vm);
            }

            // Determine display names for source and target for user feedback
            var sourceName = !string.IsNullOrWhiteSpace(source.FirstName) || !string.IsNullOrWhiteSpace(source.LastName)
                ? $"{source.FirstName} {source.LastName}".Trim()
                : source.CompanyOrganization ?? "Unknown";

            var targetName = !string.IsNullOrWhiteSpace(target.FirstName) || !string.IsNullOrWhiteSpace(target.LastName)
                ? $"{target.FirstName} {target.LastName}".Trim()
                : target.CompanyOrganization ?? "Unknown";

            // Re-assign all donations from source to target
            var donations = await _context.Donations
                .Where(d => d.NonCongregantID == vm.SourceId)
                .ToListAsync();

            // Update each donation's NonCongregantID to point to the target record
            foreach (var d in donations)
                d.NonCongregantID = vm.TargetId;

            // Remove source record and save all changes atomically
            _context.NonCongregants.Remove(source);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Merged '{sourceName}' into '{targetName}'. {donations.Count} donation(s) transferred. No data was lost.";
            return RedirectToAction(nameof(MergeNonCongregants));
        }
    }
}
