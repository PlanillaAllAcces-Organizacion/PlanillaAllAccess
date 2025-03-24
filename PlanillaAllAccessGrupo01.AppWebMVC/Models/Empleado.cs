using System;
using System.Collections.Generic;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Models;

public partial class Empleado
{
    public int Id { get; set; }

    public int? JefeInmediatoId { get; set; }

    public int? TipoDeHorarioId { get; set; }

    public string Dui { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public string Apellido { get; set; } = null!;

    public string Telefono { get; set; } = null!;

    public string? Correo { get; set; }

    public byte? Estado { get; set; }

    public decimal? SalarioBase { get; set; }

    public DateOnly FechaContraInicial { get; set; }

    public DateOnly FechaContraFinal { get; set; }

    public string? Usuario { get; set; }

    public string? Password { get; set; }

    public int? PuestoTrabajoId { get; set; }

    public virtual ICollection<AsignacionBono> AsignacionBonos { get; set; } = new List<AsignacionBono>();

    public virtual ICollection<AsignacionDescuento> AsignacionDescuentos { get; set; } = new List<AsignacionDescuento>();

    public virtual ICollection<ControlAsistencium> ControlAsistencia { get; set; } = new List<ControlAsistencium>();

    public virtual ICollection<EmpleadoPlanilla> EmpleadoPlanillas { get; set; } = new List<EmpleadoPlanilla>();

    public virtual ICollection<Empleado> InverseJefeInmediato { get; set; } = new List<Empleado>();

    public virtual Empleado? JefeInmediato { get; set; }

    public virtual PuestoTrabajo? PuestoTrabajo { get; set; }

    public virtual TipodeHorario? TipoDeHorario { get; set; }

    public virtual ICollection<Vacacion> Vacacions { get; set; } = new List<Vacacion>();
}
