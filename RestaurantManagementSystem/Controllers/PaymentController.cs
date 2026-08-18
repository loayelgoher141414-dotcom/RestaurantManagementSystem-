using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.Controllers
{
    public class PaymentController : Controller
    {
        private readonly RMSContext _context;

        public PaymentController(RMSContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Index()
        {
            var payments = await _context.Payments
                .Include(p => p.Order)
                .OrderByDescending(p => p.Date)
                .ToListAsync();

            return View(payments);
        }

        // Invoice

        [Authorize(Roles = "Employee,Customer")]
        [HttpGet]
        public IActionResult Invoice(int id)
        {
            var order = _context.Orders
                .Include(o => o.Branch)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
        // Payment Page

        [Authorize(Roles = "Employee,Customer")]
        [HttpGet]
        public IActionResult Pay(int id)
        {
            var order = _context.Orders
                .FirstOrDefault(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // Pay Order

        [Authorize(Roles = "Employee,Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int orderId, string paymentMethod)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }

            // Prevent paying the same order twice
            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId);

            if (existingPayment != null)
            {
                TempData["Error"] = "This order has already been paid.";
                return RedirectToAction("ShowDetails", "Order",
                    new { id = orderId });
            }

            if (string.IsNullOrEmpty(paymentMethod))
            {
                TempData["Error"] = "Please select a payment method.";
                return RedirectToAction("Pay",
                    new { id = orderId });
            }

            var payment = new Payment
            {
                OrderId = order.OrderId,
                Amount = order.TotalPrice,
                Date = DateTime.Now,
                PaymentMethod = paymentMethod,
                PaymentStatus = "Paid"
            };

            _context.Payments.Add(payment);

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Receipt",
                new { id = payment.PaymentId });
        }

        // Receipt

        [Authorize(Roles = "Employee,Customer")]
        [HttpGet]
        public IActionResult Receipt(int id)
        {
            var payment = _context.Payments
                .Include(p => p.Order)
                .FirstOrDefault(p => p.PaymentId == id);

            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }
    }
}