using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NganHangNhaTro.Data;
using NganHangNhaTro.Models;

namespace NganHangNhaTro.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _db;
    public AdminController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Dashboard()
    {
        ViewBag.Users = await _db.Users.CountAsync();
        ViewBag.Tenants = await _db.Users.CountAsync(u => u.Role == "Tenant");
        ViewBag.Landlords = await _db.Users.CountAsync(u => u.Role == "Landlord");
        ViewBag.Pending = await _db.Rooms.CountAsync(r => r.Status == "Pending");
        ViewBag.Approved = await _db.Rooms.CountAsync(r => r.Status == "Approved");
        ViewBag.Rented = await _db.Rooms.CountAsync(r => r.Status == "Rented");
        ViewBag.Contacts = await _db.Contacts.CountAsync();
        ViewBag.Views7Days = await _db.RoomViews.CountAsync(v => v.ViewedAt >= DateTime.Now.AddDays(-7));
        return View();
    }

    public async Task<IActionResult> Rooms(string? status)
    {
        var q = _db.Rooms.Include(r => r.Category).Include(r => r.LocationCategory).Include(r => r.RoomTypeCategory).Include(r => r.Landlord).Include(r => r.Images).AsQueryable();
        if (!string.IsNullOrEmpty(status)) q = q.Where(r => r.Status == status);
        ViewBag.Status = status;
        return View(await q.OrderByDescending(r => r.CreatedAt).ToListAsync());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var room = await _db.Rooms.FindAsync(id);
        if (room == null) return NotFound();
        var hasImage = !string.IsNullOrWhiteSpace(room.ImageUrl) || await _db.RoomImages.AnyAsync(i => i.RoomId == id);
        if (!hasImage) { TempData["Error"] = "Không thể duyệt tin chưa có ảnh phòng."; return RedirectToAction(nameof(Rooms), new { status = "Pending" }); }
        room.Status = "Approved"; room.RejectionReason = null;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã duyệt tin đăng.";
        return RedirectToAction(nameof(Rooms));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string reason)
    {
        var room = await _db.Rooms.FindAsync(id);
        if (room == null) return NotFound();
        if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length > 500)
        {
            TempData["Error"] = "Vui lòng nhập lý do từ chối (tối đa 500 ký tự).";
            return RedirectToAction(nameof(Rooms), new { status = "Pending" });
        }
        room.Status = "Rejected"; room.RejectionReason = reason.Trim();
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã từ chối tin đăng.";
        return RedirectToAction(nameof(Rooms));
    }

    public async Task<IActionResult> Users()
        => View(await _db.Users.OrderByDescending(u => u.CreatedAt).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUser(string id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        if (user.Role == "Admin") { TempData["Error"] = "Không thể khóa tài khoản Admin."; return RedirectToAction(nameof(Users)); }
        user.Status = user.Status == "Active" ? "Locked" : "Active";
        await _db.SaveChangesAsync();
        TempData["Success"] = user.Status == "Locked" ? "Đã khóa tài khoản." : "Đã mở khóa tài khoản.";
        return RedirectToAction(nameof(Users));
    }

    public async Task<IActionResult> Categories()
        => View(await _db.Categories.OrderBy(c => c.Type).ThenBy(c => c.CategoryName).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCategory(string categoryName, string type)
    {
        if (string.IsNullOrWhiteSpace(categoryName) || categoryName.Trim().Length > 150 || type is not ("Location" or "RoomType"))
            TempData["Error"] = "Dữ liệu danh mục không hợp lệ.";
        else
        {
            _db.Categories.Add(new Category { CategoryName = categoryName.Trim(), Type = type });
            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã thêm danh mục.";
        }
        return RedirectToAction(nameof(Categories));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var cat = await _db.Categories.FindAsync(id);
        if (cat == null) return NotFound();
        if (await _db.Rooms.AnyAsync(r => r.CategoryId == id || r.LocationCategoryId == id || r.RoomTypeCategoryId == id)) TempData["Error"] = "Không thể xóa danh mục đang được phòng trọ sử dụng.";
        else { _db.Categories.Remove(cat); await _db.SaveChangesAsync(); TempData["Success"] = "Đã xóa danh mục."; }
        return RedirectToAction(nameof(Categories));
    }
}
