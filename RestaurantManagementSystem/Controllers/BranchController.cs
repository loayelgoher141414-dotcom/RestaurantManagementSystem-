using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.Controllers
{
    [Authorize(Roles = "Employee")]
    public class BranchController : Controller
    {
        private readonly RMSContext _context;

        public BranchController(RMSContext context)
        {
            _context = context;
        }

        // Display all branches

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var branches = await _context.Branches
                .OrderBy(b => b.BranchId)
                .AsNoTracking()
                .ToListAsync();

            return View(branches);
        }


        // Branch Details

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var branch = await _context.Branches
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BranchId == id);

            if (branch == null)
                return NotFound();

            return View(branch);
        }


        // Create (GET)

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }


        // Create (POST)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Address,BranchPhoneNumber")] Branch branch)
        {
            if (!ModelState.IsValid)
                return View(branch);

            _context.Branches.Add(branch);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Branch added successfully.";

            return RedirectToAction(nameof(Index));
        }


        // Edit (GET)

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var branch =
                await _context.Branches.FindAsync(id);

            if (branch == null)
                return NotFound();

            return View(branch);
        }


        // Edit (POST)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("BranchId,Address,BranchPhoneNumber")]
            Branch branch)
        {
            if (id != branch.BranchId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(branch);

            try
            {
                _context.Update(branch);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Branch updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BranchExists(branch.BranchId))
                    return NotFound();

                throw;
            }
        }


        // Delete (GET)

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var branch = await _context.Branches
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    b => b.BranchId == id);

            if (branch == null)
                return NotFound();

            return View(branch);
        }


        // Delete (POST)

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var branch =
                await _context.Branches.FindAsync(id);

            if (branch == null)
                return NotFound();

            // Check ApplicationUsers
            bool hasUsers =
                await _context.Users
                    .AnyAsync(u => u.BranchId == id);

            // Check Orders
            bool hasOrders =
                await _context.Orders
                    .AnyAsync(o => o.BranchId == id);

            if (hasUsers || hasOrders)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "This branch cannot be deleted because it still has users or orders linked to it.");

                return View(branch);
            }

            _context.Branches.Remove(branch);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Branch deleted successfully.";

            return RedirectToAction(nameof(Index));
        }


        // Orders by Branch

        [HttpGet]
        public async Task<IActionResult> Orders(int? id)
        {
            if (id == null)
                return NotFound();

            var branch = await _context.Branches
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    b => b.BranchId == id);

            if (branch == null)
                return NotFound();

            var orders = await _context.Orders
                .Where(o => o.BranchId == id)
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .OrderByDescending(o => o.OrderId)
                .AsNoTracking()
                .ToListAsync();

            ViewBag.Branch = branch;

            return View(orders);
        }


        // Branch Exists

        private bool BranchExists(int id)
        {
            return _context.Branches
                .Any(e => e.BranchId == id);
        }
    }
}