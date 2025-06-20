using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGallery.Models;
using System.Security.Claims;

namespace OnlineGallery.Controllers
{
    public class AccountController : Controller
    {
        private readonly OnlineGalleryContext _context;

        public AccountController(OnlineGalleryContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_context.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email уже используется");
                    return View(model);
                }

                var user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    PasswordHash = model.Password, // без хеша — смотри ниже
                    RoleName = "Пользователь"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return RedirectToAction("Login", "Account");
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.PasswordHash == model.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Неверный логин или пароль");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Email ?? ""),
                new Claim(ClaimTypes.Role, user.RoleName ?? "Пользователь")
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            if (user.RoleName == "Админ")
                return RedirectToAction("AdminProfile", "Account");

            return RedirectToAction("UserProfile", "Account");
        }

        [HttpPost]
        [Authorize(Roles = "Пользователь")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCardDetails(string CardHolderName, string CardNumber, string ExpirationDate, string CVV)
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound();

            user.CardHolderName = CardHolderName;
            user.CardNumber = CardNumber;
            user.ExpirationDate = ExpirationDate;
            user.CVV = CVV;

            await _context.SaveChangesAsync();

            return RedirectToAction("UserProfile");
        }

        [HttpGet]
        [Authorize(Roles = "Пользователь")]
        public async Task<IActionResult> UserProfile()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var user = await _context.Users
                .Include(u => u.Images)
                .Include(u => u.Favorites).ThenInclude(f => f.Image)
                .Include(u => u.Likes).ThenInclude(l => l.Image)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return NotFound();

            return View("~/Views/Users/UserProfile.cshtml", user);
        }

        [HttpGet]
        [Authorize(Roles = "Админ")]
        public async Task<IActionResult> AdminProfile()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var admin = await _context.Users
                .Include(u => u.Images)
                .Include(u => u.Favorites).ThenInclude(f => f.Image)
                .Include(u => u.Likes).ThenInclude(l => l.Image)
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (admin == null)
                return NotFound();

            return View("~/Views/Users/AdminProfile.cshtml", admin);
        }

        [HttpGet]
        public async Task<IActionResult> MyPurchases()
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var purchases = await _context.Transactions
                .Where(t => t.UserId == userId && t.Image != null)
                .Include(t => t.Image)
                    .ThenInclude(i => i.User)
                .Include(t => t.Image)
                    .ThenInclude(i => i.Category)
                .Select(t => t.Image!)
                .ToListAsync();

            return View(purchases);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}
