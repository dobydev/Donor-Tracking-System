using DonorTrackingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DonorTrackingSystem.Controllers
{
    [Authorize(Roles = "Support Staff")]
    public class SupportStaffController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SupportStaffController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Support Staff Dashboard
        public async Task<IActionResult> Index()
        {
            // Get recent donations with all related data
            var recentDonations = await _context.Donations
                .Include(d => d.FundDesignation)
                .Include(d => d.Congregant)
                .Include(d => d.NonMember)
                .OrderByDescending(d => d.DonationDate)
                .Take(10)
                .ToListAsync();

            return View(recentDonations);
        }

        // GET: Record Donation
        public IActionResult RecordDonation()
        {
            // Populate fund designations dropdown
            ViewBag.FundDesignations = new SelectList(
                _context.FundDesignations.Where(f => f.ActiveStatus), 
                "ID", 
                "Name"
            );

            // Populate congregants dropdown (optional)
            var congregants = _context.Congregants
                .Where(c => c.ActiveStatus == ActiveStatus.CurrentMember)
                .Select(c => new
                {
                    c.ID,
                    DisplayName = c.Name
                })
                .OrderBy(c => c.DisplayName)
                .ToList();
            ViewBag.Congregants = new SelectList(congregants, "ID", "DisplayName");

            // Populate non-member donors dropdown (optional)
            var nonMembers = _context.NonMembers
                .Select(n => new
                {
                    n.ID,
                    DisplayName = !string.IsNullOrWhiteSpace(n.FirstName) || !string.IsNullOrWhiteSpace(n.LastName)
                        ? n.FirstName + " " + n.LastName
                        : n.CompanyOrganization
                })
                .OrderBy(n => n.DisplayName)
                .ToList();
            ViewBag.NonMembers = new SelectList(nonMembers, "ID", "DisplayName");

            var donation = new Donation
            {
                DonationDate = DateTime.Now
            };

            return View(donation);
        }

        // POST: Record Donation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordDonation(Donation donation)
        {
            if (ModelState.IsValid)
            {
                // Auto-assign Donor ID starting from 1000
                var maxDonorId = await _context.Donations
                    .MaxAsync(d => (int?)d.DonorID) ?? 999; // Start at 999 so first ID will be 1000
                donation.DonorID = maxDonorId + 1;

                // Get current user's ID (staff member)
                var user = await _userManager.GetUserAsync(User);
                if (user != null && int.TryParse(user.UserName, out int staffId))
                {
                    donation.StaffMemberID = staffId;
                }

                _context.Donations.Add(donation);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Donation recorded successfully! Donor ID: {donation.DonorID}";
                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdown on error
            ViewBag.FundDesignations = new SelectList(
                _context.FundDesignations.Where(f => f.ActiveStatus), 
                "ID", 
                "Name"
            );

            // Repopulate congregants dropdown
            var congregants = _context.Congregants
                .Where(c => c.ActiveStatus == ActiveStatus.CurrentMember)
                .Select(c => new
                {
                    c.ID,
                    DisplayName = c.Name
                })
                .OrderBy(c => c.DisplayName)
                .ToList();
            ViewBag.Congregants = new SelectList(congregants, "ID", "DisplayName");

            // Repopulate non-member donors dropdown
            var nonMembers = _context.NonMembers
                .Select(n => new
                {
                    n.ID,
                    DisplayName = !string.IsNullOrWhiteSpace(n.FirstName) || !string.IsNullOrWhiteSpace(n.LastName)
                        ? n.FirstName + " " + n.LastName
                        : n.CompanyOrganization
                })
                .OrderBy(n => n.DisplayName)
                .ToList();
            ViewBag.NonMembers = new SelectList(nonMembers, "ID", "DisplayName");

            return View(donation);
        }

        // GET: View Donations
        public async Task<IActionResult> ViewDonations()
        {
            var donations = await _context.Donations
                .Include(d => d.FundDesignation)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();

            return View(donations);
        }

        // GET: Add Non-Member Donor
        public IActionResult AddNonMember()
        {
            return View();
        }

        // POST: Add Non-Member Donor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNonMember(NonMember nonMember)
        {
            if (ModelState.IsValid)
            {
                _context.NonMembers.Add(nonMember);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Non-member donor added successfully! Donor ID: {nonMember.ID}";
                return RedirectToAction(nameof(ViewNonMembers));
            }

            return View(nonMember);
        }

        // GET: View Non-Members
        public async Task<IActionResult> ViewNonMembers()
        {
            var nonMembers = await _context.NonMembers
                .OrderBy(n => n.LastName)
                .ThenBy(n => n.FirstName)
                .ThenBy(n => n.CompanyOrganization)
                .ToListAsync();

            return View(nonMembers);
        }

        // GET: Daily Donation Report
        public async Task<IActionResult> DailyReport()
        {
            // Get today's date (start and end of day)
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // Get all donations for today, including related data
            var todaysDonations = await _context.Donations
                .Include(d => d.FundDesignation)
                .Include(d => d.Congregant)
                .Include(d => d.NonMember)
                .Where(d => d.DonationDate >= today && d.DonationDate < tomorrow)
                .OrderByDescending(d => d.DonationDate)
                .ThenBy(d => d.ID)
                .ToListAsync();

            // Calculate summary statistics
            ViewBag.TotalDonations = todaysDonations.Count;
            ViewBag.TotalAmount = todaysDonations.Sum(d => d.DonationAmount);
            ViewBag.EnvelopeDonations = todaysDonations.Count(d => !string.IsNullOrWhiteSpace(d.EnvelopeNumber));
            ViewBag.NonEnvelopeDonations = todaysDonations.Count(d => string.IsNullOrWhiteSpace(d.EnvelopeNumber));
            ViewBag.ReportDate = today;

            return View(todaysDonations);
        }
    }
}
