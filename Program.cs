using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NganHangNhaTro.Data;
using NganHangNhaTro.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IPasswordHasher<AppUser>, PasswordHasher<AppUser>>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });
builder.Services.AddAuthorization();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Seed an initial administrator if the database has no admin account.
// Change this password immediately after first login.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();
    try
    {
        var admin = db.Users.FirstOrDefault(u => u.Role == "Admin");
        if (admin == null)
        {
            admin = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "admin@nht.local",
                Email = "admin@nht.local",
                FullName = "Quản trị viên",
                Role = "Admin",
                Status = "Active",
                CreatedAt = DateTime.Now
            };

            admin.PasswordHash = hasher.HashPassword(admin, "Admin@12345");
            db.Users.Add(admin);
            db.SaveChanges();
        }
        else if (string.IsNullOrWhiteSpace(admin.PasswordHash))
        {
            admin.PasswordHash = hasher.HashPassword(admin, "Admin@12345");
            db.SaveChanges();
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Không thể tạo tài khoản admin mặc định. Hãy chạy NhaTroDB.sql và kiểm tra connection string.");
    }
}
app.MapControllerRoute(name: "default", pattern: "{controller=Rooms}/{action=Index}/{id?}");
app.Run();
