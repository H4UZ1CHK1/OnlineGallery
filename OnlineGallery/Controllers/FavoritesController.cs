using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGallery.Models;
using System.Security.Claims;

namespace OnlineGallery.Controllers
{
    [Authorize]
    public class FavoritesController : Controller
    {
        private readonly OnlineGalleryContext _context;

        public FavoritesController(OnlineGalleryContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var favorites = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Image)
                    .ThenInclude(i => i.Category)
                .Include(f => f.Image)
                    .ThenInclude(i => i.User)
                .ToListAsync();

            return View(favorites);
        }

        public async Task<IActionResult> Add(int imageId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var alreadyExists = await _context.Favorites
                .AnyAsync(f => f.UserId == userId && f.ImageId == imageId);

            if (!alreadyExists)
            {
                var favorite = new Favorite
                {
                    UserId = userId,
                    ImageId = imageId,
                    DateAdded = DateTime.Now
                };

                _context.Favorites.Add(favorite);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index", "Images");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.FavoriteId == id && f.UserId == userId);

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
