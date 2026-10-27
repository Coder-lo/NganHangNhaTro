using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NganHangNhaTro.Data;
using NganHangNhaTro.Models;

namespace NganHangNhaTro.Controllers;

public class ContactsController : Controller
{
    private readonly AppDbContext _db;
    public ContactsController(AppDbContext db) => _db = db;

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int roomId, string senderName, string senderPhone, string? senderEmail, string? message)
    {
        var room = await _db.Rooms.FirstOrDefaultAsync(r => r.RoomId == roomId && r.Status == "Approved");
        if (room == null) return NotFound();
        if (string.IsNullOrWhiteSpace(senderName) || string.IsNullOrWhiteSpace(senderPhone))
        {
            TempData["Error"] = "Vui lòng nhập họ tên và số điện thoại.";
            return RedirectToAction("Details", "Rooms", new { id = roomId });
        }
        if (senderName.Length > 150 || senderPhone.Length > 20 || (message?.Length ?? 0) > 500)
        {
            TempData["Error"] = "Thông tin liên hệ vượt quá độ dài cho phép.";
            return RedirectToAction("Details", "Rooms", new { id = roomId });
        }
        if (!string.IsNullOrWhiteSpace(senderEmail) && (senderEmail.Length > 256 || !senderEmail.Contains('@'))) { TempData["Error"] = "Email không hợp lệ."; return RedirectToAction("Details", "Rooms", new { id = roomId }); }
        var senderUserId = User.Identity?.IsAuthenticated == true ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;
        _db.Contacts.Add(new Contact { RoomId = roomId, SenderName = senderName.Trim(), SenderPhone = senderPhone.Trim(), SenderEmail = senderEmail?.Trim(), SenderUserId = senderUserId, Message = message?.Trim(), CreatedAt = DateTime.Now });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã gửi thông tin liên hệ đến chủ trọ.";
        return RedirectToAction("Details", "Rooms", new { id = roomId });
    }

    [Authorize(Roles = "Tenant")]
    public async Task<IActionResult> Mine()
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var contacts = await _db.Contacts.Include(c => c.Room).Where(c => c.SenderUserId == uid).OrderByDescending(c => c.CreatedAt).ToListAsync();
        return View(contacts);
    }

    [Authorize(Roles = "Tenant")]
    public async Task<IActionResult> Details(int id)
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var contact = await _db.Contacts.Include(c => c.Room).ThenInclude(r => r!.Landlord).FirstOrDefaultAsync(c => c.ContactId == id && c.SenderUserId == uid);
        if (contact == null) return NotFound();
        return View(contact);
    }
}
