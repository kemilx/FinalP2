using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SIGEBI.Application.Interfaces;
using SIGEBI.Application.Models;
using SIGEBI.Domain.Entities;
using SIGEBI.Domain.ValueObjects;
using SIGEBI.Web.Models;

namespace SIGEBI.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IAdminService _adminService;
    private readonly IPrestamoService _prestamoService;
    private readonly IReporteService _reporteService;

    public HomeController(
        ILogger<HomeController> logger,
        IAdminService adminService,
        IPrestamoService prestamoService,
        IReporteService reporteService)
    {
        _logger = logger;
        _adminService = adminService;
        _prestamoService = prestamoService;
        _reporteService = reporteService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Mostrando la página principal de SIGEBI");
        var summaryTask = _adminService.ObtenerResumenAsync(cancellationToken);
        var prestamosPorEstadoTask = _reporteService.ObtenerPrestamosPorEstadoAsync(cancellationToken);
        var librosPorEstadoTask = _reporteService.ObtenerLibrosPorEstadoAsync(cancellationToken);
        var prestamosVencidosTask = _prestamoService.ObtenerVencidosAsync(DateTime.UtcNow, cancellationToken);

        await Task.WhenAll(summaryTask, prestamosPorEstadoTask, librosPorEstadoTask, prestamosVencidosTask);

        var summary = summaryTask.Result;
        var prestamosPorEstado = prestamosPorEstadoTask.Result;
        var librosPorEstado = librosPorEstadoTask.Result;
        var prestamosVencidos = prestamosVencidosTask.Result
            .OrderByDescending(p => p.Periodo.FechaFinCompromisoUtc)
            .Take(5)
            .Select(MapToViewModel)
            .ToList();

        var viewModel = new HomeDashboardViewModel
        {
            Summary = summary,
            PrestamosPorEstado = prestamosPorEstado,
            LibrosPorEstado = librosPorEstado,
            PrestamosVencidos = prestamosVencidos,
            HeroLinks = new List<HeroLink>
            {
                new("Panel de préstamos", "🔁", controller: "Prestamo", action: "Index"),
                new("Registrar libro", "📘", page: "/Libros/Create"),
                new("Usuarios", "👤", controller: "Usuario", action: "Index"),
                new("Políticas", "🔐", controller: "Home", action: "Privacy")
            },
            Modules = BuildModules(summary, prestamosPorEstado, librosPorEstado, prestamosVencidos.Count),
            Activity = BuildActivity(summary, prestamosPorEstado, librosPorEstado, prestamosVencidos),
            SpotlightModules = BuildSpotlights(summary, prestamosPorEstado, librosPorEstado)
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

    private static IReadOnlyList<DashboardModule> BuildModules(
        AdminSummary summary,
        IReadOnlyDictionary<string, int> prestamosPorEstado,
        IReadOnlyDictionary<string, int> librosPorEstado,
        int alertasPrestamo)
    {
        var pendientes = GetValue(prestamosPorEstado, EstadoPrestamo.Pendiente.ToString());
        var librosReservados = GetValue(librosPorEstado, EstadoLibro.Reservado.ToString());
        var librosDañados = GetValue(librosPorEstado, EstadoLibro.Dañado.ToString());

        return new List<DashboardModule>
        {
            new(
                "Préstamos",
                $"Activos: {summary.PrestamosActivos} · Pendientes: {pendientes}",
                "🔁",
                "text-primary",
                "operacion",
                "Alertas",
                alertasPrestamo,
                controller: "Prestamo",
                action: "Index"),
            new(
                "Libros",
                $"Disponibles: {summary.LibrosDisponibles} · Reservados: {librosReservados}",
                "📚",
                "text-success",
                "catalogo",
                "Dañados",
                librosDañados,
                page: "/Libros/Index"),
            new(
                "Usuarios",
                $"Activos: {summary.UsuariosActivos} · Totales: {summary.TotalUsuarios}",
                "👤",
                "text-info",
                "personas",
                "Activos",
                summary.UsuariosActivos,
                controller: "Usuario",
                action: "Index"),
            new(
                "Penalizaciones",
                "Integra las sanciones generadas desde PrestamoService",
                "⚠️",
                "text-danger",
                "operacion",
                "Activas",
                summary.PenalizacionesActivas,
                controller: "Penalizacion",
                action: "Index"),
            new(
                "Reportes",
                "Consulta los KPI agregados desde ReporteService",
                "📈",
                "text-secondary",
                "analitica",
                "Estados",
                prestamosPorEstado.Sum(x => x.Value),
                controller: "Reporte",
                action: "Index"),
            new(
                "Notificaciones",
                "Alertas basadas en préstamos vencidos",
                "🔔",
                "text-warning",
                "operacion",
                "Pendientes",
                alertasPrestamo,
                controller: "Notificacion",
                action: "Index")
        };
    }

    private static IReadOnlyList<ActivityItem> BuildActivity(
        AdminSummary summary,
        IReadOnlyDictionary<string, int> prestamosPorEstado,
        IReadOnlyDictionary<string, int> librosPorEstado,
        IReadOnlyList<PrestamoViewModel> prestamosVencidos)
    {
        var activity = new List<ActivityItem>();
        var now = DateTime.UtcNow;

        foreach (var prestamo in prestamosVencidos)
        {
            var vencidoHace = (int)Math.Max(0, Math.Ceiling((now - prestamo.FechaFinUtc).TotalDays));
            var timeAgo = vencidoHace <= 0 ? "Vence hoy" : $"Vencido hace {vencidoHace} día(s)";
            activity.Add(new ActivityItem(
                $"Préstamo {ShortId(prestamo.Id)}",
                $"Usuario {ShortId(prestamo.UsuarioId)} · Libro {ShortId(prestamo.LibroId)}",
                timeAgo,
                "bg-danger"));
        }

        if (!activity.Any())
        {
            activity.Add(new ActivityItem(
                "Sin préstamos vencidos",
                "Todos los préstamos se encuentran dentro del periodo comprometido.",
                "Dato generado por PrestamoService",
                "bg-success"));
        }

        var pendientes = GetValue(prestamosPorEstado, EstadoPrestamo.Pendiente.ToString());
        activity.Add(new ActivityItem(
            "Préstamos pendientes",
            $"{pendientes} solicitudes esperan activación.",
            "Dato real del repositorio de préstamos",
            "bg-warning"));

        activity.Add(new ActivityItem(
            "Usuarios activos",
            $"{summary.UsuariosActivos} pueden acceder al sistema.",
            "Dato real del módulo de usuarios",
            "bg-primary"));

        var librosInactivos = GetValue(librosPorEstado, EstadoLibro.Inactivo.ToString());
        activity.Add(new ActivityItem(
            "Libros inactivos",
            $"{librosInactivos} ejemplares fuera de circulación.",
            "Dato real del módulo de libros",
            "bg-secondary"));

        return activity;
    }

    private static IReadOnlyList<SpotlightModule> BuildSpotlights(
        AdminSummary summary,
        IReadOnlyDictionary<string, int> prestamosPorEstado,
        IReadOnlyDictionary<string, int> librosPorEstado)
    {
        var devueltos = GetValue(prestamosPorEstado, EstadoPrestamo.Devuelto.ToString());
        var vencidos = GetValue(prestamosPorEstado, EstadoPrestamo.Vencido.ToString());
        var disponibles = GetValue(librosPorEstado, EstadoLibro.Disponible.ToString());

        return new List<SpotlightModule>
        {
            new(
                "prestamos",
                "Control en vivo de préstamos",
                $"{summary.PrestamosActivos} activos, {vencidos} vencidos y {devueltos} devueltos.",
                "Abrir Préstamos",
                "btn-primary",
                controller: "Prestamo",
                action: "Index"),
            new(
                "libros",
                "Catálogo y disponibilidad",
                $"{disponibles} ejemplares disponibles con {summary.LibrosDisponibles} accesibles.",
                "Abrir Libros",
                "btn-success",
                page: "/Libros/Index"),
            new(
                "reportes",
                "Análisis y métricas",
                "Explora los totales en ReporteService y exporta KPIs.",
                "Abrir Reportes",
                "btn-outline-secondary",
                controller: "Reporte",
                action: "Index")
        };
    }

    private static PrestamoViewModel MapToViewModel(Prestamo prestamo)
    {
        return new PrestamoViewModel
        {
            Id = prestamo.Id,
            LibroId = prestamo.LibroId,
            UsuarioId = prestamo.UsuarioId,
            Estado = prestamo.Estado,
            FechaInicioUtc = prestamo.Periodo.FechaInicioUtc,
            FechaFinUtc = prestamo.Periodo.FechaFinCompromisoUtc,
            FechaEntregaRealUtc = prestamo.FechaEntregaRealUtc,
            Observaciones = prestamo.Observaciones
        };
    }

    private static int GetValue(IReadOnlyDictionary<string, int> source, string key)
        => source.TryGetValue(key, out var value) ? value : 0;

    private static string ShortId(Guid value)
        => value.ToString().Split('-')[0].ToUpperInvariant();
}
