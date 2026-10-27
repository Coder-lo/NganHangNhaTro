using System.ComponentModel.DataAnnotations;

namespace NganHangNhaTro.Models;

public class Room
{
    public int RoomId { get; set; }
    [Required, StringLength(250)] public string Title { get; set; } = "";
    [Range(0, 1000000000)] public decimal Price { get; set; }
    [Range(0, 1000000000)] public decimal ElectricityPrice { get; set; }
    [Range(0, 1000000000)] public decimal WaterPrice { get; set; }
    [Range(0, 1000000000)] public decimal ServiceFee { get; set; }
    [Range(0.1, 100000)] public double Area { get; set; }
    [Required, StringLength(300)] public string Address { get; set; } = "";
    [StringLength(500)] public string? Amenities { get; set; }
    public string? Description { get; set; }
    [StringLength(500)] public string? ImageUrl { get; set; }
    public string Status { get; set; } = "Pending";
    [StringLength(500)] public string? RejectionReason { get; set; }
    [Required] public int CategoryId { get; set; }
    public int? LocationCategoryId { get; set; }
    public int? RoomTypeCategoryId { get; set; }
    [Required] public string LandlordId { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public Category? Category { get; set; }
    public Category? LocationCategory { get; set; }
    public Category? RoomTypeCategory { get; set; }
    public AppUser? Landlord { get; set; }
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Contact> Contacts { get; set; } = new List<Contact>();
    public ICollection<RoomImage> Images { get; set; } = new List<RoomImage>();
}
