using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly RMSContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(
            RMSContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // Index

        [HttpGet]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Branch)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .OrderByDescending(o => o.OrderId)
                .AsNoTracking()
                .ToListAsync();

            return View(orders);
        }


        // Add Order

        [HttpGet]
        public async Task<IActionResult> AddOrder()
        {
            var branches = await _context.Branches
                .AsNoTracking()
                .ToListAsync();

            var menuItems = await _context.MenuItems
                .Where(m => m.Availability)         
                .AsNoTracking()
                .ToListAsync();

            var viewModel = new AddOrderVM
            {
                Branches = branches,
                AvailableMenuItems = menuItems
            };

            return View("AddOrder", viewModel);
        }


        // Save Order

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveOrder(
            AddOrderVM order)
        {
            var currentUser =
                await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Unauthorized();

            if (order.Items == null ||
                !order.Items.Any(i => i.IsSelected))
            {
                TempData["Message"] =
                    "Please select at least one item.";

                return RedirectToAction(nameof(AddOrder));
            }

            var orderToDb = new Order
            {
                UserId = currentUser.Id,
                BranchId = order.BranchId,
                OrderStatus = "Pending"
            };

            foreach (var item in order.Items)
            {
                if (!item.IsSelected)
                    continue;

                var menuItem =
                    await _context.MenuItems
                        .FirstOrDefaultAsync(
                            x => x.ItemId == item.ItemId);

                if (menuItem == null)
                    continue;

                var orderItem = new OrderItem
                {
                    ItemId = menuItem.ItemId,
                    Quantity = item.Quantity,
                    ItemPrice = menuItem.ItemPrice
                };

                orderToDb.OrderItems.Add(orderItem);
            }

            if (orderToDb.OrderItems.Count == 0)
            {
                TempData["Message"] =
                    "Failed to create order.";

                return RedirectToAction(nameof(AddOrder));
            }

            orderToDb.TotalPrice =
                orderToDb.OrderItems
                    .Sum(x => x.ItemPrice * x.Quantity);

            _context.Orders.Add(orderToDb);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                nameof(ShowDetails),
                new { id = orderToDb.OrderId });
        }


        // Show Order Details

        [HttpGet]
        public async Task<IActionResult> ShowDetails(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Unauthorized();

            var order = await _context.Orders
                .Include(x => x.OrderItems)
                    .ThenInclude(i => i.Item)
                .Include(x => x.Branch)
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.OrderId == id);

            if (order == null)
                return NotFound();

            // Employee can view any order
            if (!User.IsInRole("Employee") &&
                order.UserId != currentUser.Id)
            {
                return Forbid();
            }

            return View("ShowDetails", order);
        }


        // Cancel Order

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Unauthorized();

            var order = await _context.Orders
                .FirstOrDefaultAsync(x => x.OrderId == id);

            if (order == null)
                return NotFound();

            if (!User.IsInRole("Employee") &&
                order.UserId != currentUser.Id)
            {
                return Forbid();
            }

            order.OrderStatus = "Cancelled";

            await _context.SaveChangesAsync();

            TempData["Message"] =
                "The order has been cancelled.";

            return RedirectToAction(
                nameof(ShowDetails),
                new { id = order.OrderId });
        }


        // Confirm Order (Employee Only)

        [Authorize(Roles = "Employee,Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmOrder(
            int id)
        {
            var order =
                await _context.Orders
                    .FirstOrDefaultAsync(
                        x => x.OrderId == id);

            if (order == null)
                return NotFound();

            order.OrderStatus = "Confirmed";

            await _context.SaveChangesAsync();

            TempData["Message"] =
                "The order has been confirmed.";

            return RedirectToAction(
                nameof(ShowDetails),
                new { id = order.OrderId });
        }


        // Update Quantity

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int orderId,int orderItemId,int newQuantity)
        {
            if (newQuantity <= 0)
            {
                TempData["Message"] =
                    "Quantity must be greater than zero.";

                return RedirectToAction(
                    nameof(ShowDetails),
                    new { id = orderId });
            }

            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return Unauthorized();

            var order = await _context.Orders
                .Include(x => x.OrderItems)
                .FirstOrDefaultAsync(x => x.OrderId == orderId);

            if (order == null)
                return NotFound();

            if (!User.IsInRole("Employee") &&
                order.UserId != currentUser.Id)
            {
                return Forbid();
            }

            var orderItem = order.OrderItems
                .FirstOrDefault(x => x.OrderItemId == orderItemId);

            if (orderItem == null)
                return NotFound();

            orderItem.Quantity = newQuantity;

            order.TotalPrice = order.OrderItems
                .Sum(x => x.ItemPrice * x.Quantity);

            await _context.SaveChangesAsync();

            TempData["Message"] =
                "Item quantity has been changed.";

            return RedirectToAction(
                nameof(ShowDetails),
                new { id = order.OrderId });
        }

        // Delete Item

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(
            int orderId,
            int orderItemId)
        {
            var order = await _context.Orders
                .Include(x => x.OrderItems)
                .FirstOrDefaultAsync(
                    x => x.OrderId == orderId);

            if (order == null)
                return NotFound();

            var orderItem =
                order.OrderItems.FirstOrDefault(
                    x => x.OrderItemId == orderItemId);

            if (orderItem == null)
                return NotFound();

            order.OrderItems.Remove(orderItem);

            order.TotalPrice =
                order.OrderItems
                    .Sum(x => x.ItemPrice * x.Quantity);

            await _context.SaveChangesAsync();

            TempData["Message"] =
                "Item has been removed.";

            return RedirectToAction(
                nameof(ShowDetails),
                new { id = order.OrderId });
        }
    }
}