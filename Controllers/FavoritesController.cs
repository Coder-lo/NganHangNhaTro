using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NganHangNhaTro.Data;

namespace NganHangNhaTro.Controllers;

[Authorize(Roles = "Tenant")]
public class FavoritesController : Controller
{
    private readonly AppDbContext _db;
    public FavoritesController(AppDbContext db) => _db = db;
    public async Task<IActionResult> Index()
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var rooms = await _db.Favorites.Where(f => f.TenantId == uid)
            .Include(f => f.Room!).ThenInclude(r => r.LocationCategory)
            .Include(f => f.Room!).ThenInclude(r => r.RoomTypeCategory)
            .Include(f => f.Room!).ThenInclude(r => r.Landlord)
            .Select(f => f.Room!).Where(r => r.ModerationStatus == "Approved" && r.RentalStatus == "Available").ToListAsync();
        return View(rooms);
    }
}
