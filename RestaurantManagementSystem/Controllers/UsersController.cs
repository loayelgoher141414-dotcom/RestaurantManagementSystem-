using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.Controllers
{
    public class UsersController : Controller
    {
        private readonly RMSContext _context;

        public UsersController()
        {
            _context = new RMSContext();
        }

        // =========================
        // Display all users
        // =========================
        [HttpGet]
        public IActionResult Index()
        {
            var users = _context.Users
                .Include(u => u.Branch)
                .AsNoTracking()
                .ToList();

            return View(users);
        }

        // =========================
        // View user profile
        // =========================
        [HttpGet]
        public IActionResult Details(int id)
        {
            var user = _context.Users
                .Include(u => u.Branch)
                .AsNoTracking()
                .FirstOrDefault(u => u.UserId == id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        // =========================
        // Add User - GET
        // =========================
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Branches = _context.Branches
                .AsNoTracking()
                .ToList();

            ViewBag.Roles = new List<string>
            {
                "Employee",
                "Customer"
            };

            return View();
        }

        // =========================
        // Add User - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(User user)
        {
            // Validate Role
            if (user.Role != "Employee" && user.Role != "Customer")
            {
                ModelState.AddModelError(
                    nameof(RestaurantManagementSystem.Models.User.Role),
                    "Role must be Employee or Customer.");
            }

            // Validate Branch
            if (user.BranchId <= 0)
            {
                ModelState.AddModelError(
                    nameof(RestaurantManagementSystem.Models.User.BranchId),
                    "Please select a branch.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Branches = _context.Branches
                    .AsNoTracking()
                    .ToList();

                ViewBag.Roles = new List<string>
                {
                    "Employee",
                    "Customer"
                };

                return View(user);
            }

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // Edit User - GET
        // =========================
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.UserId == id);

            if (user == null)
                return NotFound();

            ViewBag.Branches = _context.Branches
                .AsNoTracking()
                .ToList();

            ViewBag.Roles = new List<string>
            {
                "Employee",
                "Customer"
            };

            return View(user);
        }

        // =========================
        // Edit User - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(User user)
        {
            // Validate Role
            if (user.Role != "Employee" && user.Role != "Customer")
            {
                ModelState.AddModelError(
                    nameof(RestaurantManagementSystem.Models.User.Role),
                    "Role must be Employee or Customer.");
            }

            // Validate Branch
            if (user.BranchId <= 0)
            {
                ModelState.AddModelError(
                    nameof(RestaurantManagementSystem.Models.User.BranchId),
                    "Please select a branch.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Branches = _context.Branches
                    .AsNoTracking()
                    .ToList();

                ViewBag.Roles = new List<string>
                {
                    "Employee",
                    "Customer"
                };

                return View(user);
            }

            var existingUser = _context.Users
                .FirstOrDefault(u => u.UserId == user.UserId);

            if (existingUser == null)
                return NotFound();

            existingUser.UserName = user.UserName;
            existingUser.UserPhoneNumber = user.UserPhoneNumber;
            existingUser.Email = user.Email;
            existingUser.Address = user.Address;
            existingUser.Role = user.Role;
            existingUser.BranchId = user.BranchId;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // Assign Role - GET
        // =========================
        [HttpGet]
        public IActionResult AssignRole(int id)
        {
            var user = _context.Users
                .Include(u => u.Branch)
                .AsNoTracking()
                .FirstOrDefault(u => u.UserId == id);

            if (user == null)
                return NotFound();

            ViewBag.Roles = new List<string>
            {
                "Employee",
                "Customer"
            };

            return View(user);
        }

        // =========================
        // Assign Role - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AssignRole(int id, string role)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.UserId == id);

            if (user == null)
                return NotFound();

            if (role != "Employee" && role != "Customer")
            {
                ModelState.AddModelError(
                    "Role",
                    "Role must be Employee or Customer.");

                ViewBag.Roles = new List<string>
                {
                    "Employee",
                    "Customer"
                };

                return View(user);
            }

            user.Role = role;

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // =========================
        // Delete User - GET
        // =========================
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var user = _context.Users
                .Include(u => u.Branch)
                .AsNoTracking()
                .FirstOrDefault(u => u.UserId == id);

            if (user == null)
                return NotFound();

            return View(user);
        }

        // =========================
        // Delete User - POST
        // =========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.UserId == id);

            if (user == null)
                return NotFound();

            _context.Users.Remove(user);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}