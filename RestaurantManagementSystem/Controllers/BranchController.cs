using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.Controllers
{
    public class BranchController : Controller
    {
        private readonly RMSContext _context;

        public BranchController()
        {
            _context = new RMSContext();
        }

        public async Task<IActionResult> Index()
        {
            var branches = await _context.Branches
                .Include(b => b.Manager)
                .OrderBy(b => b.BranchId)
                .ToListAsync();

            return View(branches);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var branch = await _context.Branches
                .Include(b => b.Manager)
                .FirstOrDefaultAsync(b => b.BranchId == id);

            if (branch == null) return NotFound();

            return View(branch);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Address,BranchPhoneNumber")] Branch branch)
        {
            if (ModelState.IsValid)
            {
                _context.Add(branch);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Branch added successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(branch);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var branch = await _context.Branches.FindAsync(id);
            if (branch == null) return NotFound();

            return View(branch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BranchId,Address,BranchPhoneNumber,ManagerId")] Branch branch)
        {
            if (id != branch.BranchId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(branch);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Branch updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BranchExists(branch.BranchId)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(branch);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var branch = await _context.Branches
                .Include(b => b.Manager)
                .FirstOrDefaultAsync(b => b.BranchId == id);

            if (branch == null) return NotFound();

            return View(branch);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch != null)
            {
                bool hasUsers = await _context.Users.AnyAsync(u => u.BranchId == id);
                bool hasOrders = await _context.Orders.AnyAsync(o => o.BranchId == id);

                if (hasUsers || hasOrders)
                {
                    ModelState.AddModelError(string.Empty,
                        "This branch cannot be deleted because it still has users or orders linked to it.");
                    return View(branch);
                }

                _context.Branches.Remove(branch);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Branch deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> AssignManager(int? id)
        {
            if (id == null) return NotFound();

            var branch = await _context.Branches
                .Include(b => b.Manager)
                .FirstOrDefaultAsync(b => b.BranchId == id);

            if (branch == null) return NotFound();

            var employees = await _context.Users
                .Where(u => u.BranchId == id && u.Role == "Employee")
                .OrderBy(u => u.UserName)
                .ToListAsync();

            ViewBag.Employees = new SelectList(employees, "UserId", "UserName", branch.ManagerId);
            return View(branch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignManager(int id, int? managerId)
        {
            var branch = await _context.Branches.FindAsync(id);
            if (branch == null) return NotFound();

            if (managerId.HasValue)
            {
                var manager = await _context.Users
                    .FirstOrDefaultAsync(u => u.UserId == managerId && u.BranchId == id && u.Role == "Employee");

                if (manager == null)
                {
                    ModelState.AddModelError(string.Empty, "Selected manager must be an employee of this branch.");

                    var employees = await _context.Users
                        .Where(u => u.BranchId == id && u.Role == "Employee")
                        .OrderBy(u => u.UserName)
                        .ToListAsync();

                    ViewBag.Employees = new SelectList(employees, "UserId", "UserName", branch.ManagerId);
                    return View(branch);
                }
            }

            branch.ManagerId = managerId;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Branch manager updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Orders(int? id)
        {
            if (id == null) return NotFound();

            var branch = await _context.Branches.FirstOrDefaultAsync(b => b.BranchId == id);
            if (branch == null) return NotFound();

            var orders = await _context.Orders
                .Where(o => o.BranchId == id)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderId)
                .ToListAsync();

            ViewBag.Branch = branch;
            return View(orders);
        }

        private bool BranchExists(int id)
        {
            return _context.Branches.Any(e => e.BranchId == id);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
