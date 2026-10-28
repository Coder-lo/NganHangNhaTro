using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;


namespace NganHangNhaTro.ViewModels;

public class RoomFormViewModel
{
    public int RoomId { get; set; }

    [Required, StringLength(250)]
    public string Title { get; set; } = "";

    [Range(0, 1000000000)]
    public decimal Price { get; set; }

    [Range(typeof(decimal), "0.1", "100000")]
    public decimal Area { get; set; }

    [Required, StringLength(300)]
    public string Address { get; set; } = "";

    public string? Description { get; set; }

    public string? ExistingImageUrl { get; set; }

    public IFormFile? ImageFile { get; set; }

    public List<IFormFile> ImageFiles { get; set; } = new();

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn khu vực.")]
    public int LocationCategoryId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn loại phòng.")]
    public int RoomTypeCategoryId { get; set; }

    [Range(typeof(decimal), "0", "1000000000")]
    public decimal ElectricityPrice { get; set; }

    [Range(typeof(decimal), "0", "1000000000")]
    public decimal WaterPrice { get; set; }

    [Range(typeof(decimal), "0", "1000000000")]
    public decimal ServiceFee { get; set; }

    [StringLength(500)]
    public string? Amenities { get; set; }
}
