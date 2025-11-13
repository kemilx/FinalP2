using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SIGEBI.Application.Common.Exceptions;
using SIGEBI.Application.Interfaces;
using SIGEBI.Application.Prestamos.Commands;
using SIGEBI.Domain.Entities;
using SIGEBI.Web.Models;

namespace SIGEBI.Web.Controllers;

public class PrestamoController : Controller
{
    private readonly IPrestamoService _prestamoService;

    public PrestamoController(IPrestamoService prestamoService)
    {
        _prestamoService = prestamoService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid? usuarioId, CancellationToken cancellationToken)
    {
        var model = new PrestamoBusquedaViewModel { UsuarioId = usuarioId };

        if (usuarioId.HasValue && usuarioId.Value != Guid.Empty)
        {
            model.BusquedaRealizada = true;
            var prestamos = await _prestamoService.ObtenerPorUsuarioAsync(usuarioId.Value, cancellationToken);
            model.Resultados = prestamos.Select(MapToViewModel).ToList();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(PrestamoBusquedaViewModel model, CancellationToken cancellationToken)
    {
        model.BusquedaRealizada = true;

        if (!ModelState.IsValid || !model.UsuarioId.HasValue || model.UsuarioId == Guid.Empty)
        {
            if (!model.UsuarioId.HasValue || model.UsuarioId == Guid.Empty)
            {
                ModelState.AddModelError(nameof(model.UsuarioId), "Debe ingresar un GUID válido.");
            }

            model.Resultados.Clear();
            return View(model);
        }

        var prestamos = await _prestamoService.ObtenerPorUsuarioAsync(model.UsuarioId.Value, cancellationToken);
        model.Resultados = prestamos.Select(MapToViewModel).ToList();
        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(CrearPrestamoViewModel.CreateDefault());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CrearPrestamoViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.FechaInicio.HasValue && model.FechaFin.HasValue && model.FechaFin <= model.FechaInicio)
        {
            ModelState.AddModelError(nameof(model.FechaFin), "La fecha fin debe ser posterior a la fecha de inicio.");
            return View(model);
        }

        var command = new CrearPrestamoCommand(
            model.LibroId!.Value,
            model.UsuarioId!.Value,
            DateTime.SpecifyKind(model.FechaInicio!.Value, DateTimeKind.Utc),
            DateTime.SpecifyKind(model.FechaFin!.Value, DateTimeKind.Utc));

        try
        {
            var prestamo = await _prestamoService.CrearAsync(command, cancellationToken);
            TempData["PrestamoCreado"] = prestamo.Id.ToString();
            return RedirectToAction(nameof(Index), new { usuarioId = prestamo.UsuarioId });
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                var key = string.IsNullOrWhiteSpace(error.PropertyName) ? string.Empty : error.PropertyName;
                ModelState.AddModelError(key, error.ErrorMessage);
            }
        }
        catch (NotFoundException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        catch (ConflictException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        return View(model);
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
}
