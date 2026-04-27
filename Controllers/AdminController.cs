using DonorTrackingSystem.Models;
using DonorTrackingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DonorTrackingSystem.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : Controller
    {
        // Dependencies for database access and user/role management
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        // Define staff roles as a constant array for easy maintenance and validation
        private static readonly string[] StaffRoles = { "Administrator", "Office Manager", "Support Staff" };

        public AdminController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
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

        // ── Merge Congregants 

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

        // ── Merge Non-Congregants

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

        // ── Staff Management ──────────────────────────────────────────────────

        /// <summary>
        /// Lists all staff accounts with their assigned roles and active status.
        /// </summary>
        public async Task<IActionResult> ManageStaff()
        {
            var users = _userManager.Users.ToList();
            var staff = new List<StaffListViewModel>();

            foreach (var user in users.OrderBy(u => u.UserName))
            {
                var roles = await _userManager.GetRolesAsync(user);
                staff.Add(new StaffListViewModel
                {
                    Id       = user.Id,
                    StaffId  = user.UserName ?? string.Empty,
                    Role     = roles.FirstOrDefault() ?? "No Role",
                    IsActive = user.IsActive
                });
            }

            return View(staff);
        }

        /// <summary>
        /// Returns the form for adding a new staff account.
        /// </summary>
        public IActionResult AddStaff()
        {
            ViewBag.Roles = StaffRoles;
            return View(new AddStaffViewModel());
        }

        /// <summary>
        /// Handles POST to create a new staff account with the specified role.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStaff(AddStaffViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = StaffRoles;
                return View(vm);
            }

            // Staff ID must be unique
            if (await _userManager.FindByNameAsync(vm.StaffId) != null)
            {
                ModelState.AddModelError(nameof(vm.StaffId), "A staff account with this ID already exists.");
                ViewBag.Roles = StaffRoles;
                return View(vm);
            }

            if (!StaffRoles.Contains(vm.Role))
            {
                ModelState.AddModelError(nameof(vm.Role), "Invalid role selected.");
                ViewBag.Roles = StaffRoles;
                return View(vm);
            }

            var user = new ApplicationUser { UserName = vm.StaffId, IsActive = true };
            var result = await _userManager.CreateAsync(user, vm.Password);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                ViewBag.Roles = StaffRoles;
                return View(vm);
            }

            await _userManager.AddToRoleAsync(user, vm.Role);

            TempData["SuccessMessage"] = $"Staff account '{vm.StaffId}' created successfully with the '{vm.Role}' role.";
            return RedirectToAction(nameof(ManageStaff));
        }

        /// <summary>
        /// Returns the edit form for an existing staff account.
        /// </summary>
        public async Task<IActionResult> EditStaff(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Roles = StaffRoles;

            return View(new EditStaffViewModel
            {
                Id       = user.Id,
                StaffId  = user.UserName ?? string.Empty,
                Role     = roles.FirstOrDefault() ?? string.Empty,
                IsActive = user.IsActive
            });
        }

        /// <summary>
        /// Handles POST to update role, active status, and optionally reset the password.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStaff(EditStaffViewModel vm)
        {
            // Remove NewPassword validation error if left blank (it's optional)
            if (string.IsNullOrWhiteSpace(vm.NewPassword))
                ModelState.Remove(nameof(vm.NewPassword));

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = StaffRoles;
                return View(vm);
            }

            var user = await _userManager.FindByIdAsync(vm.Id);
            if (user == null) return NotFound();

            if (!StaffRoles.Contains(vm.Role))
            {
                ModelState.AddModelError(nameof(vm.Role), "Invalid role selected.");
                ViewBag.Roles = StaffRoles;
                return View(vm);
            }

            // Update active status
            user.IsActive = vm.IsActive;
            await _userManager.UpdateAsync(user);

            // Update role: remove existing, assign new
            var existingRoles = await _userManager.GetRolesAsync(user);
            if (existingRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, existingRoles);
            await _userManager.AddToRoleAsync(user, vm.Role);

            // Reset password if provided
            if (!string.IsNullOrWhiteSpace(vm.NewPassword))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var pwResult = await _userManager.ResetPasswordAsync(user, token, vm.NewPassword);
                if (!pwResult.Succeeded)
                {
                    foreach (var e in pwResult.Errors)
                        ModelState.AddModelError(string.Empty, e.Description);
                    ViewBag.Roles = StaffRoles;
                    return View(vm);
                }
            }

            TempData["SuccessMessage"] = $"Staff account '{user.UserName}' updated successfully.";
            return RedirectToAction(nameof(ManageStaff));
        }

        /// <summary>
        /// Handles POST to permanently delete a staff account.
        /// Prevents deleting the currently logged-in administrator.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteStaff(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Prevent self-deletion
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser?.Id == id)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(ManageStaff));
            }

            var staffId = user.UserName;
            await _userManager.DeleteAsync(user);

            TempData["SuccessMessage"] = $"Staff account '{staffId}' has been deleted.";
            return RedirectToAction(nameof(ManageStaff));
        }
    }
}
