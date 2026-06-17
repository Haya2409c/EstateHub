using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RealtorsPortal.Data;
using RealtorsPortal.Models.Entities;
using RealtorsPortal.Models.ViewModels.Account;
using RealtorsPortal.Services.Interfaces;
using System.Security.Claims;

namespace RealtorsPortal.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _db;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext db,
            IEmailService emailService,
            ILogger<AccountController> logger)
        {
            _userManager   = userManager;
            _signInManager = signInManager;
            _db            = db;
            _emailService  = emailService;
            _logger        = logger;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.ActivePage = "Account";
            return View(new LoginViewModel());
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            ViewBag.ActivePage = "Account";

            if (!ModelState.IsValid) return View(model);

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                var user = await _userManager.FindByEmailAsync(model.Email);
                return await RedirectToDashboard(user!);
            }

            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        // GET: /Account/Signup
        [HttpGet]
        public IActionResult Signup()
        {
            ViewBag.ActivePage = "Account";
            return View(new SignupViewModel());
        }

        // POST: /Account/Signup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signup(SignupViewModel model)
        {
            ViewBag.ActivePage = "Account";

            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                UserName = model.Email,
                Email = model.Email,
                PhoneNumber = model.Phone,
                AccountType = model.AccountType,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var role = model.AccountType == "Agent" ? "Agent" : "Seller";
                var roleResult = await _userManager.AddToRoleAsync(user, role);
                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    ModelState.AddModelError(string.Empty, "Account setup failed. Please try again.");
                    return View(model);
                }

                if (role == "Seller")
                {
                    _db.SellerProfiles.Add(new SellerProfile
                    {
                        UserId = user.Id,
                        PackageTier = "Basic",
                        CreatedAt = DateTime.UtcNow
                    });
                    await _db.SaveChangesAsync();
                }

                await _signInManager.SignInAsync(user, isPersistent: false);

                // Fire-and-forget registration confirmation — don't block signup on email failure
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _emailService.SendRegistrationConfirmationAsync(
                            user.Email!, user.FullName ?? user.UserName!, model.AccountType);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Registration email failed for {Email}", user.Email);
                    }
                });

                return await RedirectToDashboard(user);
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        private async Task<IActionResult> RedirectToDashboard(ApplicationUser user)
        {
            if (await _userManager.IsInRoleAsync(user, "Seller"))
                return RedirectToAction("Index", "Seller");

            if (await _userManager.IsInRoleAsync(user, "Agent"))
                return RedirectToAction("Index", "Agent");

            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return RedirectToAction("Index", "Admin");

            return RedirectToAction("Index", "Home");
        }
    }
}
