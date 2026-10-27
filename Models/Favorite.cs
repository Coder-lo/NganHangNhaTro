namespace NganHangNhaTro.Models;

public class Favorite
{
    public int FavoriteId { get; set; }
    public string TenantId { get; set; } = "";
    public int RoomId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public AppUser? Tenant { get; set; }
    public Room? Room { get; set; }
}
