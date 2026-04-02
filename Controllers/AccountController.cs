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
                var result = await signInManager.PasswordSignInAsync(vm.ID.ToString(), vm.Password, false, false);

                // If sign-in is successful, redirect to the appropriate page based on user role
                if (result.Succeeded)
                {
                    // Identity: get user and their roles
                    var user = await userManager.FindByNameAsync(vm.ID.ToString());
                    if (user != null)
                    {
                        var roles = await userManager.GetRolesAsync(user);

                        // Redirect based on user role
                        if (roles.Contains("Admin"))
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        else if (roles.Contains("Donor"))
                        {
                            return RedirectToAction("Index", "Donor");
                        }
                        else if (roles.Contains("Staff"))
                        {
                            return RedirectToAction("Index", "Staff");
                        }
                        else
                        {
                            // Default redirect if no specific role is found
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
                // If sign-in fails, add an error to the model state
                ModelState.AddModelError("", "Login Failure.");
            }
            return View(vm);
        }



        public IActionResult Index()
        {
            return View();
        }
    }
}
