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
        if (!db.Users.Any(u => u.Role == "Admin"))
        {
            var admin = new AppUser
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
    }
    catch (Exception ex)
    {
        app.Logger.LogWarning(ex, "Không thể tạo tài khoản admin mặc định. Hãy chạy NhaTroDB.sql và kiểm tra connection string.");
    }
    
    // Seed tài khoản chủ trọ và phòng mẫu
    try
    {
        const string landlordEmail = "chutro@nht.local";

        var landlord = db.Users.FirstOrDefault(u => u.Email == landlordEmail);

        if (landlord == null)
        {
            landlord = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = landlordEmail,
                Email = landlordEmail,
                FullName = "Chủ trọ mẫu",
                Role = "Landlord",
                Status = "Active",
                CreatedAt = DateTime.Now
            };

            landlord.PasswordHash = hasher.HashPassword(
                landlord, "Chutro@123");

            db.Users.Add(landlord);
            db.SaveChanges();
        }

        // Chỉ tạo phòng nếu bảng Rooms chưa có dữ liệu
        if (!db.Rooms.Any())
        {
            var locations = db.Categories
                .Where(c => c.Type == "Location")
                .OrderBy(c => c.CategoryId)
                .ToList();

            var roomTypes = db.Categories
                .Where(c => c.Type == "RoomType")
                .OrderBy(c => c.CategoryId)
                .ToList();

            var generalCategory = roomTypes.FirstOrDefault();

            if (locations.Count == 0 || roomTypes.Count == 0
                || generalCategory == null)
            {
                app.Logger.LogWarning(
                    "Chưa đủ danh mục để tạo phòng mẫu. " +
                    "Hãy kiểm tra Categories và các giá trị Type.");
            }
            else
            {
                var roomData = new[]
                    {
                        new { Title = "Phòng trọ bình dân tại Cầu Giấy",
                            Price = 2500000m, Area = 22d, Location = 0, Type = 0 },

                        new { Title = "Phòng trọ khép kín tại Cầu Giấy",
                            Price = 3200000m, Area = 25d, Location = 0, Type = 1 },

                        new { Title = "Căn hộ mini tại Đống Đa",
                            Price = 5500000m, Area = 35d, Location = 1, Type = 2 },

                        new { Title = "Phòng trọ bình dân tại Thanh Xuân",
                            Price = 2200000m, Area = 20d, Location = 2, Type = 0 },

                        new { Title = "Phòng trọ khép kín tại Đống Đa",
                            Price = 3500000m, Area = 28d, Location = 1, Type = 1 },

                        new { Title = "Căn hộ mini tại Cầu Giấy",
                            Price = 5000000m, Area = 32d, Location = 0, Type = 2 }
                    };

                var rooms = roomData.Select((item, index) => new Room
                {
                    Title = item.Title,
                    Price = item.Price,
                    ElectricityPrice = 3500m,
                    WaterPrice = 100000m,
                    ServiceFee = 150000m,
                    Area = item.Area,
                    Address = $"Khu vực {locations[item.Location % locations.Count].CategoryName}, Hà Nội",
                    Amenities = "Điều hòa, nóng lạnh, wifi",
                    Description = "Phòng sạch sẽ, an ninh tốt, phù hợp cho sinh viên và người đi làm.",
                    ImageUrl = new[]
                        {
                            "https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?auto=format&fit=crop&w=1200&q=85",
                            "https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?auto=format&fit=crop&w=1200&q=85",
                            "https://images.unsplash.com/photo-1493809842364-78817add7ffb?auto=format&fit=crop&w=1200&q=85",
                            "https://images.unsplash.com/photo-1502672260266-1c1ef2d93688?auto=format&fit=crop&w=1200&q=85",
                            "https://images.unsplash.com/photo-1484154218962-a197022b5858?auto=format&fit=crop&w=1200&q=85",
                            "https://images.unsplash.com/photo-1497366754035-f200968a6e72?auto=format&fit=crop&w=1200&q=85"
                        }[index],
                    Status = "Approved",
                    CategoryId = generalCategory.CategoryId,
                    LocationCategoryId = locations[item.Location % locations.Count].CategoryId,
                    RoomTypeCategoryId = roomTypes[item.Type % roomTypes.Count].CategoryId,
                    LandlordId = landlord.Id,
                    CreatedAt = DateTime.Now.AddMinutes(-index)
                }).ToList();

                db.Rooms.AddRange(rooms);
                db.SaveChanges();

                app.Logger.LogInformation(
                    "Đã tạo {Count} phòng mẫu.", rooms.Count);
            }
        }
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex,
            "Không thể tạo tài khoản chủ trọ hoặc phòng mẫu.");
    }

}
app.MapControllerRoute(name: "default", pattern: "{controller=Rooms}/{action=Index}/{id?}");
app.Run();
