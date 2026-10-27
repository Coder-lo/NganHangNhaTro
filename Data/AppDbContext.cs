using Microsoft.EntityFrameworkCore;
using NganHangNhaTro.Models;

namespace NganHangNhaTro.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<RoomImage> RoomImages => Set<RoomImage>();
    public DbSet<RoomView> RoomViews => Set<RoomView>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>().ToTable("AspNetUsers");
        modelBuilder.Entity<AppUser>().HasKey(x => x.Id);
        modelBuilder.Entity<AppUser>().Property(x => x.Id).HasMaxLength(450);
        modelBuilder.Entity<AppUser>().Property(x => x.UserName).HasMaxLength(256);
        modelBuilder.Entity<AppUser>().Property(x => x.Email).HasMaxLength(256);
        modelBuilder.Entity<AppUser>().Property(x => x.FullName).HasMaxLength(150).IsRequired();
        modelBuilder.Entity<AppUser>().Property(x => x.PhoneNumber).HasMaxLength(20);
        modelBuilder.Entity<AppUser>().Property(x => x.AvatarUrl).HasMaxLength(500);
        modelBuilder.Entity<AppUser>().Property(x => x.Address).HasMaxLength(300);
        modelBuilder.Entity<AppUser>().Property(x => x.Role).HasMaxLength(50).IsRequired();
        modelBuilder.Entity<AppUser>().Property(x => x.Status).HasMaxLength(50).IsRequired();
        modelBuilder.Entity<AppUser>().Property(x => x.CreatedAt).HasColumnType("datetime");

        modelBuilder.Entity<Category>().ToTable("Categories");
        modelBuilder.Entity<Category>().HasKey(x => x.CategoryId);
        modelBuilder.Entity<Category>().Property(x => x.CategoryName).HasMaxLength(150).IsRequired();
        modelBuilder.Entity<Category>().Property(x => x.Type).HasMaxLength(50).IsRequired();

        modelBuilder.Entity<Room>().ToTable("Rooms");
        modelBuilder.Entity<Room>().HasKey(x => x.RoomId);
        modelBuilder.Entity<Room>().Property(x => x.Title).HasMaxLength(250).IsRequired();
        modelBuilder.Entity<Room>().Property(x => x.Price).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Room>().Property(x => x.ElectricityPrice).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Room>().Property(x => x.WaterPrice).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Room>().Property(x => x.ServiceFee).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Room>().Property(x => x.Address).HasMaxLength(300).IsRequired();
        modelBuilder.Entity<Room>().Property(x => x.Amenities).HasMaxLength(500);
        modelBuilder.Entity<Room>().Property(x => x.ImageUrl).HasMaxLength(500);
        modelBuilder.Entity<Room>().Property(x => x.Status).HasMaxLength(50).IsRequired();
        modelBuilder.Entity<Room>().Property(x => x.RejectionReason).HasMaxLength(500);
        modelBuilder.Entity<Room>().Property(x => x.CreatedAt).HasColumnType("datetime");
        modelBuilder.Entity<Room>().HasOne(x => x.Category).WithMany(x => x.Rooms)
            .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Room>().HasOne(x => x.LocationCategory).WithMany()
            .HasForeignKey(x => x.LocationCategoryId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Room>().HasOne(x => x.RoomTypeCategory).WithMany()
            .HasForeignKey(x => x.RoomTypeCategoryId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Room>().HasOne(x => x.Landlord).WithMany(x => x.Rooms)
            .HasForeignKey(x => x.LandlordId).OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Favorite>().ToTable("Favorites");
        modelBuilder.Entity<Favorite>().HasKey(x => x.FavoriteId);
        modelBuilder.Entity<Favorite>().Property(x => x.TenantId).HasMaxLength(450).IsRequired();
        modelBuilder.Entity<Favorite>().HasIndex(x => new { x.TenantId, x.RoomId }).IsUnique();
        modelBuilder.Entity<Favorite>().HasOne(x => x.Tenant).WithMany(x => x.Favorites)
            .HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Favorite>().HasOne(x => x.Room).WithMany(x => x.Favorites)
            .HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Contact>().ToTable("Contacts");
        modelBuilder.Entity<Contact>().HasKey(x => x.ContactId);
        modelBuilder.Entity<Contact>().Property(x => x.SenderName).HasMaxLength(150).IsRequired();
        modelBuilder.Entity<Contact>().Property(x => x.SenderPhone).HasMaxLength(20).IsRequired();
        modelBuilder.Entity<Contact>().Property(x => x.SenderEmail).HasMaxLength(256);
        modelBuilder.Entity<Contact>().Property(x => x.SenderUserId).HasMaxLength(450);
        modelBuilder.Entity<Contact>().Property(x => x.Message).HasMaxLength(500);
        modelBuilder.Entity<Contact>().Property(x => x.CreatedAt).HasColumnType("datetime");
        modelBuilder.Entity<Contact>().HasOne(x => x.Room).WithMany(x => x.Contacts)
            .HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<Contact>().HasOne(x => x.SenderUser).WithMany()
            .HasForeignKey(x => x.SenderUserId).OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<RoomImage>().ToTable("RoomImages");
        modelBuilder.Entity<RoomImage>().HasKey(x => x.RoomImageId);
        modelBuilder.Entity<RoomImage>().Property(x => x.ImageUrl).HasMaxLength(500).IsRequired();
        modelBuilder.Entity<RoomImage>().HasOne(x => x.Room).WithMany(x => x.Images)
            .HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RoomView>().ToTable("RoomViews");
        modelBuilder.Entity<RoomView>().HasKey(x => x.RoomViewId);
        modelBuilder.Entity<RoomView>().Property(x => x.ViewedAt).HasColumnType("datetime");
        modelBuilder.Entity<RoomView>().HasOne(x => x.Room).WithMany()
            .HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Cascade);
    }
}
