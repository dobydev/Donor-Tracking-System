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
                .Include(d => d.NonCongregant)
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

            // Populate non-congregant donors dropdown (optional)
            var nonCongregants = _context.NonCongregants
                .Select(n => new
                {
                    n.ID,
                    DisplayName = !string.IsNullOrWhiteSpace(n.FirstName) || !string.IsNullOrWhiteSpace(n.LastName)
                        ? n.FirstName + " " + n.LastName
                        : n.CompanyOrganization
                })
                .OrderBy(n => n.DisplayName)
                .ToList();

            // Add "Anonymous" option to the beginning of the list
            var nonMembersWithAnonymous = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Select Non-Congregant (Optional) --" },
                new SelectListItem { Value = "-1", Text = "Anonymous" }
            };
            nonMembersWithAnonymous.AddRange(nonCongregants.Select(n => new SelectListItem 
            { 
                Value = n.ID.ToString(), 
                Text = n.DisplayName 
            }));
            ViewBag.NonCongregants = nonMembersWithAnonymous;

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
                // Handle anonymous donations (special value -1)
                if (donation.NonCongregantID == -1)
                {
                    donation.NonCongregantID = null;
                }

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

                // Set creation timestamp
                donation.Created = DateTimeOffset.Now;

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

            // Repopulate non-congregant donors dropdown
            var nonCongregants = _context.NonCongregants
                .Select(n => new
                {
                    n.ID,
                    DisplayName = !string.IsNullOrWhiteSpace(n.FirstName) || !string.IsNullOrWhiteSpace(n.LastName)
                        ? n.FirstName + " " + n.LastName
                        : n.CompanyOrganization
                })
                .OrderBy(n => n.DisplayName)
                .ToList();

            // Add "Anonymous" option to the beginning of the list
            var nonMembersWithAnonymous = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "-- Select Non-Member (Optional) --" },
                new SelectListItem { Value = "-1", Text = "Anonymous" }
            };
            nonMembersWithAnonymous.AddRange(nonCongregants.Select(n => new SelectListItem 
            { 
                Value = n.ID.ToString(), 
                Text = n.DisplayName 
            }));
            ViewBag.NonCongregants = nonMembersWithAnonymous;

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

        // GET: Add Congregant
        public IActionResult AddCongregant()
        {
            return View();
        }

        // POST: Add Congregant
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCongregant(Congregant congregant)
        {
            if (ModelState.IsValid)
            {
                _context.Congregants.Add(congregant);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Congregant added successfully! ID: {congregant.ID} - {congregant.Name}";
                return RedirectToAction(nameof(ViewCongregants));
            }

            return View(congregant);
        }

        // GET: View Congregants
        public async Task<IActionResult> ViewCongregants()
        {
            var congregants = await _context.Congregants
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(congregants);
        }

        // GET: Add Non-Member Donor
        public IActionResult AddNonCongregant()
        {
            return View();
        }

        // POST: Add Non-Member Donor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNonCongregant(NonCongregant nonCongregant)
        {
            if (ModelState.IsValid)
            {
                _context.NonCongregants.Add(nonCongregant);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Non-congregant donor added successfully! Donor ID: {nonCongregant.ID}";
                return RedirectToAction(nameof(ViewNonCongregants));
            }

            return View(nonCongregant);
        }

        // GET: View Non-Congregants
        public async Task<IActionResult> ViewNonCongregants()
        {
            var nonCongregants = await _context.NonCongregants
                .OrderBy(n => n.LastName)
                .ThenBy(n => n.FirstName)
                .ThenBy(n => n.CompanyOrganization)
                .ToListAsync();

            return View(nonCongregants);
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
                .Include(d => d.NonCongregant)
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
