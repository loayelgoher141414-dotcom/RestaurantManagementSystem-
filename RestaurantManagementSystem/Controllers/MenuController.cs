using Microsoft.AspNetCore.Mvc;
using RestaurantManagementSystem.Models;

namespace RestaurantManagementSystem.Controllers
{
    public class MenuController : Controller
    {
        RMSContext _context = new RMSContext();

        public IActionResult Index()
        {
            var menuItems = _context.MenuItems.ToList();

            return View(menuItems);
        }
    }
}
