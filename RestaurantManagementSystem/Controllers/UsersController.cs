using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.Controllers
{
    [Authorize(Roles = "Employee")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly RMSContext _context;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            RMSContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }


        // Display all users
        [HttpGet]

        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Index(string? role)
        {
            var users = await _context.Users
                .Include(u => u.Branch)
                .ToListAsync();

            // Filter by role
            if (!string.IsNullOrEmpty(role))
            {
                var filteredUsers = new List<ApplicationUser>();

                foreach (var user in users)
                {
                    var userRoles = await _userManager.GetRolesAsync(user);

                    if (userRoles.Contains(role))
                    {
                        filteredUsers.Add(user);
                    }
                }

                users = filteredUsers;
            }

            var userRolesDictionary = new Dictionary<int, string>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (roles.Any())
                {
                    userRolesDictionary[user.Id] = roles.First();
                }
            }

            ViewBag.UserRoles = userRolesDictionary;

            return View(users);
        }


        // View user profile

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.Users
                .Include(u => u.Branch)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            ViewBag.CurrentRole = roles.FirstOrDefault() ?? "No Role";

            return View(user);
        }


        // Add User - GET

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadCreateData();

            return View();
        }


        // Add User - POST

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ApplicationUser user,
            string password,
            string role)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "Password",
                    "Password is required.");
            }

            if (string.IsNullOrWhiteSpace(role) ||
                (role != "Employee" && role != "Customer"))
            {
                ModelState.AddModelError(
                    "Role",
                    "Role must be Employee or Customer.");
            }

            if (!ModelState.IsValid)
            {
                await LoadCreateData();
                return View(user);
            }

            // Make username equal to email
            user.UserName = user.Email;

            var result = await _userManager.CreateAsync(
                user,
                password);

            if (result.Succeeded)
            {
                // Make sure role exists
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(
                        new IdentityRole<int>(role));
                }

                await _userManager.AddToRoleAsync(
                    user,
                    role);

                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            await LoadCreateData();

            return View(user);
        }


        // Edit User (GET)

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            await LoadCreateData();

            var roles = await _userManager.GetRolesAsync(user);

            ViewBag.CurrentRole =
                roles.FirstOrDefault() ?? "Customer";

            return View(user);
        }


        // Edit User (POST)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ApplicationUser model,
            string role)
        {
            var user = await _userManager
                .FindByIdAsync(id.ToString());

            if (user == null)
                return NotFound();

            if (role != "Employee" && role != "Customer")
            {
                ModelState.AddModelError(
                    "Role",
                    "Role must be Employee or Customer.");
            }

            if (!ModelState.IsValid)
            {
                await LoadCreateData();

                ViewBag.CurrentRole = role;

                return View(model);
            }

            user.Name = model.Name;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;
            user.BranchId = model.BranchId;

            var updateResult =
                await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                await LoadCreateData();

                return View(model);
            }

            // Update Role
            var currentRoles =
                await _userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(
                    user,
                    currentRoles);
            }

            await _userManager.AddToRoleAsync(
                user,
                role);

            return RedirectToAction(nameof(Index));
        }

        // Delete User (GET)

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.Users
                .Include(u => u.Branch)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            return View(user);
        }


        // Delete User (POST)

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
                return NotFound();

            // Prevent employee from deleting himself
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser != null && currentUser.Id == user.Id)
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View("Delete", user);
            }

            TempData["Success"] = "User deleted successfully.";

            return RedirectToAction(nameof(Index));
        }


        // Helper Method

        private async Task LoadCreateData()
        {
            ViewBag.Branches = await _context.Branches
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Roles = new List<string>
            {
                "Employee",
                "Customer"
            };
        }
    }
}