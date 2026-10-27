using System.ComponentModel.DataAnnotations;

namespace NganHangNhaTro.Models;

public class RoomImage
{
    public int RoomImageId { get; set; }
    public int RoomId { get; set; }
    [Required, StringLength(500)] public string ImageUrl { get; set; } = "";
    public int DisplayOrder { get; set; }
    public Room? Room { get; set; }
}
