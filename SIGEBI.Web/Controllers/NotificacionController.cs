using Microsoft.AspNetCore.Mvc;

namespace SIGEBI.Web.Controllers;

public class NotificacionController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}
