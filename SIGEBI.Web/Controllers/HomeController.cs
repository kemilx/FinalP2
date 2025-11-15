using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SIGEBI.Application.Interfaces;
using SIGEBI.Web.Models;

namespace SIGEBI.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IAdminService _adminService;

    public HomeController(ILogger<HomeController> logger, IAdminService adminService)
    {
        _logger = logger;
        _adminService = adminService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Mostrando la página principal de SIGEBI");
        var summary = await _adminService.ObtenerResumenAsync(cancellationToken);

        var viewModel = new HomeDashboardViewModel
        {
            Summary = summary,
            HeroLinks = new List<HeroLink>
            {
                new("Panel de préstamos", "🔁", controller: "Prestamo", action: "Index"),
                new("Registrar libro", "📘", page: "/Libros/Create"),
                new("Usuarios", "👤", controller: "Usuario", action: "Index"),
                new("Políticas", "🔐", controller: "Home", action: "Privacy")
            },
            Modules = new List<DashboardModule>
            {
                new("Préstamos", "Consulta historial, activa préstamos y revisa extensiones.", "🔁", "text-primary", "operacion", controller: "Prestamo", action: "Index"),
                new("Libros", "Administra títulos, ejemplares y disponibilidad.", "📚", "text-success", "catalogo", page: "/Libros/Index"),
                new("Usuarios", "Gestiona perfiles y accesos de lectores.", "👤", "text-info", "personas", controller: "Usuario", action: "Index"),
                new("Penalizaciones", "Controla sanciones y desbloqueos.", "⚠️", "text-danger", "operacion", controller: "Penalizacion", action: "Index"),
                new("Reportes", "Genera KPIs y métricas académicas.", "📈", "text-secondary", "analitica", controller: "Reporte", action: "Index"),
                new("Notificaciones", "Envía alertas y recordatorios automáticos.", "🔔", "text-warning", "operacion", controller: "Notificacion", action: "Index")
            },
            Activity = new List<ActivityItem>
            {
                new("Préstamos activados", "3 préstamos confirmados desde la web.", "Hace 5 minutos", "bg-primary"),
                new("Reporte mensual", "El área académica exportó un informe.", "Hace 12 minutos", "bg-success"),
                new("Penalizaciones en revisión", "2 sanciones pendientes de aprobación.", "Hace 30 minutos", "bg-warning"),
                new("Nuevos libros", "4 títulos añadidos al catálogo.", "Hace 1 hora", "bg-info")
            },
            SpotlightModules = new List<SpotlightModule>
            {
                new("prestamos", "Control en vivo de préstamos", "Conecta las acciones Index/Create con la partial _PrestamoResultados para monitorear estados.", "Abrir Préstamos", "btn-primary", controller: "Prestamo", action: "Index"),
                new("libros", "Catálogo y disponibilidad", "Registra títulos nuevos y consulta stock desde las Razor Pages de Libros.", "Abrir Libros", "btn-success", page: "/Libros/Index"),
                new("reportes", "Análisis y métricas", "Centraliza indicadores mensuales para auditorías académicas.", "Abrir Reportes", "btn-outline-secondary", controller: "Reporte", action: "Index")
            }
        };

        return View(viewModel);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var model = new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier };
        return View(model);
    }
}
