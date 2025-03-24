using System;
using System.Collections.Generic;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Models;

public partial class Vacacion
{
    public int Id { get; set; }

    public int EmpleadosId { get; set; }

    public string MesVacaciones { get; set; } = null!;

    public string AnnoVacacion { get; set; } = null!;

    public DateTime DiaInicio { get; set; }

    public DateTime DiaFin { get; set; }

    public byte? Estado { get; set; }

    public byte? VacacionPagada { get; set; }

    public decimal? PagoVacaciones { get; set; }

    public DateTime? FechaPago { get; set; }

    public virtual ICollection<DevengoPlanilla> DevengoPlanillas { get; set; } = new List<DevengoPlanilla>();

    public virtual ICollection<EmpleadoPlanilla> EmpleadoPlanillas { get; set; } = new List<EmpleadoPlanilla>();

    public virtual Empleado Empleados { get; set; } = null!;
}
