using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Models;

public partial class TipodeHorario
{
    public int Id { get; set; }

    [DisplayName("Nombre de Horario")]
    [Required(ErrorMessage = "El nombre del horario es obligatorio")]
    public string NombreHorario { get; set; } = null!;

    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();

    public virtual ICollection<Horario> Horarios { get; set; } = new List<Horario>();
}
