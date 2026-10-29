using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NganHangNhaTro.Data;
using NganHangNhaTro.Models;
using NganHangNhaTro.ViewModels;

namespace NganHangNhaTro.Controllers;

[Authorize(Roles = "Landlord")]
public class LandlordController : Controller
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    public LandlordController(AppDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Dashboard()
    {
        var rooms = _db.Rooms
            .Where(r => r.LandlordId == UserId);

        ViewBag.Total = await rooms.CountAsync();

        ViewBag.Pending = await rooms
            .CountAsync(r => r.ModerationStatus == "Pending");

        ViewBag.Approved = await rooms
            .CountAsync(r => r.ModerationStatus == "Approved");

        ViewBag.Rented = await rooms
            .CountAsync(r => r.RentalStatus == "Rented");

        ViewBag.Contacts = await _db.Contacts
            .CountAsync(c => c.Room != null &&
                            c.Room.LandlordId == UserId);

        ViewBag.RecentRooms = await rooms
            .OrderByDescending(r => r.CreatedAt)
            .Take(5)
            .ToListAsync();

        return View();
    }

    public async Task<IActionResult> Mine()
    {
        var rooms = await _db.Rooms.Include(r => r.LocationCategory).Include(r => r.RoomTypeCategory)
            .Where(r => r.LandlordId == UserId).OrderByDescending(r => r.CreatedAt).ToListAsync();
        return View(rooms);
    }

    [HttpGet] public async Task<IActionResult> Create() { await LoadCategories(); return View(new RoomFormViewModel()); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoomFormViewModel model)
    {
        await LoadCategories();
        if (!await _db.Categories.AnyAsync(c => c.CategoryId == model.LocationCategoryId && c.Type == "Location" && c.IsActive))
            ModelState.AddModelError(nameof(model.LocationCategoryId), "Khu vực không hợp lệ.");
        if (!await _db.Categories.AnyAsync(c => c.CategoryId == model.RoomTypeCategoryId && c.Type == "RoomType" && c.IsActive))
            ModelState.AddModelError(nameof(model.RoomTypeCategoryId), "Loại phòng không hợp lệ.");
        var image = await SaveImage(model.ImageFile);
        if (model.ImageFile != null && image == null) ModelState.AddModelError(nameof(model.ImageFile), "Chỉ nhận ảnh JPG, PNG hoặc WEBP, tối đa 5MB.");
        if (!ModelState.IsValid) return View(model);
        _db.Rooms.Add(new Room {
            Title = model.Title.Trim(), Price = model.Price, Area = model.Area, Address = model.Address.Trim(),
            Description = model.Description?.Trim(), Amenities = model.Amenities?.Trim(), ImageUrl = image,
            ElectricityPrice = model.ElectricityPrice, WaterPrice = model.WaterPrice, ServiceFee = model.ServiceFee,
            LocationCategoryId = model.LocationCategoryId, RoomTypeCategoryId = model.RoomTypeCategoryId,
            LandlordId = UserId, ModerationStatus = "Pending", RentalStatus = "Available", CreatedAt = DateTime.Now
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã gửi tin đăng. Tin sẽ hiển thị sau khi Admin duyệt.";
        return RedirectToAction(nameof(Mine));
    }

    [HttpGet] public async Task<IActionResult> Edit(int id)
    {
        var room = await _db.Rooms.FirstOrDefaultAsync(r => r.RoomId == id && r.LandlordId == UserId);
        if (room == null) return NotFound();
        await LoadCategories();
        return View(new RoomFormViewModel {
            RoomId = room.RoomId, Title = room.Title, Price = room.Price, Area = room.Area, Address = room.Address,
            Description = room.Description, Amenities = room.Amenities, ElectricityPrice = room.ElectricityPrice,
            WaterPrice = room.WaterPrice, ServiceFee = room.ServiceFee, ExistingImageUrl = room.ImageUrl,
            LocationCategoryId = room.LocationCategoryId, RoomTypeCategoryId = room.RoomTypeCategoryId
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(RoomFormViewModel model)
    {
        var room = await _db.Rooms.FirstOrDefaultAsync(r => r.RoomId == model.RoomId && r.LandlordId == UserId);
        if (room == null) return NotFound();
        await LoadCategories();
        if (!await _db.Categories.AnyAsync(c => c.CategoryId == model.LocationCategoryId && c.Type == "Location" && c.IsActive))
            ModelState.AddModelError(nameof(model.LocationCategoryId), "Khu vực không hợp lệ.");
        if (!await _db.Categories.AnyAsync(c => c.CategoryId == model.RoomTypeCategoryId && c.Type == "RoomType" && c.IsActive))
            ModelState.AddModelError(nameof(model.RoomTypeCategoryId), "Loại phòng không hợp lệ.");
        var image = await SaveImage(model.ImageFile);
        if (model.ImageFile != null && image == null) ModelState.AddModelError(nameof(model.ImageFile), "Chỉ nhận ảnh JPG, PNG hoặc WEBP, tối đa 5MB.");
        if (!ModelState.IsValid) { model.ExistingImageUrl = room.ImageUrl; return View(model); }
        room.Title = model.Title.Trim(); room.Price = model.Price; room.Area = model.Area; room.Address = model.Address.Trim();
        room.Description = model.Description?.Trim(); room.Amenities = model.Amenities?.Trim();
        room.ElectricityPrice = model.ElectricityPrice; room.WaterPrice = model.WaterPrice; room.ServiceFee = model.ServiceFee;
        room.LocationCategoryId = model.LocationCategoryId; room.RoomTypeCategoryId = model.RoomTypeCategoryId;
        if (image != null) room.ImageUrl = image;
        room.ModerationStatus = "Pending"; room.RejectionReason = null; room.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật tin. Tin được chuyển về trạng thái chờ duyệt.";
        return RedirectToAction(nameof(Mine));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var room = await _db.Rooms.FirstOrDefaultAsync(r => r.RoomId == id && r.LandlordId == UserId);
        if (room == null) return NotFound();
        _db.Rooms.Remove(room); await _db.SaveChangesAsync();
        TempData["Success"] = "Đã xóa tin đăng.";
        return RedirectToAction(nameof(Mine));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleRented(int id)
    {
        var room = await _db.Rooms.FirstOrDefaultAsync(r => r.RoomId == id && r.LandlordId == UserId && r.ModerationStatus == "Approved");
        if (room == null) return NotFound();
        room.RentalStatus = room.RentalStatus == "Rented" ? "Available" : "Rented";
        room.UpdatedAt = DateTime.Now;
        await _db.SaveChangesAsync();
        return Json(new { success = true, status = room.RentalStatus, label = room.RentalStatus == "Rented" ? "Đã cho thuê" : "Còn trống" });
    }

    public async Task<IActionResult> Contacts()
    {
        var contacts = await _db.Contacts.Include(c => c.Room).Where(c => c.Room!.LandlordId == UserId)
            .OrderByDescending(c => c.CreatedAt).ToListAsync();
        return View(contacts);
    }

    private async Task LoadCategories()
    {
        ViewBag.LocationCategories = await _db.Categories.Where(c => c.Type == "Location" && c.IsActive).OrderBy(c => c.CategoryName).ToListAsync();
        ViewBag.RoomTypeCategories = await _db.Categories.Where(c => c.Type == "RoomType" && c.IsActive).OrderBy(c => c.CategoryName).ToListAsync();
    }

    private async Task<string?> SaveImage(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;
        if (file.Length > 5 * 1024 * 1024) return null;
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(ext)) return null;
        var folder = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(folder);
        var name = $"{Guid.NewGuid():N}{ext}";
        await using var stream = System.IO.File.Create(Path.Combine(folder, name));
        await file.CopyToAsync(stream);
        return "/uploads/" + name;
    }
}
