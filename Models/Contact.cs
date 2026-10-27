using System.ComponentModel.DataAnnotations;

namespace NganHangNhaTro.Models;

public class Contact
{
    public int ContactId { get; set; }
    public int RoomId { get; set; }
    [Required, StringLength(150)] public string SenderName { get; set; } = "";
    [Required, StringLength(20)] public string SenderPhone { get; set; } = "";
    [StringLength(256)] public string? SenderEmail { get; set; }
    [StringLength(450)] public string? SenderUserId { get; set; }
    [StringLength(500)] public string? Message { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public Room? Room { get; set; }
    public AppUser? SenderUser { get; set; }
}
