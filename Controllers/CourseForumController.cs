using COTHUYPRO.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace COTHUYPRO.Controllers;

[Authorize]
public class CourseForumController : Controller
{
    private readonly TrainingContext _context;

    public CourseForumController(TrainingContext context)
    {
        _context = context;
    }

    // Lấy danh sách tin nhắn của một khóa học (Partial View)
    public async Task<IActionResult> GetMessages(int courseId)
    {
        var messages = await _context.CourseForumMessages
            .Include(m => m.User)
            .Where(m => m.CourseId == courseId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(100) // Giới hạn 100 tin nhắn gần nhất
            .ToListAsync();

        ViewBag.CurrentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        ViewBag.CourseId = courseId;
        
        return PartialView("_ForumPartial", messages.OrderBy(m => m.CreatedAt).ToList());
    }

    // Gửi tin nhắn mới
    [HttpPost]
    public async Task<IActionResult> SendMessage(int courseId, string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return BadRequest("Nội dung không được để trống.");

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        var message = new CourseForumMessage
        {
            CourseId = courseId,
            UserId = userId,
            Content = content.Trim(),
            CreatedAt = DateTime.Now
        };

        _context.CourseForumMessages.Add(message);
        await _context.SaveChangesAsync();

        // Trả về danh sách tin nhắn mới nhất để cập nhật giao diện
        return RedirectToAction("GetMessages", new { courseId = courseId });
    }
}
