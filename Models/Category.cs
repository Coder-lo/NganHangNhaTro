using System.ComponentModel.DataAnnotations;

namespace NganHangNhaTro.Models;

public class Category
{
    public int CategoryId { get; set; }

    [Required, StringLength(150)]
    public string CategoryName { get; set; } = "";

    [Required, StringLength(50)]
    public string Type { get; set; } = "RoomType";

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Room> LocationRooms { get; set; } = new List<Room>();
    public ICollection<Room> RoomTypeRooms { get; set; } = new List<Room>();
}
