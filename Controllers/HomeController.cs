using Microsoft.AspNetCore.Mvc;

namespace NganHangNhaTro.Controllers;

public class HomeController : Controller
{
    public IActionResult Error() => View("Error");
}
