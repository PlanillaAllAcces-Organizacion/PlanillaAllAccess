        using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Models;

public partial class PuestoTrabajo
{
    public int Id { get; set; }


    [Required(ErrorMessage = "El nombre del puesto es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre del puesto es demasiado largo.")]
    public string NombrePuesto { get; set; } = null!;

    [Required(ErrorMessage = "El salario base es obligatorio.")]
    [Range(0, double.MaxValue, ErrorMessage = "El salario base debe ser un valido.")]
    public decimal SalarioBase { get; set; }

    [Required(ErrorMessage = "El valor por hora es obligatorio.")]
    [Range(0, double.MaxValue, ErrorMessage = "El valor por hora debe ser un número positivo.")]
    public decimal ValorxHora { get; set; }

    [Required(ErrorMessage = "El campo es obligatorio")]
    [Range(0, double.MaxValue, ErrorMessage = "El valor extra debe ser un número positivo.")]
    public decimal? ValorExtra { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio.")]
    public byte Estado { get; set; }


    [NotMapped]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public virtual ICollection<Empleado> Empleados { get; set; } = new List<Empleado>();
}
