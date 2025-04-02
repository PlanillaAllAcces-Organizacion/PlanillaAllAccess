using System;
using System.Collections.Generic;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Models;

public partial class ControlAsistencium
{
    public int Id { get; set; }

    public int EmpleadosId { get; set; }

    public string Dia { get; set; } = null!;

    public TimeSpan Entrada { get; set; }

    public TimeSpan Salida { get; set; }

    public DateOnly Fecha { get; set; }

    public string? Asistencia { get; set; }

    public int? HoraTardia { get; set; }

    public int? HorasExtra { get; set; }

    public byte? EstadoPlanilla { get; set; }

    public virtual Empleado Empleados { get; set; } = null!;
}
