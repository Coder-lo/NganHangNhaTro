
using System.ComponentModel.DataAnnotations;

namespace NganHangNhaTro.Models;

public class Room
{
    public int RoomId { get; set; }

    [Required, StringLength(250)]
    public string Title { get; set; } = "";

    [Range(0, 1000000000)]
    public decimal Price { get; set; }

    [Range(0.1, 100000)]
    public decimal Area { get; set; }

    [Required, StringLength(300)]
    public string Address { get; set; } = "";

    public string? Description { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public decimal ElectricityPrice { get; set; }
    public decimal WaterPrice { get; set; }
    public decimal ServiceFee { get; set; }

    [StringLength(500)]
    public string? Amenities { get; set; }

    public int LocationCategoryId { get; set; }
    public int RoomTypeCategoryId { get; set; }

    [Required, StringLength(450)]
    public string LandlordId { get; set; } = "";

    [Required, StringLength(30)]
    public string ModerationStatus { get; set; } = "Pending";

    [Required, StringLength(30)]
    public string RentalStatus { get; set; } = "Available";

    [StringLength(500)]
    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    public Category? LocationCategory { get; set; }
    public Category? RoomTypeCategory { get; set; }
    public AppUser? Landlord { get; set; }

    public ICollection<RoomImage> Images { get; set; } = new List<RoomImage>();
    public ICollection<RoomView> Views { get; set; } = new List<RoomView>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
}
