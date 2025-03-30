using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Models;

public partial class Planilla
{
    public int Id { get; set; }
    [Required(ErrorMessage = "El nombre de la planilla es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede tener más de 100 caracteres.")]
    [Display(Name = "Nombre de la Planilla")]
    public string NombrePlanilla { get; set; } = null!;
    [Required(ErrorMessage = "El tipo de planilla es obligatorio.")]
    [Display(Name = "Tipo de Planilla")]
    public int TipoPlanillaId { get; set; }
    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    [Display(Name = "Fecha de Inicio")]
    [DataType(DataType.Date)]
    public DateTime FechaInicio { get; set; }
    [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
    [Display(Name = "Fecha de Fin")]
    [DataType(DataType.Date)]
    public DateTime FechaFin { get; set; }
    [Display(Name = "Autorización")]
    public byte? Autorizacion { get; set; }
    [Required(ErrorMessage = "El total de pago es obligatorio.")]
    [Range(0, 12000, ErrorMessage = "El valor del bono debe estrar entre $0 y $12000.")]
    [Display(Name = "Total de Pago")]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal? TotalPago { get; set; }

    public virtual ICollection<EmpleadoPlanilla> EmpleadoPlanillas { get; set; } = new List<EmpleadoPlanilla>();
    [ForeignKey("TipoPlanillaId")]
    public virtual TipoPlanilla TipoPlanilla { get; set; } = null!;

    [NotMapped]
    [Display(Name = "Cantidad de Empleados")]
    public int CantidadEmpleados => EmpleadoPlanillas?.Count ?? 0;

    //[NotMapped]
    //[Display(Name = "Estado de Autorización")]
    //public string EstadoAutorizacion => Autorizacion == 1 ? "Autorizada" : "No Autorizada";
    [NotMapped]
    [Display(Name = "Estado de Autorización")]
    public string EstadoAutorizacion => Autorizacion switch
    {
        1 => "Autorizada",
        2 => "No Autorizada",
        _ => "Pendiente" // Puedes agregar un estado predeterminado si hay más casos
    };

    public bool EstaActiva()
    {
        var fechaActual = DateTime.UtcNow.Date;
        return fechaActual >= FechaInicio.Date && fechaActual <= FechaFin.Date;
    }
}
