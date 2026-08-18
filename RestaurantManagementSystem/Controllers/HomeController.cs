using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RestaurantManagementSystem.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            if (User.Identity != null &&
                User.Identity.IsAuthenticated &&
                User.IsInRole("Employee"))
            {
                return RedirectToAction(
                    "Index",
                    "Order"
                );
            }

            return View();
        }
    }
}