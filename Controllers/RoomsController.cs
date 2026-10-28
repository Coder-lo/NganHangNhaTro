using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NganHangNhaTro.Data;
using NganHangNhaTro.Models;

namespace NganHangNhaTro.Controllers;

public class RoomsController : Controller
{
    private readonly AppDbContext _db;
    public RoomsController(AppDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? q, int? categoryId, decimal? minPrice, decimal? maxPrice, decimal? minArea, decimal? maxArea, string? sort, bool ajax = false)
    {
        var query = _db.Rooms.Include(r => r.LocationCategory).Include(r => r.RoomTypeCategory).Include(r => r.Landlord)
            .Where(r => r.ModerationStatus == "Approved" && r.RentalStatus == "Available");
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(r => r.Title.Contains(q) || r.Address.Contains(q) || (r.Description != null && r.Description.Contains(q)));
        if (categoryId.HasValue) query = query.Where(r => r.LocationCategoryId == categoryId.Value || r.RoomTypeCategoryId == categoryId.Value);
        if (minPrice.HasValue) query = query.Where(r => r.Price >= minPrice.Value);
        if (maxPrice.HasValue) query = query.Where(r => r.Price <= maxPrice.Value);
        if (minArea.HasValue) query = query.Where(r => r.Area >= minArea.Value);
        if (maxArea.HasValue) query = query.Where(r => r.Area <= maxArea.Value);
        query = sort switch { "price_asc" => query.OrderBy(r => r.Price), "price_desc" => query.OrderByDescending(r => r.Price), "area_desc" => query.OrderByDescending(r => r.Area), _ => query.OrderByDescending(r => r.CreatedAt) };
        var rooms = await query.ToListAsync();
        ViewBag.Categories = await _db.Categories.Where(c => c.IsActive).OrderBy(c => c.CategoryName).ToListAsync();
        ViewBag.Query = q; ViewBag.CategoryId = categoryId; ViewBag.MinPrice = minPrice; ViewBag.MaxPrice = maxPrice;
        ViewBag.MinArea = minArea; ViewBag.MaxArea = maxArea; ViewBag.Sort = sort;
        if (ajax) return PartialView("_RoomCards", rooms);
        return View(rooms);
    }

    public async Task<IActionResult> Details(int id)
    {
        var room = await _db.Rooms.Include(r => r.LocationCategory).Include(r => r.RoomTypeCategory).Include(r => r.Landlord).Include(r => r.Images)
            .FirstOrDefaultAsync(r => r.RoomId == id && r.ModerationStatus == "Approved" && r.RentalStatus == "Available");
        if (room == null) return NotFound();
        _db.RoomViews.Add(new RoomView { RoomId = room.RoomId, ViewedAt = DateTime.Now });
        await _db.SaveChangesAsync();
        ViewBag.IsFavorite = false;
        if (User.Identity?.IsAuthenticated == true && User.IsInRole("Tenant"))
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewBag.IsFavorite = await _db.Favorites.AnyAsync(f => f.RoomId == id && f.TenantId == uid);
        }
        return View(room);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavorite(int roomId)
    {
        if (!User.Identity!.IsAuthenticated) return Unauthorized(new { message = "Vui lòng đăng nhập." });
        if (!User.IsInRole("Tenant")) return Forbid();
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var existing = await _db.Favorites.FirstOrDefaultAsync(f => f.RoomId == roomId && f.TenantId == uid);
        if (existing != null) { _db.Favorites.Remove(existing); await _db.SaveChangesAsync(); return Json(new { success = true, isFavorite = false, message = "Đã bỏ yêu thích." }); }
        if (!await _db.Rooms.AnyAsync(r => r.RoomId == roomId && r.ModerationStatus == "Approved" && r.RentalStatus == "Available")) return NotFound();
        _db.Favorites.Add(new Favorite { TenantId = uid, RoomId = roomId, CreatedAt = DateTime.Now });
        await _db.SaveChangesAsync();
        return Json(new { success = true, isFavorite = true, message = "Đã thêm vào yêu thích." });
    }
}
