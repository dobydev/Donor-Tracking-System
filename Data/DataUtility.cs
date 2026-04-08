using Microsoft.AspNetCore.Identity;
using DonorTrackingSystem.Models;

namespace DonorTrackingSystem.Data
{
    public static class DataUtility
    {
        public static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Define roles
            string[] roles = { "Administrator", "Office Manager", "Support Staff" };

            // Create roles if they do not exist
            foreach (var role in roles)
            {                               
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed default users for each role

            // Administrator: ID 1000, Password 123456
            await CreateUserIfNotExists(userManager, "1000", "123456", "Administrator");

            // Office Manager: ID 2000, Password 654321
            await CreateUserIfNotExists(userManager, "2000", "654321", "Office Manager");

            // Support Staff: ID 3000, Password 111111
            await CreateUserIfNotExists(userManager, "3000", "111111", "Support Staff");
        }

        // Helper method to create a user if it does not exist
        private static async Task CreateUserIfNotExists(UserManager<ApplicationUser> userManager, string ID, string password, string role)
        {
            // Check if the user already exists
            if (await userManager.FindByNameAsync(ID) == null)
            {
                var user = new ApplicationUser
                {
                    UserName = ID
                };

                // Create the user with the specified password
                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
        }
    }
}
