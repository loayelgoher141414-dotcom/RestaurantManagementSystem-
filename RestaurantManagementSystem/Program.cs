using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // MVC
            
            builder.Services.AddControllersWithViews();

            // Database

            builder.Services.AddDbContext<RMSContext>(options =>
                options.UseSqlServer(
                    @"Server=.\SQLEXPRESS;Database=RestaurantManagementDB;Trusted_Connection=True;TrustServerCertificate=True;"
                ));

            // Identity

            builder.Services
                .AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
                {
                    options.Password.RequireDigit = true;
                    options.Password.RequireLowercase = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 6;

                    options.User.RequireUniqueEmail = true;

                    options.Lockout.DefaultLockoutTimeSpan =
                        TimeSpan.FromMinutes(5);

                    options.Lockout.MaxFailedAccessAttempts = 5;
                })
                .AddEntityFrameworkStores<RMSContext>()
                .AddDefaultTokenProviders();

            // Cookie Settings

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            var app = builder.Build();

            // HTTP Pipeline

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            // Authentication MUST come before Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // =========================
            // Seed Roles + First Employee
            // =========================
            using (var scope = app.Services.CreateScope())
            {
                var roleManager =
                    scope.ServiceProvider
                        .GetRequiredService<RoleManager<IdentityRole<int>>>();

                var userManager =
                    scope.ServiceProvider
                        .GetRequiredService<UserManager<ApplicationUser>>();

                // Create Roles

                string[] roles =
                {
                    "Employee",
                    "Customer"
                };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(
                            new IdentityRole<int>(role));
                    }
                }

                // Create First Employee

                string employeeEmail = "admin@rms.com";
                string employeePassword = "Admin123";

                var employee =
                    await userManager.FindByEmailAsync(employeeEmail);

                if (employee == null)
                {
                    employee = new ApplicationUser
                    {
                        UserName = employeeEmail,
                        Email = employeeEmail,
                        Name = "System Admin",
                        EmailConfirmed = true
                    };

                    var createResult =
                        await userManager.CreateAsync(
                            employee,
                            employeePassword);

                    if (createResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(
                            employee,
                            "Employee");
                    }
                    else
                    {
                        foreach (var error in createResult.Errors)
                        {
                            Console.WriteLine(
                                $"Employee creation error: {error.Description}");
                        }
                    }
                }
                else
                {
                    // Make sure existing employee has Employee role
                    if (!await userManager.IsInRoleAsync(
                            employee,
                            "Employee"))
                    {
                        await userManager.AddToRoleAsync(
                            employee,
                            "Employee");
                    }
                }
            }

            await app.RunAsync();
        }
    }
}