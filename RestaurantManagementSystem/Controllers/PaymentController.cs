using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly RMSContext _context;

        public PaymentsController(RMSContext context)
        {
            _context = context;
        }

        // =========================
        // Invoice + Payment Page
        // =========================
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

        // =========================
        // Pay Order
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Pay(int orderId, string paymentMethod)
        {
            var order = _context.Orders
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
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
            _context.SaveChanges();

            return RedirectToAction("Receipt", new { id = payment.PaymentId });
        }

        // =========================
        // Receipt
        // =========================
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