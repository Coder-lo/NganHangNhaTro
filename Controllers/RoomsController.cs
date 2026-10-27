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

    public async Task<IActionResult> Index(string? q, int? categoryId, int? locationCategoryId, int? roomTypeCategoryId, decimal? minPrice, decimal? maxPrice, double? minArea, double? maxArea, string? amenity, string? sort, bool ajax = false)
    {
        var query = _db.Rooms.Include(r => r.Category).Include(r => r.LocationCategory).Include(r => r.RoomTypeCategory).Include(r => r.Landlord)
            .Where(r => r.Status == "Approved");
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(r => r.Title.Contains(q) || r.Address.Contains(q) || (r.Description != null && r.Description.Contains(q)));
        if (categoryId.HasValue) query = query.Where(r => r.CategoryId == categoryId.Value || r.LocationCategoryId == categoryId.Value || r.RoomTypeCategoryId == categoryId.Value);
        if (locationCategoryId.HasValue) query = query.Where(r => r.LocationCategoryId == locationCategoryId.Value);
        if (roomTypeCategoryId.HasValue) query = query.Where(r => r.RoomTypeCategoryId == roomTypeCategoryId.Value);
        if (minPrice.HasValue) query = query.Where(r => r.Price >= minPrice.Value);
        if (maxPrice.HasValue) query = query.Where(r => r.Price <= maxPrice.Value);
        if (minArea.HasValue) query = query.Where(r => r.Area >= minArea.Value);
        if (maxArea.HasValue) query = query.Where(r => r.Area <= maxArea.Value);
        if (!string.IsNullOrWhiteSpace(amenity)) query = query.Where(r => r.Amenities != null && r.Amenities.Contains(amenity));
        query = sort switch { "price_asc" => query.OrderBy(r => r.Price), "price_desc" => query.OrderByDescending(r => r.Price), "area_desc" => query.OrderByDescending(r => r.Area), _ => query.OrderByDescending(r => r.CreatedAt) };
        var rooms = await query.ToListAsync();
        ViewBag.LocationCategories = await _db.Categories.Where(c => c.Type == "Location").OrderBy(c => c.CategoryName).ToListAsync();
        ViewBag.RoomTypeCategories = await _db.Categories.Where(c => c.Type == "RoomType").OrderBy(c => c.CategoryName).ToListAsync();
        ViewBag.Query = q; ViewBag.CategoryId = categoryId; ViewBag.LocationCategoryId = locationCategoryId; ViewBag.RoomTypeCategoryId = roomTypeCategoryId; ViewBag.MinPrice = minPrice; ViewBag.MaxPrice = maxPrice;
        ViewBag.MinArea = minArea; ViewBag.MaxArea = maxArea; ViewBag.Amenity = amenity; ViewBag.Sort = sort;
        if (ajax) return PartialView("_RoomCards", rooms);
        return View(rooms);
    }

    public async Task<IActionResult> Details(int id)
    {
        var room = await _db.Rooms.Include(r => r.Category).Include(r => r.LocationCategory).Include(r => r.RoomTypeCategory).Include(r => r.Landlord).Include(r => r.Images)
            .FirstOrDefaultAsync(r => r.RoomId == id && r.Status == "Approved");
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
        if (!await _db.Rooms.AnyAsync(r => r.RoomId == roomId && r.Status == "Approved")) return NotFound();
        _db.Favorites.Add(new Favorite { TenantId = uid, RoomId = roomId, CreatedAt = DateTime.Now });
        await _db.SaveChangesAsync();
        return Json(new { success = true, isFavorite = true, message = "Đã thêm vào yêu thích." });
    }
}
