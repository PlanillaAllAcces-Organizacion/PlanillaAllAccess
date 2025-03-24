using System;
using System.Collections.Generic;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Models;

public partial class Bono
{
    public int Id { get; set; }

    public string NombreBono { get; set; } = null!;

    public decimal Valor { get; set; }

    public byte? Estado { get; set; }

    public DateOnly? FechaValidacion { get; set; }

    public DateOnly? FechaExpiracion { get; set; }

    public byte Operacion { get; set; }

    public byte Planilla { get; set; }

    public virtual ICollection<AsignacionBono> AsignacionBonos { get; set; } = new List<AsignacionBono>();
}
