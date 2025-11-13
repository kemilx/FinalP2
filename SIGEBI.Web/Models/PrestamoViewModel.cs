using System.ComponentModel.DataAnnotations;
using SIGEBI.Domain.ValueObjects;

namespace SIGEBI.Web.Models;

public class PrestamoViewModel
{
    public Guid Id { get; init; }
    public Guid LibroId { get; init; }
    public Guid UsuarioId { get; init; }
    public EstadoPrestamo Estado { get; init; }
    public DateTime FechaInicioUtc { get; init; }
    public DateTime FechaFinUtc { get; init; }
    public DateTime? FechaEntregaRealUtc { get; init; }
    public string? Observaciones { get; init; }

    public string EstadoDescripcion => Estado switch
    {
        EstadoPrestamo.Pendiente => "Pendiente",
        EstadoPrestamo.Activo => "Activo",
        EstadoPrestamo.Vencido => "Vencido",
        EstadoPrestamo.Devuelto => "Devuelto",
        EstadoPrestamo.Cancelado => "Cancelado",
        _ => Estado.ToString()
    };

    public string PeriodoDescripcion =>
        $"{FechaInicioUtc:dd/MM/yyyy} - {FechaFinUtc:dd/MM/yyyy}";
}

public class PrestamoBusquedaViewModel
{
    [Display(Name = "Usuario")]
    [Required(ErrorMessage = "Debe indicar el identificador del usuario.")]
    public Guid? UsuarioId { get; set; }

    public bool BusquedaRealizada { get; set; }

    public IList<PrestamoViewModel> Resultados { get; set; } = new List<PrestamoViewModel>();
}

public class CrearPrestamoViewModel
{
    [Display(Name = "Libro")]
    [Required(ErrorMessage = "Debe indicar el identificador del libro.")]
    public Guid? LibroId { get; set; }

    [Display(Name = "Usuario")]
    [Required(ErrorMessage = "Debe indicar el identificador del usuario.")]
    public Guid? UsuarioId { get; set; }

    [Display(Name = "Fecha de inicio")]
    [DataType(DataType.Date)]
    [Required]
    public DateTime? FechaInicio { get; set; }

    [Display(Name = "Fecha compromiso")]
    [DataType(DataType.Date)]
    [Required]
    public DateTime? FechaFin { get; set; }

    public static CrearPrestamoViewModel CreateDefault()
    {
        var inicio = DateTime.UtcNow.Date;
        return new CrearPrestamoViewModel
        {
            FechaInicio = inicio,
            FechaFin = inicio.AddDays(7)
        };
    }
}
