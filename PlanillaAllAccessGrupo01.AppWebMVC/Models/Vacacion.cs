using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Models;

public partial class Vacacion
{
    public int Id { get; set; }

    public int EmpleadosId { get; set; }

    [DisplayName("Mes de la vacacion")]
    [Required(ErrorMessage = "Es necesario el mes de la vacacion")]
    public string MesVacaciones { get; set; } = null!;


    [DisplayName("Año de la vacacion")]
    [Required(ErrorMessage = "Es necesario el año de la vacacion")]
    public string AnnoVacacion { get; set; } = null!;

    [DisplayName("Dia de inicio")]
    [Required(ErrorMessage = "Es necesario el dia de inicio")]
    public DateTime DiaInicio { get; set; }



    [DisplayName("Dia fin")]
    [Required(ErrorMessage = "Es necesario el dia de inicio")]
    public DateTime DiaFin { get; set; }

    [DisplayName("Estado")]
    public byte? Estado { get; set; }

    [DisplayName("¿Vacacion pagada?")]
    public byte? VacacionPagada { get; set; }

    [DisplayName("Pago de la vacacion")]
    public decimal? PagoVacaciones { get; set; }

    [DisplayName("Fecha en la que se le pagara")]

    public DateTime? FechaPago { get; set; }

    public virtual ICollection<DevengoPlanilla> DevengoPlanillas { get; set; } = new List<DevengoPlanilla>();

    public virtual ICollection<EmpleadoPlanilla> EmpleadoPlanillas { get; set; } = new List<EmpleadoPlanilla>();

    public virtual Empleado Empleados { get; set; } = null!;
}
