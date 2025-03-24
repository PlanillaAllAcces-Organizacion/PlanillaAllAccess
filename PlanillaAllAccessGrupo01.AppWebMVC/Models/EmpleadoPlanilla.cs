using System;
using System.Collections.Generic;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Models;

public partial class EmpleadoPlanilla
{
    public int Id { get; set; }

    public int? EmpleadosId { get; set; }

    public int? DescuentoPlanillaId { get; set; }

    public int? PlanillaId { get; set; }

    public decimal? SueldoBase { get; set; }

    public int? TotalDiasTrabajados { get; set; }

    public int? TotalHorasTardias { get; set; }

    public int? TotalHorasTrabajadas { get; set; }

    public decimal? ValorDeHorasExtra { get; set; }

    public int? TotalHorasExtra { get; set; }

    public decimal? TotalPagoHorasExtra { get; set; }

    public decimal? TotalDevengos { get; set; }

    public decimal? SubTotal { get; set; }

    public int? VacacionId { get; set; }

    public decimal? TotalPagoVacacion { get; set; }

    public decimal? TotalDescuentos { get; set; }

    public decimal? LiquidoTotal { get; set; }

    public virtual DescuentoPlanilla? DescuentoPlanilla { get; set; }

    public virtual Empleado? Empleados { get; set; }

    public virtual Planilla? Planilla { get; set; }

    public virtual Vacacion? Vacacion { get; set; }
}
