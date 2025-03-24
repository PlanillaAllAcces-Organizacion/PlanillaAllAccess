using System;
using System.Collections.Generic;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Models;

public partial class PuestoTrabajo
{
    public int Id { get; set; }

    public string NombrePuesto { get; set; } = null!;

    public decimal SalarioBase { get; set; }

    public decimal ValorxHora { get; set; }

    public decimal ValorExtra { get; set; }

    public byte? Estado { get; set; }

    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}
