using Microsoft.AspNetCore.Mvc;

namespace SIGEBI.Web.Controllers;

public class UsuarioController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}
