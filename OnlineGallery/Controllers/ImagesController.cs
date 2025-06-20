using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using OnlineGallery.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OnlineGallery.Controllers
{
    public class ImagesController : Controller
    {
        private readonly OnlineGalleryContext _context;

        public ImagesController(OnlineGalleryContext context)
        {
            _context = context;
        }

        // GET: Images
        [AllowAnonymous]
        public async Task<IActionResult> Index(int? categoryId, string? search, string? sortLikes)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out var userId);

            ViewBag.AllTransactions = await _context.Transactions.ToListAsync();
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategoryId = categoryId;
            ViewBag.CurrentSort = sortLikes;

            var query = _context.Images
                .Include(i => i.Category)
                .Include(i => i.User)
                .AsQueryable();

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                query = query.Where(i =>
                    i.Status == "Approved" ||
                    (i.Status == "Rejected" && i.UserId == userId));
            }
            else
            {
                query = query.Where(i => i.Status == "Approved");
            }

            if (categoryId.HasValue)
                query = query.Where(i => i.CategoryId == categoryId);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(i => i.Title != null && i.Title.Contains(search));

            var images = await query.ToListAsync();

            var model = images
                .Select(img => new ImageWithLikesViewModel
                {
                    Image = img,
                    LikesCount = _context.Likes.Count(l => l.ImageId == img.ImageId)
                })
                .ToList();

            if (sortLikes == "asc")
                model = model.OrderBy(m => m.LikesCount).ToList();
            else if (sortLikes == "desc")
                model = model.OrderByDescending(m => m.LikesCount).ToList();

            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy(int id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int userId))
                return Unauthorized();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(user.CardHolderName) ||
                string.IsNullOrWhiteSpace(user.CardNumber) ||
                string.IsNullOrWhiteSpace(user.ExpirationDate) ||
                string.IsNullOrWhiteSpace(user.CVV))
            {
                TempData["Error"] = "Чтобы совершить покупку, пожалуйста, заполните реквизиты карты в профиле.";
                return RedirectToAction("UserProfile", "Account");
            }

            bool alreadyPurchased = await _context.Transactions
                .AnyAsync(t => t.UserId == userId && t.ImageId == id);

            if (alreadyPurchased)
                return RedirectToAction("Index", "Images");

            var transaction = new Transactions
            {
                UserId = userId,
                ImageId = id,
                TransactionDate = DateTime.Now
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Images");
        }

        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var image = await _context.Images
                .Include(i => i.Category)
                .Include(i => i.User)
                .Include(i => i.Comments).ThenInclude(c => c.User)
                .Include(i => i.Transactions)
                .FirstOrDefaultAsync(m => m.ImageId == id);

            if (image == null)
                return NotFound();

            bool isPurchased = await _context.Transactions
                .AnyAsync(t => t.ImageId == id);

            ViewBag.IsPurchased = isPurchased;
            return View(image);
        }

        [Authorize]
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Image image, IFormFile ImageFile)
        {
            if (ModelState.IsValid)
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdString, out int userId))
                    return Unauthorized();

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                    Directory.CreateDirectory(uploadsFolder);

                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    image.FilePath = "/uploads/" + uniqueFileName;
                }

                image.DateUploaded = DateTime.Now;
                image.CreatedDate = DateTime.Now;
                image.UpdatedDate = null;
                image.CreatedBy = userId;
                image.UserId = userId;
                image.Status = "Pending";

                _context.Add(image);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Изображение успешно отправлено на модерацию!";
                return RedirectToAction("Index");
            }

            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "CategoryName", image.CategoryId);
            ViewBag.Users = new SelectList(_context.Users, "UserId", "FullName", image.UserId);

            return View(image);
        }

        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var image = await _context.Images.FindAsync(id);
            if (image == null)
                return NotFound();

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", image.CategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", image.UserId);
            return View(image);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Image image, IFormFile NewImageFile)
        {
            if (id != image.ImageId)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingImage = await _context.Images.AsNoTracking().FirstOrDefaultAsync(i => i.ImageId == id);
                    if (existingImage == null)
                        return NotFound();

                    if (NewImageFile != null && NewImageFile.Length > 0)
                    {
                        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                        Directory.CreateDirectory(uploads);

                        var newFileName = Guid.NewGuid() + Path.GetExtension(NewImageFile.FileName);
                        var newPath = Path.Combine(uploads, newFileName);

                        using (var stream = new FileStream(newPath, FileMode.Create))
                        {
                            await NewImageFile.CopyToAsync(stream);
                        }

                        image.FilePath = "/uploads/" + newFileName;
                    }
                    else
                    {
                        image.FilePath = existingImage.FilePath;
                    }

                    image.UpdatedDate = DateTime.Now;
                    image.UpdatedBy = 1;

                    _context.Update(image);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Images.Any(e => e.ImageId == image.ImageId))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", image.CategoryId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email", image.UserId);
            return View(image);
        }

        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var image = await _context.Images
                .Include(i => i.Category)
                .Include(i => i.User)
                .FirstOrDefaultAsync(m => m.ImageId == id);

            if (image == null)
                return NotFound();

            return View(image);
        }

        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var image = await _context.Images.FindAsync(id);
            if (image != null)
            {
                var favorites = _context.Favorites.Where(f => f.ImageId == id);
                _context.Favorites.RemoveRange(favorites);

                var likes = _context.Likes.Where(l => l.ImageId == id);
                _context.Likes.RemoveRange(likes);

                var comments = _context.Comments.Where(c => c.ImageId == id);
                _context.Comments.RemoveRange(comments);

                _context.Images.Remove(image);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
