using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineGallery.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineGallery.Controllers
{
    [Authorize]

    public class LikesController : Controller
    {
        private readonly OnlineGalleryContext _context;

        public LikesController(OnlineGalleryContext context)
        {
            _context = context;
        }

        // GET: Likes
        public async Task<IActionResult> Index()
        {
            int currentUserId = 1;
            var likes = await _context.Likes
                .Include(l => l.Image)
                .Where(l => l.UserId == currentUserId)
                .ToListAsync();

            return View(likes);
        }

        // GET: Likes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var like = await _context.Likes
                .Include(l => l.Image)
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.LikeId == id);
            if (like == null)
            {
                return NotFound();
            }

            return View(like);
        }
        [HttpPost]
        public async Task<IActionResult> Add(int imageId)
        {
            var image = await _context.Images.FindAsync(imageId);
            if (image == null)
                return NotFound();

            var like = new Like
            {
                ImageId = imageId,
                UserId = 1, 
                DateLiked = DateTime.Now
            };
            _context.Likes.Add(like);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Images");
        }

        // GET: Likes/Create
        [HttpGet]
        public async Task<IActionResult> Create(int imageId, int userId)
        {
            var like = new Like
            {
                ImageId = imageId,
                UserId = userId,
                DateLiked = DateTime.Now
            };

            _context.Likes.Add(like);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Images", new { id = imageId });
        }

        // POST: Likes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LikeId,UserId,ImageId,DateLiked")] Like like)
        {
            if (ModelState.IsValid)
            {
                _context.Add(like);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ImageId"] = new SelectList(_context.Images, "ImageId", "ImageId", like.ImageId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", like.UserId);
            return View(like);
        }

        // GET: Likes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var like = await _context.Likes.FindAsync(id);
            if (like == null)
            {
                return NotFound();
            }
            ViewData["ImageId"] = new SelectList(_context.Images, "ImageId", "ImageId", like.ImageId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", like.UserId);
            return View(like);
        }

        // POST: Likes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LikeId,UserId,ImageId,DateLiked")] Like like)
        {
            if (id != like.LikeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(like);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LikeExists(like.LikeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ImageId"] = new SelectList(_context.Images, "ImageId", "ImageId", like.ImageId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", like.UserId);
            return View(like);
        }

        // GET: Likes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var like = await _context.Likes
                .Include(l => l.Image)
                .Include(l => l.User)
                .FirstOrDefaultAsync(m => m.LikeId == id);
            if (like == null)
            {
                return NotFound();
            }

            return View(like);
        }

        // POST: Likes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var like = await _context.Likes.FindAsync(id);
            if (like != null)
            {
                _context.Likes.Remove(like);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LikeExists(int id)
        {
            return _context.Likes.Any(e => e.LikeId == id);
        }
    }
}
