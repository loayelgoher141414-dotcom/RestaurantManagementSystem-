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
        // Display all employees
        // =========================
        public IActionResult Index()
        {
            var employees = _context.Users
                .Include(u => u.Branch)
                .Where(u => u.Role == "Employee")
                .AsNoTracking()
                .ToList();

            return View(employees);
        }

        // =========================
        // View employee profile
        // =========================
        [HttpGet]
        public IActionResult Details(int id)
        {
            var employee = _context.Users
                .Include(u => u.Branch)
                .AsNoTracking()
                .FirstOrDefault(u =>
                    u.UserId == id &&
                    u.Role == "Employee");

            if (employee == null)
                return NotFound();

            return View(employee);
        }

        // =========================
        // Add Employee - GET
        // =========================
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Branches = _context.Branches
                .AsNoTracking()
                .ToList();

            return View();
        }

        // =========================
        // Add Employee - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(User user)
        {
            // New users created from this page are Employees
            user.Role = "Employee";

            // Remove Role validation because it is assigned automatically
            ModelState.Remove(
                nameof(RestaurantManagementSystem.Models.User.Role));

            if (ModelState.IsValid)
            {
                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Branches = _context.Branches
                .AsNoTracking()
                .ToList();

            return View(user);
        }

        // =========================
        // Edit Employee - GET
        // =========================
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var employee = _context.Users
                .FirstOrDefault(u =>
                    u.UserId == id &&
                    u.Role == "Employee");

            if (employee == null)
                return NotFound();

            ViewBag.Branches = _context.Branches
                .AsNoTracking()
                .ToList();

            return View(employee);
        }

        // =========================
        // Edit Employee - POST
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(User user)
        {
            // Keep the user as Employee from the Employee edit page
            user.Role = "Employee";

            // Remove Role validation because it is assigned automatically
            ModelState.Remove(
                nameof(RestaurantManagementSystem.Models.User.Role));

            if (ModelState.IsValid)
            {
                _context.Users.Update(user);
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Branches = _context.Branches
                .AsNoTracking()
                .ToList();

            return View(user);
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

            // Only two roles are allowed
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
        // Delete Employee - GET
        // =========================
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var employee = _context.Users
                .Include(u => u.Branch)
                .AsNoTracking()
                .FirstOrDefault(u =>
                    u.UserId == id &&
                    u.Role == "Employee");

            if (employee == null)
                return NotFound();

            return View(employee);
        }

        // =========================
        // Delete Employee - POST
        // =========================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var employee = _context.Users
                .FirstOrDefault(u =>
                    u.UserId == id &&
                    u.Role == "Employee");

            if (employee == null)
                return NotFound();

            _context.Users.Remove(employee);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}