using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Models;

public partial class AsignacionBono
{
    public int Id { get; set; }

    [ForeignKey("Empleados")]
    public int EmpleadosId { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un bono.")]
    [ForeignKey("Bonos")]
    public int BonosId { get; set; }

    [Required]
    [Range(0, 1, ErrorMessage = "El estado debe ser 0 (Inactivo) o 1 (Activo).")]
    public byte Estado { get; set; }

    [NotMapped]
    public byte Planilla { get; set; }

    public virtual Bono Bonos { get; set; } = null!;

    public virtual ICollection<DevengoPlanilla> DevengoPlanillas { get; set; } = new List<DevengoPlanilla>();

    public virtual Empleado Empleados { get; set; } = null!;
}
