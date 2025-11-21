using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Part2_CMCS.Data;
using Part2_CMCS.Models;

// Alias to avoid conflict with Part2_CMCS.Models.Claim
using SecClaim = System.Security.Claims.Claim;

namespace Part2_CMCS.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly PasswordHasher<User> _hasher = new PasswordHasher<User>();

        public AccountController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel());
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _db.Users.FirstOrDefault(u => u.Username == model.Username);

            if (user == null)
            {
                ViewBag.Error = "Invalid username or password";
                return View(model);
            }

            // Verify hashed password
            var result = _hasher.VerifyHashedPassword(user, user.Password, model.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                ViewBag.Error = "Invalid username or password";
                return View(model);
            }

            // Create secure claims
            var claims = new List<SecClaim>
            {
                new SecClaim(ClaimTypes.Name, user.Username),
                new SecClaim(ClaimTypes.Role, user.Role),
                new SecClaim("FullName", user.FullName ?? user.Username)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (_db.Users.Any(u => u.Username == model.Username))
            {
                ViewBag.Error = "User already exists";
                return View(model);
            }

            var allowedRoles = new[] { "Lecturer", "PC", "Manager" };
            if (!allowedRoles.Contains(model.Role))
            {
                ViewBag.Error = "Invalid role selection";
                return View(model);
            }

            var newUser = new User
            {
                Username = model.Username,
                FullName = model.FullName,
                Role = model.Role
            };

            // Hash password
            newUser.Password = _hasher.HashPassword(newUser, model.Password);

            _db.Users.Add(newUser);
            _db.SaveChanges();

            TempData["Success"] = "User registered successfully.";
            return RedirectToAction("Login");
        }
    }
}
