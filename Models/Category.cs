using System.ComponentModel.DataAnnotations;

namespace NganHangNhaTro.Models;

public class Category
{
    public int CategoryId { get; set; }
    [Required, StringLength(150, ErrorMessage = "Tên danh mục tối đa 150 ký tự.")]
    public string CategoryName { get; set; } = "";
    [Required] public string Type { get; set; } = "RoomType";
    public ICollection<Room> Rooms { get; set; } = new List<Room>();
}
