using DonorTrackingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DonorTrackingSystem.Controllers
{
    [Authorize(Roles = "Office Manager, Administrator")]
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
        public async Task<IActionResult> DeactivateCongregant(int id)
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
        /// Displays the donation history view where an Office Manager can select a congregant to view their donation history.
        /// </summary>
        /// <returns>A view with a list of active congregants to select from.</returns>
        public async Task<IActionResult> ViewDonationHistory()
        {
            var congregants = await _context.Congregants
                .Where(c => c.ActiveStatus == ActiveStatus.CurrentMember)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(congregants);
        }

        /// <summary>
        /// Displays the donation history for a specific congregant.
        /// </summary>
        /// <param name="id">The ID of the congregant whose donation history should be displayed.</param>
        /// <returns>A view showing all donations made by the specified congregant, or NotFound if the congregant doesn't exist.</returns>
        public async Task<IActionResult> CongregantDonationHistory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var congregant = await _context.Congregants.FindAsync(id);
            if (congregant == null)
            {
                return NotFound();
            }

            var donations = await _context.Donations
                .Include(d => d.FundDesignation)
                .Where(d => d.CongregantID == id)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();

            ViewBag.CongregantName = congregant.Name;
            ViewBag.CongregantID = congregant.ID;

            return View(donations);
        }

        /// <summary>
        /// Displays the donation history selection view where an Office Manager can select a non-congregant to view their donation history.
        /// </summary>
        /// <returns>A view with a list of all non-congregant donors to select from.</returns>
        public async Task<IActionResult> ViewNonCongregantDonationHistory()
        {
            var nonCongregants = await _context.NonCongregants
                .OrderBy(n => n.LastName)
                .ThenBy(n => n.FirstName)
                .ThenBy(n => n.CompanyOrganization)
                .ToListAsync();

            return View(nonCongregants);
        }

        /// <summary>
        /// Displays the donation history for a specific non-congregant donor.
        /// </summary>
        /// <param name="id">The ID of the non-congregant whose donation history should be displayed.</param>
        /// <returns>A view showing all donations made by the specified non-congregant, or NotFound if they don't exist.</returns>
        public async Task<IActionResult> NonCongregantDonationHistory(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nonCongregant = await _context.NonCongregants.FindAsync(id);
            if (nonCongregant == null)
            {
                return NotFound();
            }

            var donations = await _context.Donations
                .Include(d => d.FundDesignation)
                .Where(d => d.NonCongregantID == id)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();

            var displayName = !string.IsNullOrWhiteSpace(nonCongregant.FirstName) || !string.IsNullOrWhiteSpace(nonCongregant.LastName)
                ? $"{nonCongregant.FirstName} {nonCongregant.LastName}".Trim()
                : nonCongregant.CompanyOrganization;

            ViewBag.NonCongregantName = displayName;
            ViewBag.NonCongregantID = nonCongregant.ID;

            return View(donations);
        }

        /// <summary>
        /// Retrieves and displays a list of all non-congregant donors, ordered by name and company.
        /// </summary>
        /// <returns>A view showing all non-congregant donor records.</returns>
        public async Task<IActionResult> ViewNonCongregants()
        {
            var nonCongregants = await _context.NonCongregants
                .OrderBy(n => n.IsActive ? 0 : 1) // Active first
                .ThenBy(n => n.LastName)
                .ThenBy(n => n.FirstName)
                .ThenBy(n => n.CompanyOrganization)
                .ToListAsync();

            return View(nonCongregants);
        }

        /// <summary>
        /// Displays the details for a specific non-congregant donor.
        /// </summary>
        /// <param name="id">The ID of the non-congregant to view.</param>
        /// <returns>A view with the non-congregant's details, or NotFound if they don't exist.</returns>
        public async Task<IActionResult> NonCongregantDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nonCongregant = await _context.NonCongregants.FindAsync(id);
            if (nonCongregant == null)
            {
                return NotFound();
            }

            // Get donation history for this non-congregant
            var donations = await _context.Donations
                .Include(d => d.FundDesignation)
                .Where(d => d.NonCongregantID == id)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();

            ViewBag.Donations = donations;

            return View(nonCongregant);
        }

        /// <summary>
        /// Displays the edit form for a specific non-congregant donor.
        /// </summary>
        /// <param name="id">The ID of the non-congregant to edit.</param>
        /// <returns>A view with the non-congregant's current information for editing, or NotFound if they don't exist.</returns>
        public async Task<IActionResult> EditNonCongregant(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nonCongregant = await _context.NonCongregants.FindAsync(id);
            if (nonCongregant == null)
            {
                return NotFound();
            }

            return View(nonCongregant);
        }

        /// <summary>
        /// Handles the POST request to update a non-congregant's information in the database.
        /// </summary>
        /// <param name="id">The ID of the non-congregant to update.</param>
        /// <param name="nonCongregant">The updated non-congregant information.</param>
        /// <returns>A redirect to the non-congregants list if the update is successful; otherwise, returns the view with validation errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditNonCongregant(int id, NonCongregant nonCongregant)
        {
            if (id != nonCongregant.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nonCongregant);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Non-congregant donor updated successfully!";
                    return RedirectToAction(nameof(ViewNonCongregants));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NonCongregantExists(nonCongregant.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(nonCongregant);
        }

        /// <summary>
        /// Deactivates a non-congregant donor by setting their IsActive status to false.
        /// </summary>
        /// <param name="id">The ID of the non-congregant to deactivate.</param>
        /// <returns>A redirect to the non-congregants list with a success message.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateNonCongregant(int id)
        {
            var nonCongregant = await _context.NonCongregants.FindAsync(id);
            if (nonCongregant != null)
            {
                nonCongregant.IsActive = false;
                _context.Update(nonCongregant);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Non-congregant donor '{GetNonCongregantDisplayName(nonCongregant)}' has been deactivated.";
            }

            return RedirectToAction(nameof(ViewNonCongregants));
        }

        /// <summary>
        /// Reactivates a non-congregant donor by setting their IsActive status to true.
        /// </summary>
        /// <param name="id">The ID of the non-congregant to reactivate.</param>
        /// <returns>A redirect to the non-congregants list with a success message.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReactivateNonCongregant(int id)
        {
            var nonCongregant = await _context.NonCongregants.FindAsync(id);
            if (nonCongregant != null)
            {
                nonCongregant.IsActive = true;
                _context.Update(nonCongregant);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Non-congregant donor '{GetNonCongregantDisplayName(nonCongregant)}' has been reactivated.";
            }

            return RedirectToAction(nameof(ViewNonCongregants));
        }

        /// <summary>
        /// Helper method to get a display name for a non-congregant.
        /// </summary>
        /// <param name="nonCongregant">The non-congregant donor.</param>
        /// <returns>A string representing the non-congregant's name or company.</returns>
        private string GetNonCongregantDisplayName(NonCongregant nonCongregant)
        {
            if (!string.IsNullOrWhiteSpace(nonCongregant.FirstName) || !string.IsNullOrWhiteSpace(nonCongregant.LastName))
            {
                return $"{nonCongregant.FirstName} {nonCongregant.LastName}".Trim();
            }
            return nonCongregant.CompanyOrganization ?? "Unknown";
        }

        /// <summary>
        /// Checks if a non-congregant exists in the database.
        /// </summary>
        /// <param name="id">The ID of the non-congregant to check.</param>
        /// <returns>True if the non-congregant exists; otherwise, false.</returns>
        private bool NonCongregantExists(int id)
        {
            return _context.NonCongregants.Any(e => e.ID == id);
        }

        /// <summary>
        /// Displays the edit form for a specific donation.
        /// </summary>
        /// <param name="id">The ID of the donation to edit.</param>
        /// <param name="congregantId">The ID of the congregant to return to after editing.</param>
        /// <param name="nonCongregantId">The ID of the non-congregant to return to after editing.</param>
        /// <returns>A view with the donation's current information for editing, or NotFound if the donation doesn't exist.</returns>
        public async Task<IActionResult> EditDonation(int? id, int? congregantId, int? nonCongregantId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donation = await _context.Donations
                .Include(d => d.FundDesignation)
                .Include(d => d.Congregant)
                .Include(d => d.NonCongregant)
                .FirstOrDefaultAsync(d => d.ID == id);

            if (donation == null)
            {
                return NotFound();
            }

            // Populate fund designations dropdown
            ViewBag.FundDesignations = new SelectList(
                _context.FundDesignations.Where(f => f.ActiveStatus),
                "ID",
                "Name",
                donation.FundDesignationID
            );

            // Store IDs for return navigation
            ViewBag.CongregantID = congregantId ?? donation.CongregantID;
            ViewBag.NonCongregantID = nonCongregantId ?? donation.NonCongregantID;

            // Determine display name
            if (donation.Congregant != null)
            {
                ViewBag.DonorName = donation.Congregant.Name;
                ViewBag.IsCongregant = true;
            }
            else if (donation.NonCongregant != null)
            {
                var nc = donation.NonCongregant;
                ViewBag.DonorName = (!string.IsNullOrWhiteSpace(nc.FirstName) || !string.IsNullOrWhiteSpace(nc.LastName))
                    ? $"{nc.FirstName} {nc.LastName}".Trim()
                    : nc.CompanyOrganization ?? "Unknown";
                ViewBag.IsCongregant = false;
            }
            else
            {
                ViewBag.DonorName = "Unknown";
                ViewBag.IsCongregant = true;
            }

            return View(donation);
        }

        /// <summary>
        /// Handles the POST request to update a donation's information in the database.
        /// </summary>
        /// <param name="id">The ID of the donation to update.</param>
        /// <param name="donation">The updated donation information.</param>
        /// <param name="congregantId">The ID of the congregant to return to after editing.</param>
        /// <param name="nonCongregantId">The ID of the non-congregant to return to after editing.</param>
        /// <returns>A redirect to the donation history if the update is successful; otherwise, returns the view with validation errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDonation(int id, Donation donation, int? congregantId, int? nonCongregantId)
        {
            if (id != donation.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Retrieve the original donation to preserve certain fields
                    var originalDonation = await _context.Donations.AsNoTracking().FirstOrDefaultAsync(d => d.ID == id);
                    if (originalDonation == null)
                    {
                        return NotFound();
                    }

                    // Preserve fields that shouldn't be modified
                    donation.DonorID = originalDonation.DonorID;
                    donation.StaffMemberID = originalDonation.StaffMemberID;
                    donation.Created = originalDonation.Created;

                    _context.Update(donation);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Donation updated successfully!";

                    // Return to the appropriate donation history
                    if (congregantId.HasValue)
                    {
                        return RedirectToAction(nameof(CongregantDonationHistory), new { id = congregantId });
                    }
                    else if (nonCongregantId.HasValue)
                    {
                        return RedirectToAction(nameof(NonCongregantDonationHistory), new { id = nonCongregantId });
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DonationExists(donation.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Repopulate dropdown on error
            ViewBag.FundDesignations = new SelectList(
                _context.FundDesignations.Where(f => f.ActiveStatus),
                "ID",
                "Name",
                donation.FundDesignationID
            );

            ViewBag.CongregantID = congregantId;
            ViewBag.NonCongregantID = nonCongregantId;

            return View(donation);
        }

        /// <summary>
        /// Checks if a donation exists in the database.
        /// </summary>
        /// <param name="id">The ID of the donation to check.</param>
        /// <returns>True if the donation exists; otherwise, false.</returns>
        private bool DonationExists(int id)
        {
            return _context.Donations.Any(e => e.ID == id);
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

        /// <summary>
        /// Displays a list of all family groups.
        /// </summary>
        public async Task<IActionResult> ManageFamilies()
        {
            var families = await _context.Families
                .Include(f => f.Members)
                .OrderBy(f => f.FamilyName)
                .ToListAsync();

            return View(families);
        }

        /// <summary>
        /// Displays the form to create a new family group.
        /// </summary>
        public IActionResult AddFamily()
        {
            PopulateFamilyUnassignedCongregants();
            return View(new Family());
        }

        /// <summary>
        /// Handles POST to create a new family group and assign selected congregants.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFamily(Family family, int[] memberIds)
        {
            if (ModelState.IsValid)
            {
                _context.Families.Add(family);
                await _context.SaveChangesAsync();

                // Assign selected congregants to this family
                if (memberIds.Length > 0)
                {
                    var members = await _context.Congregants
                        .Where(c => memberIds.Contains(c.ID))
                        .ToListAsync();

                    foreach (var member in members)
                    {
                        member.FamilyID = family.ID;
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = $"Family '{family.FamilyName}' created successfully!";
                return RedirectToAction(nameof(ManageFamilies));
            }

            PopulateFamilyUnassignedCongregants();
            return View(family);
        }

        /// <summary>
        /// Displays the form to edit a family group and manage its members.
        /// </summary>
        public async Task<IActionResult> EditFamily(int? id)
        {
            if (id == null) return NotFound();

            var family = await _context.Families
                .Include(f => f.Members)
                .FirstOrDefaultAsync(f => f.ID == id);

            if (family == null) return NotFound();

            // Available: unassigned active members + members already in this family
            var available = await _context.Congregants
                .Where(c => c.ActiveStatus == ActiveStatus.CurrentMember &&
                            (c.FamilyID == null || c.FamilyID == id))
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.AvailableCongregants = available;
            return View(family);
        }

        /// <summary>
        /// Handles POST to update a family group name and member assignments.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditFamily(int id, Family family, int[] memberIds)
        {
            if (id != family.ID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Update family name
                    var existing = await _context.Families
                        .Include(f => f.Members)
                        .FirstOrDefaultAsync(f => f.ID == id);

                    if (existing == null) return NotFound();

                    existing.FamilyName = family.FamilyName;

                    // Remove all current members from family
                    foreach (var member in existing.Members)
                    {
                        member.FamilyID = null;
                    }

                    // Assign newly selected members
                    if (memberIds.Length > 0)
                    {
                        var newMembers = await _context.Congregants
                            .Where(c => memberIds.Contains(c.ID))
                            .ToListAsync();

                        foreach (var member in newMembers)
                        {
                            member.FamilyID = id;
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Family '{existing.FamilyName}' updated successfully!";
                    return RedirectToAction(nameof(ManageFamilies));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Families.Any(f => f.ID == id)) return NotFound();
                    throw;
                }
            }

            var availableOnError = await _context.Congregants
                .Where(c => c.ActiveStatus == ActiveStatus.CurrentMember &&
                            (c.FamilyID == null || c.FamilyID == id))
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.AvailableCongregants = availableOnError;
            return View(family);
        }

        /// <summary>
        /// Populates ViewBag with unassigned active congregants for family creation.
        /// </summary>
        private void PopulateFamilyUnassignedCongregants()
        {
            ViewBag.AvailableCongregants = _context.Congregants
                .Where(c => c.ActiveStatus == ActiveStatus.CurrentMember && c.FamilyID == null)
                .OrderBy(c => c.Name)
                .ToList();
        }

        // ─── Reports ────────────────────────────────

        /// <summary>
        ///  This action serves as the main entry point for the reports section, providing an overview and navigation to specific report types such as forecasting, congregant details, non-member details, and financial summaries.
        /// </summary>
        /// <returns>An IActionResult representing the reports view.</returns>
        public IActionResult Reports()
        {
            return View();
        }

        /// <summary>
        /// Generates a forecasting report of donation totals for the current and prior year, grouped by donor, and
        /// returns the results to the view.
        /// </summary>
        /// <remarks>The report includes year-to-date totals for the current year, year-to-date totals for
        /// the prior year up to the same day, and total donations for the prior year. Donors are grouped by name,
        /// including both congregants and non-congregants. The results are ordered by current year-to-date total in
        /// descending order.</remarks>
        /// <returns>An <see cref="IActionResult"/> that renders the forecasting report view with a list of donor summary rows.</returns>
        public async Task<IActionResult> ForecastingReport()
        {
            var today = DateTime.Today;
            var currentYear = today.Year;
            var priorYear = currentYear - 1;
            var priorYearSameDay = new DateTime(priorYear, today.Month, today.Day);

            var donations = await _context.Donations
                .Include(d => d.Congregant)
                .Include(d => d.NonCongregant)
                .Where(d => d.DonationDate.Year == currentYear || d.DonationDate.Year == priorYear)
                .ToListAsync();

            var rows = donations
                .GroupBy(d => d.CongregantID.HasValue
                    ? (d.Congregant?.Name ?? "Unknown")
                    : (string.IsNullOrWhiteSpace(d.NonCongregant?.FirstName) && string.IsNullOrWhiteSpace(d.NonCongregant?.LastName)
                        ? d.NonCongregant?.CompanyOrganization ?? "Unknown"
                        : $"{d.NonCongregant?.FirstName} {d.NonCongregant?.LastName}".Trim()))
                .Select(g => new DonorTrackingSystem.ViewModels.ForecastingReportRow
                {
                    DonorName = g.Key,
                    YtdTotal = g.Where(d => d.DonationDate.Year == currentYear).Sum(d => d.DonationAmount),
                    PriorYtdTotal = g.Where(d => d.DonationDate.Year == priorYear && d.DonationDate <= priorYearSameDay).Sum(d => d.DonationAmount),
                    PriorYearTotal = g.Where(d => d.DonationDate.Year == priorYear).Sum(d => d.DonationAmount)
                })
                .OrderByDescending(r => r.YtdTotal)
                .ToList();

            return View(rows);
        }

        /// <summary>
        /// Handles HTTP requests to display a report of all congregants, ordered by name.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/>
        /// that renders the congregant report view with the list of congregants.</returns>
        public async Task<IActionResult> CongregantReport()
        {
            var congregants = await _context.Congregants
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(congregants);
        }

        /// <summary>
        /// Generates a report of non-member donors, including their contact information and the date of their most
        /// recent donation.
        /// </summary>
        /// <remarks>The report includes all non-member donors, sorted by last name, first name, and
        /// organization. Each entry displays the donor's name, contact details, and the date of their last recorded
        /// donation, if any.</remarks>
        /// <returns>An <see cref="IActionResult"/> that renders a view displaying a list of non-member donors and their latest
        /// donation dates.</returns>
        public async Task<IActionResult> NonCongregantReport()
        {
            var nonCongregants = await _context.NonCongregants
                .OrderBy(n => n.LastName).ThenBy(n => n.FirstName).ThenBy(n => n.CompanyOrganization)
                .ToListAsync();

            var lastDonations = await _context.Donations
                .Where(d => d.NonCongregantID.HasValue)
                .GroupBy(d => d.NonCongregantID!.Value)
                .Select(g => new { NonCongregantID = g.Key, LastDate = g.Max(d => d.DonationDate) })
                .ToListAsync();

            var rows = nonCongregants.Select(n =>
            {
                var name = (!string.IsNullOrWhiteSpace(n.FirstName) || !string.IsNullOrWhiteSpace(n.LastName))
                    ? $"{n.FirstName} {n.LastName}".Trim()
                    : n.CompanyOrganization ?? "Unknown";
                var lastDonation = lastDonations.FirstOrDefault(d => d.NonCongregantID == n.ID)?.LastDate;
                return new DonorTrackingSystem.ViewModels.NonCongregantReportRow
                {
                    Name = name,
                    ContactInfo = n.ContactDetails,
                    LastDonationDate = lastDonation
                };
            }).ToList();

            return View(rows);
        }

        /// <summary>
        /// Generates a financial report comparing year-to-date donation totals for the current and prior year by month.
        /// </summary>
        /// <remarks>The report includes monthly and cumulative donation totals for both the current and
        /// previous calendar years. The resulting view model provides data suitable for visualizing trends or
        /// performing further analysis.</remarks>
        /// <returns>An <see cref="IActionResult"/> that renders the financial report view with year-over-year donation data.</returns>
        public async Task<IActionResult> FinancialReport()
        {
            var today = DateTime.Today;
            var currentYear = today.Year;
            var priorYear = currentYear - 1;
            var totalDiff = currentYear - priorYear;

            var donations = await _context.Donations
                .Where(d => d.DonationDate.Year == currentYear || d.DonationDate.Year == priorYear)
                .ToListAsync();

            var monthRows = Enumerable.Range(1, 12).Select(m =>
            {
                var currentYtd = donations
                    .Where(d => d.DonationDate.Year == currentYear && d.DonationDate.Month <= m)
                    .Sum(d => d.DonationAmount);
                var priorYtd = donations
                    .Where(d => d.DonationDate.Year == priorYear && d.DonationDate.Month <= m)
                    .Sum(d => d.DonationAmount);
                return new DonorTrackingSystem.ViewModels.FinancialReportRow
                {
                    Month = m,
                    MonthName = new DateTime(currentYear, m, 1).ToString("MMMM"),
                    CurrentYtd = currentYtd,
                    PriorYtd = priorYtd
                };
            }).ToList();

            var vm = new DonorTrackingSystem.ViewModels.FinancialReportViewModel
            {
                MonthlyRows = monthRows,
                CurrentYearTotal = donations.Where(d => d.DonationDate.Year == currentYear).Sum(d => d.DonationAmount),
                PriorYearTotal = donations.Where(d => d.DonationDate.Year == priorYear).Sum(d => d.DonationAmount),
                CurrentYear = currentYear,
                PriorYear = priorYear
                };

            return View(vm);
        }

        // CSV Exports

        /// <summary>
        /// Generates and returns a CSV file containing the forecasting report data for all donors.
        /// </summary>
        /// <remarks>The exported CSV includes columns for donor name, year-to-date total, prior
        /// year-to-date total, and prior year total. This method is typically used to allow users to download
        /// forecasting data for further analysis in spreadsheet applications.</remarks>
        /// <returns>A file result containing the forecasting report as a CSV file. The file is named "ForecastingReport.csv" and
        /// uses UTF-8 encoding.</returns>
        public async Task<IActionResult> ExportForecastingReport()
        {
            var rows = (await GetForecastingRows()).ToList();
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Donor Name,YTD Total,Prior YTD Total,Prior Year Total");
            foreach (var r in rows)
                csv.AppendLine($"\"{r.DonorName}\",{r.YtdTotal},{r.PriorYtdTotal},{r.PriorYearTotal}");
            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "ForecastingReport.csv");
        }

        /// <summary>
        /// Generates and returns a CSV file containing a report of all congregants, including their name, address,
        /// phone number, email address, birth date, and join date.
        /// </summary>
        /// <remarks>The report includes all congregants ordered by name. The CSV file is encoded in UTF-8
        /// and includes headers for each field. This action is intended for download scenarios and returns the file as
        /// an HTTP response with the appropriate content type.</remarks>
        /// <returns>An <see cref="IActionResult"/> that, when executed, sends a CSV file named "CongregantReport.csv" containing
        /// the congregant report data to the client.</returns>
        public async Task<IActionResult> ExportCongregantReport()
        {
            var congregants = await _context.Congregants.OrderBy(c => c.Name).ToListAsync();
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Name,Address,Phone,Email,Birth Date,Join Date");
            foreach (var c in congregants)
                csv.AppendLine($"\"{c.Name}\",\"{c.Address}\",\"{c.PhoneNumber}\",\"{c.EmailAddress}\",\"{c.BirthDate?.ToString("yyyy-MM-dd")}\",\"{c.JoinDate?.ToString("yyyy-MM-dd")}\"");
            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "CongregantReport.csv");
        }

        /// <summary>
        /// Generates and returns a CSV report containing non-member information and their most recent donation dates.
        /// </summary>
        /// <remarks>The report includes each non-member's name, contact information, and the date of
        /// their last recorded donation, if available. The CSV file is encoded in UTF-8 and named
        /// "NonMemberReport.csv".</remarks>
        /// <returns>A file result containing the CSV report of non-members, suitable for download by the client.</returns>
        public async Task<IActionResult> ExportNonCongregantReport()
        {
            var nonCongregants = await _context.NonCongregants
                .OrderBy(n => n.LastName).ThenBy(n => n.FirstName).ThenBy(n => n.CompanyOrganization)
                .ToListAsync();
            var lastDonations = await _context.Donations
                .Where(d => d.NonCongregantID.HasValue)
                .GroupBy(d => d.NonCongregantID!.Value)
                .Select(g => new { NonCongregantID = g.Key, LastDate = g.Max(d => d.DonationDate) })
                .ToListAsync();
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Name,Contact Info,Last Donation Date");
            foreach (var n in nonCongregants)
            {
                var name = (!string.IsNullOrWhiteSpace(n.FirstName) || !string.IsNullOrWhiteSpace(n.LastName))
                    ? $"{n.FirstName} {n.LastName}".Trim()
                    : n.CompanyOrganization ?? "Unknown";
                var lastDate = lastDonations.FirstOrDefault(d => d.NonCongregantID == n.ID)?.LastDate.ToString("yyyy-MM-dd");
                csv.AppendLine($"\"{name}\",\"{n.ContactDetails}\",\"{lastDate}\"");
            }
            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "Non-CongregantReport.csv");
        }

        /// <summary>
        /// Generates and returns a CSV file containing year-to-date donation totals by month for the current and prior
        /// year.
        /// </summary>
        /// <remarks>The generated CSV includes monthly and total donation amounts for both the current
        /// and previous calendar years. The report is intended for financial analysis and can be opened in spreadsheet
        /// applications.</remarks>
        /// <returns>An <see cref="IActionResult"/> that, when executed, sends a CSV file named "FinancialReport.csv" containing
        /// the financial report data for download.</returns>
        public async Task<IActionResult> ExportFinancialReport()
        {
            var today = DateTime.Today;
            var currentYear = today.Year;
            var priorYear = currentYear - 1;
            var totalDiff = currentYear - priorYear;
            var donations = await _context.Donations
                .Where(d => d.DonationDate.Year == currentYear || d.DonationDate.Year == priorYear)
                .ToListAsync();
            var csv = new System.Text.StringBuilder();
            csv.AppendLine($"Month,{currentYear} YTD,{priorYear} YTD");
            // Calculate YTD totals for each month for both years
            foreach (var m in Enumerable.Range(1, 12))
            {
                var monthName = new DateTime(currentYear, m, 1).ToString("MMMM");
                var cur = donations.Where(d => d.DonationDate.Year == currentYear && d.DonationDate.Month <= m).Sum(d => d.DonationAmount);
                var pri = donations.Where(d => d.DonationDate.Year == priorYear && d.DonationDate.Month <= m).Sum(d => d.DonationAmount);
                csv.AppendLine($"{monthName},{cur},{pri}");
            }
            // Add totals row
            var curTotal = donations.Where(d => d.DonationDate.Year == currentYear).Sum(d => d.DonationAmount);
            var priTotal = donations.Where(d => d.DonationDate.Year == priorYear).Sum(d => d.DonationAmount);
            csv.AppendLine($"TOTAL,{curTotal},{priTotal}");
            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "FinancialReport.csv");
        }

        /// <summary>
        /// Retrieves forecasting report rows containing year-to-date and prior year donation totals for each donor.
        /// </summary>
        /// <remarks>Each report row includes the donor's name, the total donations for the current year
        /// to date, the total donations for the same period in the prior year, and the total donations for the entire
        /// prior year. Donors are grouped by name, and the results are ordered by current year-to-date total in
        /// descending order.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task result contains a collection of forecasting
        /// report rows, each summarizing donation totals for a donor in the current and prior year.</returns>
        private async Task<IEnumerable<DonorTrackingSystem.ViewModels.ForecastingReportRow>> GetForecastingRows()
        {
            var today = DateTime.Today;
            var currentYear = today.Year;
            var priorYear = currentYear - 1;
            var priorYearSameDay = new DateTime(priorYear, today.Month, today.Day);
            var donations = await _context.Donations
                .Include(d => d.Congregant)
                .Include(d => d.NonCongregant)
                .Where(d => d.DonationDate.Year == currentYear || d.DonationDate.Year == priorYear)
                .ToListAsync();
            return donations
                .GroupBy(d => d.CongregantID.HasValue
                    ? (d.Congregant?.Name ?? "Unknown")
                    : (string.IsNullOrWhiteSpace(d.NonCongregant?.FirstName) && string.IsNullOrWhiteSpace(d.NonCongregant?.LastName)
                        ? d.NonCongregant?.CompanyOrganization ?? "Unknown"
                        : $"{d.NonCongregant?.FirstName} {d.NonCongregant?.LastName}".Trim()))
                .Select(g => new DonorTrackingSystem.ViewModels.ForecastingReportRow
                {
                    DonorName = g.Key,
                    YtdTotal = g.Where(d => d.DonationDate.Year == currentYear).Sum(d => d.DonationAmount),
                    PriorYtdTotal = g.Where(d => d.DonationDate.Year == priorYear && d.DonationDate <= priorYearSameDay).Sum(d => d.DonationAmount),
                    PriorYearTotal = g.Where(d => d.DonationDate.Year == priorYear).Sum(d => d.DonationAmount)
                })
                .OrderByDescending(r => r.YtdTotal);
        }

        // Tax Letters 

        /// <summary>
        /// This action serves as the main entry point for the tax letters section, providing an overview and navigation to specific tax letter types such as family tax letters, non-congregant tax letters, and individual tax letters for both congregants and non-congregants.
        /// </summary>
        /// <returns></returns>
        public IActionResult TaxLetters()
        {
            ViewBag.CurrentYear = DateTime.Today.Year;
            return View();
        }

        /// <summary>
        /// Generates and displays tax letter data for each family for the specified tax year.
        /// </summary>
        /// <remarks>Only families with at least one donation in the specified year are included. The
        /// first member's address is used as the recipient address for each family.</remarks>
        /// <param name="year">The tax year for which to generate family tax letters. If null, the current year is used.</param>
        /// <returns>A view displaying a list of family tax letter view models for the specified year.</returns>
        public async Task<IActionResult> FamilyTaxLetters(int? year)
        {
            int taxYear = year ?? DateTime.Today.Year;

            var families = await _context.Families
                .Include(f => f.Members)
                .OrderBy(f => f.FamilyName)
                .ToListAsync();

            var donations = await _context.Donations
                .Include(d => d.FundDesignation)
                .Include(d => d.Congregant)
                .Where(d => d.DonationDate.Year == taxYear && d.CongregantID.HasValue)
                .ToListAsync();

            var letters = new List<DonorTrackingSystem.ViewModels.FamilyTaxLetterViewModel>();

            // Group donations by family based on congregant membership and create a tax letter for each family with donations in the specified year
            foreach (var family in families)
            {
                var memberIds = family.Members.Select(m => m.ID).ToList();
                var familyDonations = donations
                    .Where(d => d.CongregantID.HasValue && memberIds.Contains(d.CongregantID.Value))
                    .OrderBy(d => d.DonationDate)
                    .ToList();

                if (!familyDonations.Any()) continue;

                // Use the first member's address for the letter
                var primaryMember = family.Members.FirstOrDefault();

                // Create a tax letter view model for the family
                letters.Add(new DonorTrackingSystem.ViewModels.FamilyTaxLetterViewModel
                {
                    FamilyName = family.FamilyName,
                    RecipientName = family.FamilyName,
                    RecipientAddress = primaryMember?.Address,
                    MemberNames = family.Members.Select(m => m.Name).ToList(),
                    Year = taxYear,
                    Donations = familyDonations.Select(d => new DonorTrackingSystem.ViewModels.TaxLetterDonationLine
                    {
                        Date = d.DonationDate,
                        FundName = d.FundDesignation?.Name ?? "General Fund",
                        Amount = d.DonationAmount
                    }).ToList()
                });
            }

            ViewBag.Year = taxYear;
            return View(letters);
        }

        /// <summary>
        /// Generates and displays tax letters for all active non-congregant donors for the specified tax year.
        /// </summary>
        /// <remarks>Only non-congregant donors with at least one donation in the specified year are
        /// included. The resulting view can be used to review or print tax letters for these donors.</remarks>
        /// <param name="year">The tax year for which to generate tax letters. If null, the current year is used.</param>
        /// <returns>An IActionResult that renders a view containing a list of tax letter view models for non-congregant donors
        /// with donations in the specified year.</returns>
        public async Task<IActionResult> NonCongregantTaxLetters(int? year)
        {
            int taxYear = year ?? DateTime.Today.Year;

            // Only include active non-congregants in the report
            var nonCongregants = await _context.NonCongregants
                .Where(n => n.IsActive)
                .OrderBy(n => n.LastName).ThenBy(n => n.FirstName).ThenBy(n => n.CompanyOrganization)
                .ToListAsync();

            // Get all donations for the specified year that are associated with non-congregants
            var donations = await _context.Donations
                .Include(d => d.FundDesignation)
                .Where(d => d.DonationDate.Year == taxYear && d.NonCongregantID.HasValue)
                .ToListAsync();

            var letters = new List<DonorTrackingSystem.ViewModels.TaxLetterViewModel>();

            // Group donations by non-congregant and create a tax letter for each with donations in the specified year
            foreach (var nc in nonCongregants)
            {
                var ncDonations = donations
                    .Where(d => d.NonCongregantID == nc.ID)
                    .OrderBy(d => d.DonationDate)
                    .ToList();

                if (!ncDonations.Any()) continue;

                // Determine the recipient name based on available information
                var name = (!string.IsNullOrWhiteSpace(nc.FirstName) || !string.IsNullOrWhiteSpace(nc.LastName))
                    ? $"{nc.FirstName} {nc.LastName}".Trim()
                    : nc.CompanyOrganization ?? "Unknown";

                // Create a tax letter view model for the non-congregant donor
                letters.Add(new DonorTrackingSystem.ViewModels.TaxLetterViewModel
                {
                    RecipientName = name,
                    RecipientAddress = nc.ContactDetails,
                    Year = taxYear,
                    Donations = ncDonations.Select(d => new DonorTrackingSystem.ViewModels.TaxLetterDonationLine
                    {
                        Date = d.DonationDate,
                        FundName = d.FundDesignation?.Name ?? "General Fund",
                        Amount = d.DonationAmount
                    }).ToList()
                });
            }

            ViewBag.Year = taxYear;
            return View(letters);
        }

        /// <summary>
        /// Displays a view for selecting an individual tax letter for a specified year.
        /// </summary>
        /// <remarks>The view includes lists of congregants and non-congregants, ordered by name, for the
        /// specified year.</remarks>
        /// <param name="year">The tax year to display. If null, the current year is used.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the view for selecting an
        /// individual tax letter.</returns>
        public async Task<IActionResult> SelectIndividualTaxLetter(int? year)
        {
            int taxYear = year ?? DateTime.Today.Year;

            var congregants = await _context.Congregants
                .OrderBy(c => c.Name)
                .ToListAsync();

            var nonCongregants = await _context.NonCongregants
                .OrderBy(n => n.LastName).ThenBy(n => n.FirstName).ThenBy(n => n.CompanyOrganization)
                .ToListAsync();

            ViewBag.Year = taxYear;
            ViewBag.Congregants = congregants;
            ViewBag.NonCongregants = nonCongregants;
            return View();
        }

        /// <summary>
        /// Generates and returns a tax letter view for an individual congregant for a specified tax year.
        /// </summary>
        /// <remarks>The returned view includes all donations made by the congregant during the specified
        /// year, grouped by fund designation. If no year is provided, the current calendar year is used.</remarks>
        /// <param name="id">The unique identifier of the congregant for whom the tax letter is generated.</param>
        /// <param name="year">The tax year for which to generate the letter. If null, the current year is used.</param>
        /// <returns>An IActionResult that renders the tax letter view for the specified congregant and year. Returns a NotFound
        /// result if the congregant does not exist.</returns>
        public async Task<IActionResult> IndividualCongregantTaxLetter(int id, int? year)
        {
            int taxYear = year ?? DateTime.Today.Year;

            var congregant = await _context.Congregants.FindAsync(id);
            if (congregant == null) return NotFound();

            var donations = await _context.Donations
                .Include(d => d.FundDesignation)
                .Where(d => d.CongregantID == id && d.DonationDate.Year == taxYear)
                .OrderBy(d => d.DonationDate)
                .ToListAsync();

            // Create a tax letter view model for the congregant
            var letter = new DonorTrackingSystem.ViewModels.TaxLetterViewModel
            {
                RecipientName = congregant.Name,
                RecipientAddress = congregant.Address,
                Year = taxYear,
                Donations = donations.Select(d => new DonorTrackingSystem.ViewModels.TaxLetterDonationLine
                {
                    Date = d.DonationDate,
                    FundName = d.FundDesignation?.Name ?? "General Fund",
                    Amount = d.DonationAmount
                }).ToList()
            };

            return View("IndividualTaxLetter", letter);
        }

        /// <summary>
        /// Generates a tax letter view for an individual non-congregant donor for a specified tax year.
        /// </summary>
        /// <remarks>The returned view includes all donations made by the specified non-congregant during
        /// the given tax year, grouped by fund designation. If the donor's name is not available, the organization name
        /// or 'Unknown' is used as the recipient.</remarks>
        /// <param name="id">The unique identifier of the non-congregant donor whose tax letter is to be generated.</param>
        /// <param name="year">The tax year for which to generate the letter. If null, the current year is used.</param>
        /// <returns>A view displaying the tax letter for the specified non-congregant and year. Returns a NotFound result if the
        /// donor does not exist.</returns>
        public async Task<IActionResult> IndividualNonCongregantTaxLetter(int id, int? year)
        {
            int taxYear = year ?? DateTime.Today.Year;

            var nc = await _context.NonCongregants.FindAsync(id);
            if (nc == null) return NotFound();

            // Get all donations for the specified year associated with this non-congregant donor
            var donations = await _context.Donations
                .Include(d => d.FundDesignation)
                .Where(d => d.NonCongregantID == id && d.DonationDate.Year == taxYear)
                .OrderBy(d => d.DonationDate)
                .ToListAsync();
            // Determine the recipient name based on available information
            var name = (!string.IsNullOrWhiteSpace(nc.FirstName) || !string.IsNullOrWhiteSpace(nc.LastName))
                ? $"{nc.FirstName} {nc.LastName}".Trim()
                : nc.CompanyOrganization ?? "Unknown";

            // Create a tax letter view model for the non-congregant donor
            var letter = new DonorTrackingSystem.ViewModels.TaxLetterViewModel
            {
                RecipientName = name,
                RecipientAddress = nc.ContactDetails,
                Year = taxYear,
                Donations = donations.Select(d => new DonorTrackingSystem.ViewModels.TaxLetterDonationLine
                {
                    Date = d.DonationDate,
                    FundName = d.FundDesignation?.Name ?? "General Fund",
                    Amount = d.DonationAmount
                }).ToList()
            };

            return View("IndividualTaxLetter", letter);
        }

    }
}
