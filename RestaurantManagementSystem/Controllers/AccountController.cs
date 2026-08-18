using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RestaurantManagementSystem.Models;
using RestaurantManagementSystem.ViewModels;

namespace RestaurantManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // REGISTER(GET)

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // REGISTER(POST)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name,
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                BranchId = model.BranchId
            };

            var result = await _userManager.CreateAsync(
                user,
                model.Password
            );

            if (result.Succeeded)
            {
                // Every public registration is Customer
                await _userManager.AddToRoleAsync(user, "Customer");

                await _signInManager.SignInAsync(user, isPersistent: false);

                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description
                );
            }

            return View(model);
        }

        // LOGIN(GET)


        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }


        // LOGIN(POST)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true
            );

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.ReturnUrl)
                    && Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                // Get current logged-in user
                var user = await _userManager.FindByEmailAsync(
                    model.Email
                );

                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    if (roles.Contains("Employee"))
                    {
                        return RedirectToAction(
                            "Index",
                            "Home"
                        );
                    }

                    if (roles.Contains("Customer"))
                    {
                        return RedirectToAction(
                            "Index",
                            "Home"
                        );
                    }
                }

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Your account is temporarily locked. Please try again later."
                );

                return View(model);
            }

            ModelState.AddModelError(
                string.Empty,
                "Invalid email or password."
            );

            return View(model);
        }

        // LOGOUT

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(
                "Index",
                "Home"
            );
        }

        // ACCESS DENIED

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}