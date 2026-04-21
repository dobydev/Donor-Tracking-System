using DonorTrackingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        /// <returns>A view with the donation's current information for editing, or NotFound if the donation doesn't exist.</returns>
        public async Task<IActionResult> EditDonation(int? id, int? congregantId)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donation = await _context.Donations
                .Include(d => d.FundDesignation)
                .Include(d => d.Congregant)
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

            // Store congregant ID for return navigation
            ViewBag.CongregantID = congregantId ?? donation.CongregantID;
            ViewBag.CongregantName = donation.Congregant?.Name ?? "Unknown";

            return View(donation);
        }

        /// <summary>
        /// Handles the POST request to update a donation's information in the database.
        /// </summary>
        /// <param name="id">The ID of the donation to update.</param>
        /// <param name="donation">The updated donation information.</param>
        /// <param name="congregantId">The ID of the congregant to return to after editing.</param>
        /// <returns>A redirect to the congregant's donation history if the update is successful; otherwise, returns the view with validation errors.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDonation(int id, Donation donation, int? congregantId)
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

                    // Return to the congregant's donation history if congregantId is provided
                    if (congregantId.HasValue)
                    {
                        return RedirectToAction(nameof(CongregantDonationHistory), new { id = congregantId });
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
        public async Task<IActionResult> NonMemberReport()
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
                return new DonorTrackingSystem.ViewModels.NonMemberReportRow
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

        // ─── CSV Exports ──────────────────────────────────────────────────────────

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
            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "NonMemberReport.csv");
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
            var donations = await _context.Donations
                .Where(d => d.DonationDate.Year == currentYear || d.DonationDate.Year == priorYear)
                .ToListAsync();
            var csv = new System.Text.StringBuilder();
            csv.AppendLine($"Month,{currentYear} YTD,{priorYear} YTD");
            foreach (var m in Enumerable.Range(1, 12))
            {
                var monthName = new DateTime(currentYear, m, 1).ToString("MMMM");
                var cur = donations.Where(d => d.DonationDate.Year == currentYear && d.DonationDate.Month <= m).Sum(d => d.DonationAmount);
                var pri = donations.Where(d => d.DonationDate.Year == priorYear && d.DonationDate.Month <= m).Sum(d => d.DonationAmount);
                csv.AppendLine($"{monthName},{cur},{pri}");
            }
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

    }
}
