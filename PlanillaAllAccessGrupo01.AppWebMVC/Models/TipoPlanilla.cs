using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Models;

public partial class TipoPlanilla
{
    public int Id { get; set; }
    [Required(ErrorMessage = "El nombre del tipo de planilla es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres.")]
    [RegularExpression(@"^[^\d]*$", ErrorMessage = "El nombre no puede contener números.")]
    [Display(Name = "Nombre del Tipo de Planilla")]
    public string NombreTipo { get; set; } = null!;

    [NotMapped]
    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; }  = DateTime.Now;

    public DateTime? FechaModificacion { get; set; }

    public virtual ICollection<Planilla> Planillas { get; set; } = new List<Planilla>();
}
