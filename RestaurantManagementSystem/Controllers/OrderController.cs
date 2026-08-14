using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.Controllers
{
    public class OrderController : Controller
    {
        RMSContext _context = new RMSContext();
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddOrder()
        {
            var branches = _context.Branches.ToList();

            var branchesVM = new AddOrderVM();
            var menuItems = _context.MenuItems.ToList();


            AddOrderVM viewModel = new AddOrderVM();
            viewModel.Branches = branches;
            viewModel.AvailableMenuItems = menuItems;

            return View("AddOrder", viewModel);
        }



        public IActionResult SaveOrder(AddOrderVM order)
        {
            Order order_to_db = new Order();
            order_to_db.UserId = order.UserId;
            order_to_db.BranchId = order.BranchId;
            foreach (var item in order.Items)
            {
                if (!item.IsSelected) continue;

                var menuitem = _context.MenuItems.FirstOrDefault(x => x.ItemId == item.ItemId);
                if (menuitem == null)
                {
                    continue;
                }

                OrderItem orderitem = new OrderItem();
                orderitem.ItemId = menuitem.ItemId;
                orderitem.Quantity = item.Quantity;
                orderitem.ItemPrice = menuitem.ItemPrice;
                order_to_db.OrderItems.Add(orderitem);

            }
            if (order_to_db.OrderItems.Count == 0)
            {
                return Content("Failed Request");
            }
            order_to_db.TotalPrice = order_to_db.OrderItems.Sum(x => x.ItemPrice * x.Quantity);
            order_to_db.OrderStatus = "Pending";
            _context.Orders.Add(order_to_db);
            _context.SaveChanges();

            return RedirectToAction("ShowDetails", new { id = order_to_db.OrderId });

        }




        public IActionResult ShowDetails(int id)
        {
            var order = _context.Orders.Include(x => x.OrderItems).
                                        ThenInclude(i => i.Item)
                                        .Include(x => x.Branch)
                                        .FirstOrDefault(i => i.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View("ShowDetails", order);

        }


        public IActionResult CancelOrder(int id)
        {
            var order = _context.Orders.FirstOrDefault(x => x.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }
            order.OrderStatus = "Cancelled";
            _context.SaveChanges();

            TempData["Message"] = "The order has been cancelled";

            return RedirectToAction("ShowDetails", new { id = order.OrderId });
        }

        public IActionResult ConfirmOrder(int id)
        {
            var order = _context.Orders.FirstOrDefault(x => x.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }
            order.OrderStatus = "Confirmed";
            _context.SaveChanges();

            TempData["Message"] = "The order has been Confirmed";

            return RedirectToAction("ShowDetails", new { id = order.OrderId });
        }


        public IActionResult UpdateQuantity(int orderId , int orderItemId , int newQuantity)
        {
            var order = _context.Orders.Include(x => x.OrderItems)
                .FirstOrDefault(x => x.OrderId == orderId);

            if (order == null)
            {
                return NotFound();
            }

            var orderitem = order.OrderItems.FirstOrDefault(x => x.OrderItemId == orderItemId);

            if (orderitem == null)
            {
                return NotFound();
            }

            orderitem.Quantity = newQuantity;
            order.TotalPrice = order.OrderItems.Sum(x => x.ItemPrice * x.Quantity);

            TempData["Message"] = "Your item quantity has been changed";
            _context.SaveChanges();

            return RedirectToAction("ShowDetails", new { id = order.OrderId });
            
        }

        public IActionResult DeleteItem(int orderId, int orderItemId)
        {
            var order = _context.Orders.Include(x => x.OrderItems).FirstOrDefault(x => x.OrderId == orderId);
            if (order == null)
            {
                return NotFound();
            }

            var orderitem = order.OrderItems.FirstOrDefault(x => x.OrderItemId == orderItemId);
            if (orderitem == null)
            {
                return NotFound();
            }

            order.OrderItems.Remove(orderitem);
            order.TotalPrice = order.OrderItems.Sum(x => x.ItemPrice * x.Quantity);
            TempData["Message"] = "Item has been removed";
            _context.SaveChanges();

            return RedirectToAction("ShowDetails", new { id = order.OrderId });
        }
    }

    }
