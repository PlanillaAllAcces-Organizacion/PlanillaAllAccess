using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Models;

public partial class Descuento
{
    [Display(Name = "Id")]
    public int Id { get; set; }
    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El nombre del descuento es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre del descuento no puede exceder los 100 caracteres")]
    public string Nombre { get; set; } = null!;
    [Display(Name = "Valor")]
    [Required(ErrorMessage = "El valor del descuento es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El valor debe ser mayor a 0")]
    [RegularExpression(@"^\d{1,10}(\.\d{1,4})?$", ErrorMessage = "El valor debe tener hasta 10 dígitos antes del punto y hasta 4 decimales.")]
    public decimal Valor { get; set; }
    [Display(Name = "Estado")]
    [Required]
    [Range(0, 2, ErrorMessage = "El estado debe ser 0 (No definida), 1 (Activo) o 2 (Inactivo)")]
    public byte? Estado { get; set; }
    [Display(Name = "Operacion")]
    [Required]
    [Range(0, 2, ErrorMessage = "La operación debe ser 0 (No definida), 1 (Operación Fija) o 2 (Operación No Fija)")]
    public byte Operacion { get; set; }
    [Display(Name = "Planilla")]
    [Required]
    [Range(0, 2, ErrorMessage = "La planilla debe ser 0 (No definida), 1 (Planilla Mensual) o 2 (Planilla Quincenal)")]
    public byte Planilla { get; set; }

    public virtual ICollection<AsignacionDescuento> AsignacionDescuentos { get; set; } = new List<AsignacionDescuento>();

    #region METODO PARA MOSTRAR EN TEXTO LAS OPCIONES BYTE
    //ESTO ES EXCLUSIVAMENTE PARA MOSTERAR EN EL INDEX
    [NotMapped]
    public string EstadoTexto
    {
        get
        {
            return Estado switch
            {
                0 => "No definida",
                1 => "Activo",
                2 => "Inactivo",
                _ => "Desconocido"
            };
        }
    }

    [NotMapped]
    public string OperacionTexto
    {
        get
        {
            return Operacion switch
            {
                0 => "No definida",
                1 => "Operación Fija",
                2 => "Operación No Fija",
                _ => "Desconocida"
            };
        }
    }

    [NotMapped]
    public string PlanillaTexto
    {
        get
        {
            return Planilla switch
            {
                0 => "No definida",
                1 => "Planilla Mensual",
                2 => "Planilla Quincenal",
                _ => "Desconocida"
            };
        }
    }

    [NotMapped]
    [Display(Name = "Fecha Validación")]
    public DateOnly? FechaValidacion { get; set; }

    [NotMapped]
    [Display(Name = "Fecha Expiración")]
    public DateOnly? FechaExpiracion { get; set; }


    #endregion

}
