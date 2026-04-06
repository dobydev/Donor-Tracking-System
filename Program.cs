/* 
   Alex Alvarado, Gabriel Doby, Prabesh Khanal, 
   Justin Kim, Destini Liphart, Nursang Sherpa, Jerome Whitaker
   Group 1: Team Project
   CISS 491: Business Software Development
   April 7th, 2026
 */

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using DonorTrackingSystem.Models;

var builder = WebApplication.CreateBuilder(args);

// Database connection string
var connection = $"Server=(localdb)\\mssqllocaldb;Database=DonorTrackingSystemDb;" +
    $"Trusted_Connection=True;MultipleActiveResultSets=true";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
               options.UseSqlServer(connection));

// Identity configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Configure password requirements for 6-digit numeric passwords
    // Had to do this because Identity's default password requirements are too strict for our use case
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Seed users and roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    await DonorTrackingSystem.Data.DataUtility.SeedUsersAsync(userManager, roleManager);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware configuration
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseStaticFiles();
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();
app.UseAuthorization();
app.UseSession();


// Default route configuration
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
