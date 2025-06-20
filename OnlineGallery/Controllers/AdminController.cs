using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineGallery.Models;

[Authorize(Roles = "Админ")]
public class AdminController : Controller
{
    private readonly OnlineGalleryContext _context;

    public AdminController(OnlineGalleryContext context)
    {
        _context = context;
    }

    public IActionResult Moderation()
    {
        var pending = _context.Images
            .Where(i => i.Status == "Pending")
            .ToList();

        return View(pending);
    }

    [HttpPost]
    public IActionResult Approve(int id)
    {
        _context.Database.ExecuteSqlRaw("UPDATE Images SET Status = 'Approved' WHERE ImageId = {0}", id);
        return RedirectToAction("Moderation");
    }

    [HttpPost]
    public IActionResult Reject(int id, string comment)
    {
        _context.Database.ExecuteSqlRaw("UPDATE Images SET Status = 'Rejected', ModerationComment = {1} WHERE ImageId = {0}", id, comment);
        return RedirectToAction("Moderation");
    }
}
