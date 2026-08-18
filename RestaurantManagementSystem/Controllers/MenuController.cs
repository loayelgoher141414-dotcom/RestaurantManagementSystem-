using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace RestaurantManagementSystem.Controllers
{
    public class MenuController : Controller
    {
        private readonly RMSContext _context;

        public MenuController(RMSContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var menuItems = _context.MenuItems.ToList();
            return View(menuItems);
        }

        //Adding Items
        [HttpGet]
        public IActionResult AddItems()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItems(
            MenuItem item,
            IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                return View(item);
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/images/menu");

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var fileName = Guid.NewGuid().ToString()
                               + Path.GetExtension(imageFile.FileName);

                var filePath = Path.Combine(
                    uploadsFolder,
                    fileName);

                using (var stream = new FileStream(
                    filePath,
                    FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                item.ItemImage = "/images/menu/" + fileName;
            }

            _context.MenuItems.Add(item);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        //Editing Items

        [HttpGet]
        public IActionResult EditItem(int id)
        {
            var item = _context.MenuItems
                .FirstOrDefault(x => x.ItemId == id);

            if (item == null)
                return NotFound();

            return View(item);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> EditItem(
    MenuItem item,
    IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                return View(item);
            }

            var existingItem = await _context.MenuItems
                .FirstOrDefaultAsync(x => x.ItemId == item.ItemId);

            if (existingItem == null)
            {
                return NotFound();
            }

            existingItem.ItemName = item.ItemName;
            existingItem.ItemPrice = item.ItemPrice;
            existingItem.Description = item.Description;
            existingItem.Availability = item.Availability;
            existingItem.Category = item.Category;

            // Upload new image only if user selected one
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    "images",
                    "menu"
                );

                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var extension = Path.GetExtension(imageFile.FileName);

                var fileName = Guid.NewGuid().ToString() + extension;

                var filePath = Path.Combine(
                    uploadsFolder,
                    fileName
                );

                using (var stream = new FileStream(
                    filePath,
                    FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                // Delete old image
                if (!string.IsNullOrEmpty(existingItem.ItemImage))
                {
                    var oldImagePath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        existingItem.ItemImage.TrimStart('/')
                            .Replace("/", Path.DirectorySeparatorChar.ToString())
                    );

                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                existingItem.ItemImage = "/images/menu/" + fileName;
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Menu item updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // Delete (GET)
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var item = _context.MenuItems
                .FirstOrDefault(x => x.ItemId == id);

            if (item == null)
                return NotFound();

            return View(item);
        }


        // Delete (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _context.MenuItems
                .FirstOrDefault(x => x.ItemId == id);

            if (item == null)
                return NotFound();

            _context.MenuItems.Remove(item);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}