using Microsoft.AspNetCore.Mvc;

namespace SIGEBI.Web.Controllers;

public class PenalizacionController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}
