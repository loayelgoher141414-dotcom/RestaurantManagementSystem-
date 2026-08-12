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

        // Display all employees
        public IActionResult Index()
        {
            var employees = _context.Users
                .Include(u => u.Branch)
                .Where(u => u.Role == "Employee")
                .AsNoTracking()
                .ToList();

            return View(employees);
        }

        // View employee profile
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

        // Add Employee - GET
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Branches = _context.Branches
                .AsNoTracking()
                .ToList();

            return View();
        }

        // Add Employee - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(RestaurantManagementSystem.Models.User user)
        {
            // Employee is the default role for this page
            user.Role = "Employee";

            // Role is assigned automatically
            ModelState.Remove(nameof(RestaurantManagementSystem.Models.User.Role));

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

        // Edit Employee - GET
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

        // Edit Employee - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(RestaurantManagementSystem.Models.User user)
        {
            // Keep the employee role
            user.Role = "Employee";

            ModelState.Remove(nameof(RestaurantManagementSystem.Models.User.Role));

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

        // Delete Employee - GET
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

        // Delete Employee - POST
        [HttpPost]
        [ActionName("Delete")]
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