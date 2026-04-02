/* 
   Alex Alvarado, Gabriel Doby, Prabesh Khanal, 
   Justin Kim, Destini Liphart, Nursang Sherpa, Jerome Whitaker
   Group 1: Team Project
   CISS 491: Business Software Development
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
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
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



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
