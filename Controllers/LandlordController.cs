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
        var owned = _db.Rooms.Where(r => r.LandlordId == UserId);
        ViewBag.Total = await owned.CountAsync();
        ViewBag.Pending = await owned.CountAsync(r => r.Status == "Pending");
        ViewBag.Approved = await owned.CountAsync(r => r.Status == "Approved");
        ViewBag.Rented = await owned.CountAsync(r => r.Status == "Rented");
        ViewBag.Contacts = await _db.Contacts.CountAsync(c => c.Room!.LandlordId == UserId);
        ViewBag.RecentRooms = await owned.OrderByDescending(r => r.CreatedAt).Take(5).ToListAsync();
        return View();
    }

    public async Task<IActionResult> Mine()
    {
        var rooms = await _db.Rooms.Include(r => r.Category).Include(r => r.Images).Where(r => r.LandlordId == UserId).OrderByDescending(r => r.CreatedAt).ToListAsync();
        return View(rooms);
    }

    [HttpGet] public async Task<IActionResult> Create() { await LoadCategories(); return View(new RoomFormViewModel()); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(RoomFormViewModel model)
    {
        await LoadCategories();
        var image = await SaveImage(model.ImageFile);
        if (model.ImageFile != null && image == null) ModelState.AddModelError(nameof(model.ImageFile), "Chỉ nhận ảnh JPG, PNG hoặc WEBP, tối đa 5MB.");
        var extraFiles = model.ImageFiles ?? new List<IFormFile>();
        if (image == null && !extraFiles.Any(f => f.Length > 0 && f.Length <= 5 * 1024 * 1024 && new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(Path.GetExtension(f.FileName).ToLowerInvariant()) && new[] { "image/jpeg", "image/png", "image/webp" }.Contains(f.ContentType.ToLowerInvariant())))
            ModelState.AddModelError(nameof(model.ImageFile), "Vui lòng tải lên ít nhất một ảnh phòng hợp lệ.");
        if (extraFiles.Any(f => f.Length > 0 && (f.Length > 5 * 1024 * 1024 || !new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(Path.GetExtension(f.FileName).ToLowerInvariant()) || !new[] { "image/jpeg", "image/png", "image/webp" }.Contains(f.ContentType.ToLowerInvariant()))))
            ModelState.AddModelError(nameof(model.ImageFiles), "Mỗi ảnh phải là JPG/PNG/WEBP và tối đa 5MB.");
        if (!ModelState.IsValid) return View(model);
        if (model.LocationCategoryId.HasValue && !await _db.Categories.AnyAsync(c => c.CategoryId == model.LocationCategoryId && c.Type == "Location")) ModelState.AddModelError(nameof(model.LocationCategoryId), "Khu vực không hợp lệ.");
        if (model.RoomTypeCategoryId.HasValue && !await _db.Categories.AnyAsync(c => c.CategoryId == model.RoomTypeCategoryId && c.Type == "RoomType")) ModelState.AddModelError(nameof(model.RoomTypeCategoryId), "Loại phòng không hợp lệ.");
        if (!model.LocationCategoryId.HasValue) ModelState.AddModelError(nameof(model.LocationCategoryId), "Vui lòng chọn khu vực.");
        if (!model.RoomTypeCategoryId.HasValue) ModelState.AddModelError(nameof(model.RoomTypeCategoryId), "Vui lòng chọn loại phòng.");
        if (!ModelState.IsValid) return View(model);
        _db.Rooms.Add(new Room { Title = model.Title.Trim(), Price = model.Price, Area = model.Area, Address = model.Address.Trim(), Amenities = model.Amenities?.Trim(), Description = model.Description?.Trim(), ImageUrl = image, ElectricityPrice = model.ElectricityPrice, WaterPrice = model.WaterPrice, ServiceFee = model.ServiceFee, CategoryId = model.RoomTypeCategoryId ?? model.LocationCategoryId!.Value, LocationCategoryId = model.LocationCategoryId, RoomTypeCategoryId = model.RoomTypeCategoryId, LandlordId = UserId, Status = "Pending", CreatedAt = DateTime.Now });
        var newRoom = _db.Rooms.Local.Last();
        foreach (var file in model.ImageFiles ?? new List<IFormFile>())
        {
            var url = await SaveImage(file);
            if (url != null) newRoom.Images.Add(new RoomImage { ImageUrl = url, DisplayOrder = newRoom.Images.Count });
            else if (file.Length > 0) ModelState.AddModelError(nameof(model.ImageFiles), "Mỗi ảnh phải là JPG/PNG/WEBP và tối đa 5MB.");
        }
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã gửi tin đăng. Tin sẽ hiển thị sau khi Admin duyệt.";
        return RedirectToAction(nameof(Mine));
    }

    [HttpGet] public async Task<IActionResult> Edit(int id)
    {
        var room = await _db.Rooms.Include(r => r.Images).FirstOrDefaultAsync(r => r.RoomId == id && r.LandlordId == UserId);
        if (room == null) return NotFound();
        await LoadCategories();
        return View(new RoomFormViewModel { RoomId = room.RoomId, Title = room.Title, Price = room.Price, ElectricityPrice = room.ElectricityPrice, WaterPrice = room.WaterPrice, ServiceFee = room.ServiceFee, Area = room.Area, Address = room.Address, Description = room.Description, Amenities = room.Amenities, ExistingImageUrl = room.ImageUrl, CategoryId = room.CategoryId, LocationCategoryId = room.LocationCategoryId, RoomTypeCategoryId = room.RoomTypeCategoryId });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(RoomFormViewModel model)
    {
        var room = await _db.Rooms.Include(r => r.Images).FirstOrDefaultAsync(r => r.RoomId == model.RoomId && r.LandlordId == UserId);
        if (room == null) return NotFound();
        await LoadCategories();
        var image = await SaveImage(model.ImageFile);
        if (model.ImageFile != null && image == null) ModelState.AddModelError(nameof(model.ImageFile), "Chỉ nhận ảnh JPG, PNG hoặc WEBP, tối đa 5MB.");
        var extraFiles = model.ImageFiles ?? new List<IFormFile>();
        if (extraFiles.Any(f => f.Length > 0 && (f.Length > 5 * 1024 * 1024 || !new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(Path.GetExtension(f.FileName).ToLowerInvariant()) || !new[] { "image/jpeg", "image/png", "image/webp" }.Contains(f.ContentType.ToLowerInvariant()))))
            ModelState.AddModelError(nameof(model.ImageFiles), "Mỗi ảnh phải là JPG/PNG/WEBP và tối đa 5MB.");
        if (!ModelState.IsValid) { model.ExistingImageUrl = room.ImageUrl; return View(model); }
        room.Title = model.Title.Trim(); room.Price = model.Price; room.Area = model.Area; room.Address = model.Address.Trim();
        if (!model.LocationCategoryId.HasValue || !await _db.Categories.AnyAsync(c => c.CategoryId == model.LocationCategoryId && c.Type == "Location")) ModelState.AddModelError(nameof(model.LocationCategoryId), "Vui lòng chọn khu vực hợp lệ.");
        if (!model.RoomTypeCategoryId.HasValue || !await _db.Categories.AnyAsync(c => c.CategoryId == model.RoomTypeCategoryId && c.Type == "RoomType")) ModelState.AddModelError(nameof(model.RoomTypeCategoryId), "Vui lòng chọn loại phòng hợp lệ.");
        if (!ModelState.IsValid) { model.ExistingImageUrl = room.ImageUrl; return View(model); }
        room.Description = model.Description?.Trim(); room.Amenities = model.Amenities?.Trim(); room.CategoryId = model.RoomTypeCategoryId!.Value; room.LocationCategoryId = model.LocationCategoryId; room.RoomTypeCategoryId = model.RoomTypeCategoryId; room.ElectricityPrice = model.ElectricityPrice; room.WaterPrice = model.WaterPrice; room.ServiceFee = model.ServiceFee;
        if (image != null) room.ImageUrl = image;
        room.Status = "Pending"; room.RejectionReason = null;
        foreach (var file in model.ImageFiles ?? new List<IFormFile>())
        {
            var url = await SaveImage(file);
            if (url != null) room.Images.Add(new RoomImage { ImageUrl = url, DisplayOrder = room.Images.Count });
            else if (file.Length > 0) ModelState.AddModelError(nameof(model.ImageFiles), "Mỗi ảnh phải là JPG/PNG/WEBP và tối đa 5MB.");
        }
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
        var room = await _db.Rooms.FirstOrDefaultAsync(
            r => r.RoomId == id &&
            r.LandlordId == UserId &&
            (r.Status == "Approved" || r.Status == "Rented"));
        if (room == null) return NotFound();
        room.Status = room.Status == "Rented" ? "Approved" : "Rented";
        await _db.SaveChangesAsync();
        return Json(new { success = true, status = room.Status, label = room.Status == "Rented" ? "Đã cho thuê" : "Còn trống" });
    }

    public async Task<IActionResult> Contacts()
    {
        var contacts = await _db.Contacts.Include(c => c.Room).Where(c => c.Room!.LandlordId == UserId).OrderByDescending(c => c.CreatedAt).ToListAsync();
        return View(contacts);
    }

    public async Task<IActionResult> ContactDetails(int id)
    {
        var contact = await _db.Contacts.Include(c => c.Room)
            .FirstOrDefaultAsync(c => c.ContactId == id && c.Room!.LandlordId == UserId);
        if (contact == null) return NotFound();
        return View(contact);
    }

    private async Task LoadCategories()
    {
        ViewBag.LocationCategories = await _db.Categories.Where(c => c.Type == "Location").OrderBy(c => c.CategoryName).ToListAsync();
        ViewBag.RoomTypeCategories = await _db.Categories.Where(c => c.Type == "RoomType").OrderBy(c => c.CategoryName).ToListAsync();
    }

    private async Task<string?> SaveImage(IFormFile? file)
    {
        if (file == null || file.Length == 0) return null;
        if (file.Length > 5 * 1024 * 1024) return null;
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var allowedMime = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowed.Contains(ext) || !allowedMime.Contains(file.ContentType.ToLowerInvariant())) return null;
        var folder = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(folder);
        var name = $"{Guid.NewGuid():N}{ext}";
        await using var stream = System.IO.File.Create(Path.Combine(folder, name));
        await file.CopyToAsync(stream);
        return "/uploads/" + name;
    }
}
