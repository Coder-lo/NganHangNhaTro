using System.ComponentModel.DataAnnotations;
namespace NganHangNhaTro.ViewModels;
public class ChangePasswordViewModel
{
    [Required, DataType(DataType.Password)] public string CurrentPassword { get; set; } = "";
    [Required, MinLength(8), DataType(DataType.Password)] public string NewPassword { get; set; } = "";
    [Required, Compare(nameof(NewPassword)), DataType(DataType.Password)] public string ConfirmPassword { get; set; } = "";
}
