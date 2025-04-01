using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Models;

// Definición de la clase parcial TipoPlanilla, que representa un tipo de planilla en la base de datos
public partial class TipoPlanilla
{
    // Propiedad que representa el identificador único de la planilla
    public int Id { get; set; }

    // Campo obligatorio con validaciones para el nombre del tipo de planilla

    [Required(ErrorMessage = "El nombre del tipo de planilla es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres.")]
    [RegularExpression(@"^[^\d]*$", ErrorMessage = "El nombre no puede contener números.")]
    [Display(Name = "Nombre del Tipo de Planilla")]
    public string NombreTipo { get; set; } = null!; // Se asegura de que no sea nulo

    // Propiedad no mapeada en la base de datos que indica si la planilla está activa o no
    [NotMapped]
    public bool Activo { get; set; } = true;

    // Propiedad que almacena la fecha de creación de la planilla (se inicializa con la fecha actual)
    public DateTime FechaCreacion { get; set; }  = DateTime.Now;

    // Propiedad opcional que almacena la fecha de última modificación de la planilla
    public DateTime? FechaModificacion { get; set; }

    // Relación con la entidad Planilla (una TipoPlanilla puede estar relacionada con muchas Planillas)

    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();

    public virtual ICollection<Planilla> Planillas { get; set; } = new List<Planilla>();
}
