using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Models;

public partial class Vacacion
{
    public int Id { get; set; }

    [DisplayName("Nombre de Empleado")]
    public int EmpleadosId { get; set; }

    [DisplayName("Mes de la vacación")]
    [Required(ErrorMessage = "Es necesario seleccionar el mes de la vacación.")]
    public string MesVacaciones { get; set; } = string.Empty;

    [DisplayName("Año de la vacación")]
    [Required(ErrorMessage = "Es necesario ingresar el año de la vacación.")]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "Debe ingresar un año válido (ej. 2025).")]
    public string AnnoVacacion { get; set; } = string.Empty;

    [DisplayName("Día de inicio")]
    [Required(ErrorMessage = "Es necesario ingresar el día de inicio.")]
    [DisplayFormat(DataFormatString = "{0:MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime DiaInicio { get; set; }

    [DisplayName("Día de fin")]
    [Required(ErrorMessage = "Es necesario ingresar el día de finalización.")]
    [DisplayFormat(DataFormatString = "{0:MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime DiaFin { get; set; }

    [DisplayName("Estado")]
    [Required(ErrorMessage = "Debe seleccionar un estado.")]
    public byte? Estado { get; set; }

    [DisplayName("¿Vacación pagada?")]
    [Required(ErrorMessage = "Debe indicar si la vacación es pagada.")]
    public byte? VacacionPagada { get; set; }

    [DisplayName("Pago de la vacación")]
    [Range(0, double.MaxValue, ErrorMessage = "El pago debe ser un número positivo.")]
    public decimal? PagoVacaciones { get; set; }

    [DisplayName("Fecha en la que se le pagará")]
    [DataType(DataType.Date)]
    public DateTime? FechaPago { get; set; }

    public virtual ICollection<DevengoPlanilla> DevengoPlanillas { get; set; } = new List<DevengoPlanilla>();

    public virtual ICollection<EmpleadoPlanilla> EmpleadoPlanillas { get; set; } = new List<EmpleadoPlanilla>();

    public virtual Empleado? Empleados { get; set; }



    #region METODO PARA MOSTRAR EN TEXTO LAS OPCIONES BYTE
    // ESTO ES EXCLUSIVAMENTE PARA MOSTRAR EN EL INDEX
    [DisplayName("Estado")]
    [NotMapped]
    public string EstadoTexto
    {
        get
        {
            return Estado switch
            {
                0 => "No definida",
                1 => "Asignada",
                2 => "Finalizada",
                _ => "Desconocido"
            };
        }
    }


    [DisplayName("Vacaciones pagadas")]
    [NotMapped]
    public string VacacionPagadaTxt
    {
        get
        {
            return VacacionPagada switch
            {
                0 => "No definida",
                1 => "Si",
                2 => "No",
                _ => "Desconocida"
            };
        }
    }

    #endregion
}
