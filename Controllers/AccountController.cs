using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NganHangNhaTro.Data;
using NganHangNhaTro.Models;
using NganHangNhaTro.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace NganHangNhaTro.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<AppUser> _hasher;
    public AccountController(AppDbContext db, IPasswordHasher<AppUser> hasher) { _db = db; _hasher = hasher; }

    [HttpGet] public IActionResult Register() => View();
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (model.Role is not ("Tenant" or "Landlord")) ModelState.AddModelError(nameof(model.Role), "Vai trò không hợp lệ.");
        if (!ModelState.IsValid) return View(model);
        var email = model.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email || u.UserName == email))
        {
            ModelState.AddModelError(nameof(model.Email), "Email đã được sử dụng.");
            return View(model);
        }
        var user = new AppUser { Id = Guid.NewGuid().ToString(), UserName = email, Email = email, FullName = model.FullName.Trim(), Role = model.Role, PhoneNumber = model.PhoneNumber?.Trim(), Status = "Active", CreatedAt = DateTime.Now };
        user.PasswordHash = _hasher.HashPassword(user, model.Password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đăng ký thành công. Hãy đăng nhập.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet] public IActionResult Login(string? returnUrl = null) { ViewBag.ReturnUrl = returnUrl; return View(); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        if (!ModelState.IsValid) return View(model);
        var email = model.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email || u.UserName == email);
        if (user == null || user.Status != "Active" || string.IsNullOrWhiteSpace(user.PasswordHash))
        {
            ModelState.AddModelError("", "Email, mật khẩu không đúng hoặc tài khoản đã bị khóa.");
            return View(model);
        }
        var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
        if (verify == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
            return View(model);
        }
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id), new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email ?? ""), new(ClaimTypes.Role, user.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
        return user.Role switch
        {
            "Admin" => RedirectToAction("Dashboard", "Admin"),
            "Landlord" => RedirectToAction("Dashboard", "Landlord"),
            _ => RedirectToAction("Index", "Rooms")
        };
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Rooms");
    }


    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        return View(new ProfileViewModel { FullName = user.FullName, Email = user.Email ?? "", PhoneNumber = user.PhoneNumber, DateOfBirth = user.DateOfBirth, Address = user.Address, AvatarUrl = user.AvatarUrl });
    }

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _db.Users.FindAsync(id);
        if (user == null) return NotFound();
        var email = model.Email?.Trim().ToLowerInvariant() ?? "";
        if (!string.IsNullOrWhiteSpace(email) && await _db.Users.AnyAsync(u => u.Id != id && (u.Email == email || u.UserName == email)))
            ModelState.AddModelError(nameof(model.Email), "Email đã được sử dụng.");
        string? avatar = null;
        if (model.AvatarFile is { Length: > 0 })
        {
            if (model.AvatarFile.Length > 5 * 1024 * 1024) ModelState.AddModelError(nameof(model.AvatarFile), "Ảnh tối đa 5MB.");
            else if (!new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(Path.GetExtension(model.AvatarFile.FileName).ToLowerInvariant()) || !new[] { "image/jpeg", "image/png", "image/webp" }.Contains(model.AvatarFile.ContentType.ToLowerInvariant())) ModelState.AddModelError(nameof(model.AvatarFile), "Chỉ nhận ảnh JPG, PNG, WEBP hợp lệ.");
            else { var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars"); Directory.CreateDirectory(folder); var ext = Path.GetExtension(model.AvatarFile.FileName).ToLowerInvariant(); var name = Guid.NewGuid().ToString("N") + ext; await using var stream = System.IO.File.Create(Path.Combine(folder, name)); await model.AvatarFile.CopyToAsync(stream); avatar = "/uploads/avatars/" + name; }
        }
        if (!ModelState.IsValid) { model.AvatarUrl = user.AvatarUrl; return View(model); }
        user.FullName = model.FullName.Trim(); user.Email = email; user.UserName = email; user.PhoneNumber = model.PhoneNumber?.Trim(); user.DateOfBirth = model.DateOfBirth; user.Address = model.Address?.Trim(); if (avatar != null) user.AvatarUrl = avatar;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật thông tin cá nhân.";
        return RedirectToAction(nameof(Profile));
    }


    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

    [Authorize]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var user = await _db.Users.FindAsync(id);
        if (user == null || string.IsNullOrWhiteSpace(user.PasswordHash)) return NotFound();
        if (_hasher.VerifyHashedPassword(user, user.PasswordHash, model.CurrentPassword) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(nameof(model.CurrentPassword), "Mật khẩu hiện tại không chính xác.");
            return View(model);
        }
        user.PasswordHash = _hasher.HashPassword(user, model.NewPassword);
        await _db.SaveChangesAsync();
        TempData["Success"] = "Đổi mật khẩu thành công.";
        return RedirectToAction(nameof(Profile));
    }

    public IActionResult AccessDenied() => View();
}
