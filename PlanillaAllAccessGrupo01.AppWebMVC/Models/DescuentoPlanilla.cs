using System;
using System.Collections.Generic;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Models;

public partial class DescuentoPlanilla
{
    public int Id { get; set; }

    public int AsignacionDescuentoId { get; set; }

    public byte? Estado { get; set; }

    public virtual AsignacionDescuento AsignacionDescuento { get; set; } = null!;

    public virtual ICollection<EmpleadoPlanilla> EmpleadoPlanillas { get; set; } = new List<EmpleadoPlanilla>();
}
