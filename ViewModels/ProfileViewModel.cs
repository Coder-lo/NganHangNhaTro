using System.ComponentModel.DataAnnotations;
namespace NganHangNhaTro.ViewModels;
public class ProfileViewModel
{
    [Required, StringLength(150)] public string FullName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Phone, StringLength(20)] public string? PhoneNumber { get; set; }
    [DataType(DataType.Date)] public DateTime? DateOfBirth { get; set; }
    [StringLength(300)] public string? Address { get; set; }
    public string? AvatarUrl { get; set; }
    public IFormFile? AvatarFile { get; set; }
}
