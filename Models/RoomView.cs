namespace NganHangNhaTro.Models;
public class RoomView
{
    public long RoomViewId { get; set; }
    public int RoomId { get; set; }
    public DateTime ViewedAt { get; set; } = DateTime.Now;
    public Room? Room { get; set; }
}
