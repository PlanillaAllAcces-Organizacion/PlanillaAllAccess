using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Models;

public partial class Bono
{
    // Este modelo representa un Bono con sus respectivas propiedades y validaciones. 
    // Incluye atributos de validación para garantizar que se ingresen datos correctos, como el nombre, valor, estado, operación, y planilla.
    // También cuenta con propiedades adicionales no mapeadas (EstadoTexto, OperacionTexto, PlanillaTexto) 
    // que se utilizan para mostrar descripciones más legibles en las interfaces, como el listado (Index).
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


    #region METODO PARA MOSTRAR EN TEXTO LAS OPCIONES BYTE
    // Propiedades calculadas para mostrar texto descriptivo de las opciones de estado, operación y planilla.
    // Se marcan como [NotMapped] ya que no se almacenan en la base de datos.

    // ESTO ES EXCLUSIVAMENTE PARA MOSTRAR EN EL INDEX
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

    #endregion
}
