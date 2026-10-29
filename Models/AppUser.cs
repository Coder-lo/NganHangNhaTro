using System.ComponentModel.DataAnnotations;

namespace NganHangNhaTro.Models;

public class AppUser
{
    [Key]
    [StringLength(450)]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required, StringLength(256)]
    public string UserName { get; set; } = "";

    [Required, StringLength(256), EmailAddress]
    public string Email { get; set; } = "";

    public string? PasswordHash { get; set; }

    [Required, StringLength(150)]
    public string FullName { get; set; } = "";

    [StringLength(20)]
    public string? PhoneNumber { get; set; }

    [StringLength(500)]
    public string? AvatarUrl { get; set; }

    public DateTime? DateOfBirth { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    [Required, StringLength(50)]
    public string Role { get; set; } = "Tenant";

    [Required, StringLength(50)]
    public string Status { get; set; } = "Active";

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Room> Rooms { get; set; } = new List<Room>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<Contact> SentContacts { get; set; } = new List<Contact>();
}
