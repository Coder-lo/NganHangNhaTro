
using Microsoft.EntityFrameworkCore;
using NganHangNhaTro.Models;

namespace NganHangNhaTro.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<RoomImage> RoomImages => Set<RoomImage>();
    public DbSet<RoomView> RoomViews => Set<RoomView>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Contact> Contacts => Set<Contact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // USERS
        modelBuilder.Entity<AppUser>(e =>
        {
            e.ToTable("AspNetUsers");
            e.HasKey(x => x.Id);

            e.Property(x => x.Id).HasMaxLength(450);
            e.Property(x => x.UserName).HasMaxLength(256).IsRequired();
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.Property(x => x.PasswordHash);
            e.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            e.Property(x => x.PhoneNumber).HasMaxLength(20);
            e.Property(x => x.AvatarUrl).HasMaxLength(500);
            e.Property(x => x.DateOfBirth).HasColumnType("date");
            e.Property(x => x.Address).HasMaxLength(300);
            e.Property(x => x.Role).HasMaxLength(50).IsRequired();
            e.Property(x => x.Status).HasMaxLength(50).IsRequired();
            e.Property(x => x.CreatedAt).HasColumnType("datetime");

            e.HasIndex(x => x.UserName).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
        });

        // CATEGORIES
        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("Categories");
            e.HasKey(x => x.CategoryId);

            e.Property(x => x.CategoryName).HasMaxLength(150).IsRequired();
            e.Property(x => x.Type).HasMaxLength(50).IsRequired();
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.CreatedAt).HasColumnType("datetime");
        });

        // ROOMS
        modelBuilder.Entity<Room>(e =>
        {
            e.ToTable("Rooms");
            e.HasKey(x => x.RoomId);

            e.Property(x => x.Title).HasMaxLength(250).IsRequired();
            e.Property(x => x.Price).HasColumnType("decimal(18,2)");
            e.Property(x => x.Area).HasColumnType("decimal(10,2)");
            e.Property(x => x.Address).HasMaxLength(300).IsRequired();
            e.Property(x => x.Description);
            e.Property(x => x.ImageUrl).HasMaxLength(500);

            e.Property(x => x.ElectricityPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.WaterPrice).HasColumnType("decimal(18,2)");
            e.Property(x => x.ServiceFee).HasColumnType("decimal(18,2)");
            e.Property(x => x.Amenities).HasMaxLength(500);

            e.Property(x => x.LandlordId).HasMaxLength(450).IsRequired();
            e.Property(x => x.ModerationStatus).HasMaxLength(30).IsRequired();
            e.Property(x => x.RentalStatus).HasMaxLength(30).IsRequired();
            e.Property(x => x.RejectionReason).HasMaxLength(500);
            e.Property(x => x.CreatedAt).HasColumnType("datetime");
            e.Property(x => x.UpdatedAt).HasColumnType("datetime");

            e.HasIndex(x => new { x.ModerationStatus, x.RentalStatus });
            e.HasIndex(x => x.LocationCategoryId);
            e.HasIndex(x => x.RoomTypeCategoryId);
            e.HasIndex(x => x.LandlordId);

            e.HasOne(x => x.LocationCategory)
                .WithMany(x => x.LocationRooms)
                .HasForeignKey(x => x.LocationCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.RoomTypeCategory)
                .WithMany(x => x.RoomTypeRooms)
                .HasForeignKey(x => x.RoomTypeCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Landlord)
                .WithMany(x => x.Rooms)
                .HasForeignKey(x => x.LandlordId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ROOM IMAGES
        modelBuilder.Entity<RoomImage>(e =>
        {
            e.ToTable("RoomImages");
            e.HasKey(x => x.RoomImageId);
            e.Property(x => x.ImageUrl).HasMaxLength(500).IsRequired();

            e.HasOne(x => x.Room)
                .WithMany(x => x.Images)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ROOM VIEWS
        modelBuilder.Entity<RoomView>(e =>
        {
            e.ToTable("RoomViews");
            e.HasKey(x => x.RoomViewId);
            e.Property(x => x.ViewedAt).HasColumnType("datetime");
            e.HasIndex(x => x.ViewedAt);

            e.HasOne(x => x.Room)
                .WithMany(x => x.Views)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // FAVORITES
        modelBuilder.Entity<Favorite>(e =>
        {
            e.ToTable("Favorites");
            e.HasKey(x => x.FavoriteId);
            e.Property(x => x.TenantId).HasMaxLength(450).IsRequired();
            e.Property(x => x.CreatedAt).HasColumnType("datetime");

            e.HasIndex(x => x.TenantId);
            e.HasIndex(x => new { x.TenantId, x.RoomId }).IsUnique();

            e.HasOne(x => x.Tenant)
                .WithMany(x => x.Favorites)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Room)
                .WithMany(x => x.Favorites)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // CONTACTS
        modelBuilder.Entity<Contact>(e =>
        {
            e.ToTable("Contacts");
            e.HasKey(x => x.ContactId);

            e.Property(x => x.SenderName).HasMaxLength(150).IsRequired();
            e.Property(x => x.SenderPhone).HasMaxLength(20).IsRequired();
            e.Property(x => x.SenderEmail).HasMaxLength(256);
            e.Property(x => x.SenderUserId).HasMaxLength(450);
            e.Property(x => x.Message).HasMaxLength(500);
            e.Property(x => x.CreatedAt).HasColumnType("datetime");

            e.HasIndex(x => x.RoomId);

            e.HasOne(x => x.Room)
                .WithMany(x => x.Contacts)
                .HasForeignKey(x => x.RoomId)
                .OnDelete(DeleteBehavior.NoAction);

            e.HasOne(x => x.Sender)
                .WithMany(x => x.SentContacts)
                .HasForeignKey(x => x.SenderUserId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
