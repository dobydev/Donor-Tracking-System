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

    }
}
