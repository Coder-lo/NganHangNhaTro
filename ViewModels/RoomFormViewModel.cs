using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace NganHangNhaTro.ViewModels;

public class RoomFormViewModel
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
    public string? ExistingImageUrl { get; set; }
    public IFormFile? ImageFile { get; set; }
    public List<IFormFile>? ImageFiles { get; set; }
    [Required] public int CategoryId { get; set; }
    public int? LocationCategoryId { get; set; }
    public int? RoomTypeCategoryId { get; set; }
}
