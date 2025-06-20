using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineGallery.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OnlineGallery.Controllers
{
    [Authorize]

    public class TransactionsController : Controller
    {
        private readonly OnlineGalleryContext _context;

        public TransactionsController(OnlineGalleryContext context)
        {
            _context = context;
        }


        // GET: Transactions
        public async Task<IActionResult> Index()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Forbid(); 
            }

            var transactions = _context.Transactions
                .Include(t => t.Image)
                .Include(t => t.User)
                .Where(t => t.UserId == userId);

            return View(await transactions.ToListAsync());
        }

        // GET: Transactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Image)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transactions/Create
        public IActionResult Create()
        {
            ViewData["ImageId"] = new SelectList(_context.Images, "ImageId", "ImageId");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId");
            return View();
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TransactionId,ImageId,TransactionDate")] Transactions transaction)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdStr, out int userId))
                return Forbid();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();

            if (string.IsNullOrWhiteSpace(user.CardNumber) ||
                string.IsNullOrWhiteSpace(user.ExpirationDate) ||
                string.IsNullOrWhiteSpace(user.CVV))
            {
                TempData["Error"] = "Перед покупкой заполните реквизиты карты в профиле.";
                return RedirectToAction("UserProfile", "Account");
            }

            transaction.UserId = userId;
            transaction.TransactionDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                _context.Add(transaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ImageId"] = new SelectList(_context.Images, "ImageId", "ImageId", transaction.ImageId);
            return View(transaction);
        }


        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            ViewData["ImageId"] = new SelectList(_context.Images, "ImageId", "ImageId", transaction.ImageId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", transaction.UserId);
            return View(transaction);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TransactionId,UserId,ImageId,TransactionDate")] Transactions transaction)
        {
            if (id != transaction.TransactionId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.TransactionId))
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
            ViewData["ImageId"] = new SelectList(_context.Images, "ImageId", "ImageId", transaction.ImageId);
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "UserId", transaction.UserId);
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Image)
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
            return _context.Transactions.Any(e => e.TransactionId == id);
        }
        [Authorize]
        public async Task<IActionResult> MyPurchases()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdStr, out int userId))
                return Forbid();

            var purchases = await _context.Transactions
                .Where(t => t.UserId == userId && t.Image != null)
                .Include(t => t.Image!)
                    .ThenInclude(i => i.User)
                .Include(t => t.Image!)
                    .ThenInclude(i => i.Category)
                .Select(t => t.Image!)
                .ToListAsync();

            return View(purchases);
        }
    }
}
