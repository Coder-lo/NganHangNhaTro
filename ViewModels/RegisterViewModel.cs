using System.ComponentModel.DataAnnotations;

namespace NganHangNhaTro.ViewModels;

public class RegisterViewModel
{
    [Required, StringLength(150)] public string FullName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Phone, StringLength(20)] public string? PhoneNumber { get; set; }
    [Required, MinLength(8), DataType(DataType.Password)] public string Password { get; set; } = "";
    [Required, Compare(nameof(Password)), DataType(DataType.Password), Display(Name = "Nhập lại mật khẩu")]
    public string ConfirmPassword { get; set; } = "";
    [Required] public string Role { get; set; } = "Tenant";
}
