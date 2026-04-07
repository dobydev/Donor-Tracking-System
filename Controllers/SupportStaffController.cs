using DonorTrackingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DonorTrackingSystem.Controllers
{
    // This controller is responsible for handling all actions related to support staff functionalities, such as recording donations, managing congregants and non-congregant donors, and generating reports. Access to this controller is restricted to users with the "Support Staff" role.
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

        /// <summary>
        /// This action method retrieves the most recent donations from the database, including related data such as fund designations, congregants, and non-congregant donors. The donations are ordered by date in descending order and limited to the 10 most recent entries. The retrieved data is then passed to the view for display on the support staff dashboard.
        /// </summary>
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

        /// <summary>
        /// Displays the donation entry form with populated dropdown lists for fund designations, congregants, and non-congregant donors.
        /// </summary>
        /// <remarks>The returned view includes dropdowns for active fund designations, current
        /// congregants, and non-congregant donors, including an option for anonymous donations. This method is used to initiate the donation recording process in the application.</remarks>
        /// <returns>A view that allows users to record a new donation, pre-populated with relevant selection options.</returns>
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

        /// <summary>
        /// Processes and records a new donation, assigning donor and staff member information, and persists the sdonation to the database.
        /// </summary>
        /// <remarks>This action automatically assigns a unique DonorID starting from 1000 and associates
        /// the donation with the currently authenticated staff member. If model validation fails, the method repopulates dropdown lists to preserve user input. Only accessible via HTTP POST and requires a valid anti-forgery token.</remarks>
        /// <param name="donation">The donation to be recorded. Must contain valid donation details. If the NonCongregantID is -1, the donation will be recorded as anonymous.</param>
        /// <returns>A redirect to the index view if the donation is successfully recorded; otherwise, returns the view with
        /// validation errors and repopulated form data.</returns>
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

        /// <summary>
        /// Displays a view listing all donations, ordered by most recent donation date.
        /// </summary>
        /// <remarks>The returned view includes related fund designation information for each donation.
        /// The list is sorted in descending order by donation date.</remarks>
        /// <returns>An <see cref="IActionResult"/> that renders the donations view with a list of donation records.</returns>
        public async Task<IActionResult> ViewDonations()
        {
            var donations = await _context.Donations
                .Include(d => d.FundDesignation)
                .OrderByDescending(d => d.DonationDate)
                .ToListAsync();

            return View(donations);
        }

        // Will be used to manage congregants and non-congregant donors, including adding new entries and viewing existing ones.
        public IActionResult AddCongregant()
        {
            return View();
        }

        // Will be implemented next sprint to handle the form submission for adding a new congregant, including validation and saving to the database.
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

        // Will be implemented next sprint to display a list of all congregants, ordered alphabetically by name, with options to edit or view details for each congregant.
        public async Task<IActionResult> ViewCongregants()
        {
            var congregants = await _context.Congregants
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View(congregants);
        }

       /// <summary>
       /// Returns a view that allows the user to add a non-congregant record.
       /// </summary>
       /// <returns>A view result that renders the form for adding a non-congregant.</returns>
        public IActionResult AddNonCongregant()
        {
            return View();
        }

        /// <summary>
        /// Handles the HTTP POST request to add a new non-congregant donor to the system.
        /// </summary>
        /// <remarks>This action requires a valid anti-forgery token. If model validation fails, the user
        /// is presented with the form to correct input errors.</remarks>
        /// <param name="nonCongregant">The non-congregant donor information to add. Must contain valid data as required by the model.</param>
        /// <returns>A redirect to the list of non-congregant donors if the addition is successful; otherwise, returns the view
        /// with validation errors.</returns>
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

        /// <summary>
        /// Retrieves a list of non-congregant individuals and displays them in a view, ordered by last name, first
        /// name, and company or organization.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/>
        /// that renders the view displaying the ordered list of non-congregants.</returns>
        public async Task<IActionResult> ViewNonCongregants()
        {
            var nonCongregants = await _context.NonCongregants
                .OrderBy(n => n.LastName)
                .ThenBy(n => n.FirstName)
                .ThenBy(n => n.CompanyOrganization)
                .ToListAsync();

            return View(nonCongregants);
        }

        
        /// <summary>
        /// Generates a daily report of donations for the current day, including summary statistics and a list of today's donations.
        /// </summary>
        /// <remarks>The report includes the total number of donations, the total donation amount, counts
        /// of envelope and non-envelope donations, and the report date. The donations list includes related fund designation and donor information. This action is used to display a summary of daily giving activity in an report-like interface.</remarks>
        /// <returns>An <see cref="IActionResult"/> that renders the daily donations report view with the relevant data for the
        /// current day.</returns>
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
