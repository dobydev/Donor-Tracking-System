using DonorTrackingSystem.Models;
using DonorTrackingSystem.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DonorTrackingSystem.Controllers
{
    public class AccountController : Controller
    {

        // Dependency Injection
        private ApplicationDbContext db;
        private UserManager<ApplicationUser> userManager;
        private SignInManager<ApplicationUser> signInManager;
        private RoleManager<IdentityRole> roleManager;

        // Constructor to inject services
        public AccountController(UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager, ApplicationDbContext db)

        {
            // Assign injected services to private fields
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.db = db;
        }

        /// <summary>
        /// Shows the login form.
        /// </summary>
        public IActionResult Login()

        {
            return View();
        }

        /// <summary>
        /// Signs in the user and redirects based on their role(s).
        /// </summary>
        /// <param name="vm">Login form values.</param>
        /// <returns>Role-based redirect on success; redisplays form on failure.</returns>
        [HttpPost]
        public async Task<IActionResult> Login(AccountLoginViewModel vm)
        {
            // Check if the model state is valid
            if (ModelState.IsValid)
            {
                // Identity: attempt password sign-in using ID as username
                var result = await signInManager.PasswordSignInAsync(vm.ID.ToString(), vm.Password.ToString(), false, false);

                // If sign-in is successful, redirect to home page
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                // If sign-in fails, add specific and user-friendly error message
                else
                {
                    ModelState.AddModelError("", "Invalid Login ID or Password. Please try again.");
                }
            }
            return View(vm);
        }

        /// <summary>
        /// Logs out the current user and redirects to login.
        /// </summary>
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        // Placeholder for the default index action, can be customized as needed
        public IActionResult Index()
        {
            return View();
        }
    }
}
