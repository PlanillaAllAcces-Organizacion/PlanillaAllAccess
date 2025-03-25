using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Models;

public partial class Bono
{
    public int Id { get; set; }


    [Required(ErrorMessage = "El nombre del bono es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre del bono no puede exceder los 100 caracteres")]
    public string NombreBono { get; set; } = null!;


    [Required(ErrorMessage = "El valor del bono es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El valor debe ser mayor a 0")]
    public decimal Valor { get; set; }


    [Required]
    [Range(0, 2, ErrorMessage = "El estado debe ser 0 (No definida), 1 (Activo) o 2 (Inactivo)")]
    public byte? Estado { get; set; }


    public DateOnly? FechaValidacion { get; set; }


    public DateOnly? FechaExpiracion { get; set; }

    [Required]
    [Range(0, 2, ErrorMessage = "La operación debe ser 0 (No definida), 1 (Operación Fija) o 2 (Operación No Fija)")]
    public byte Operacion { get; set; }

    [Required]
    [Range(0, 2, ErrorMessage = "La planilla debe ser 0 (No definida), 1 (Planilla Mensual) o 2 (Planilla Quincenal)")]
    public byte Planilla { get; set; }

    public virtual ICollection<AsignacionBono> AsignacionBonos { get; set; } = new List<AsignacionBono>();
}
