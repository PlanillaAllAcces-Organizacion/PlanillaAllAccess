using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Models;

public partial class Empleado
{
    public int Id { get; set; }

    [Display(Name = "Jefe Inmediato")]
    public int? JefeInmediatoId { get; set; }


    [Display(Name = "Tipo de horario")]
    public int? TipoDeHorarioId { get; set; }

    [Display(Name = "DUI")]
    [Required(ErrorMessage = "El DUI es obligatorio.")]
    [RegularExpression(@"^\d{8}-\d$", ErrorMessage = "El formato del DUI es inválido.")]
    public string Dui { get; set; } = null!;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El nombre no debe exceder 50 caracteres.")]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [MaxLength(50, ErrorMessage = "El apellido no debe exceder 50 caracteres.")]
    public string Apellido { get; set; } = null!;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^\d{8}$", ErrorMessage = "El número de teléfono debe contener exactamente 8 dígitos.")]
    public string Telefono { get; set; } = null!;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo es inválido.")]
    public string? Correo { get; set; }


    public byte? Estado { get; set; }

    [Display(Name = "Salario Base")]
    [Range(0, double.MaxValue, ErrorMessage = "El salario base debe ser un valor positivo.")]
    public decimal? SalarioBase { get; set; }

    [Display(Name = "Fecha de contratacion")]
    [Required(ErrorMessage = "La fecha inicial del contrato es obligatoria.")]
    public DateOnly FechaContraInicial { get; set; }

    [Display(Name = "Fecha de finalizacion")]
    [Required(ErrorMessage = "La fecha final del contrato es obligatoria.")]
    public DateOnly FechaContraFinal { get; set; }

    [MaxLength(20, ErrorMessage = "El usuario no debe exceder 20 caracteres.")]
    public string? Usuario { get; set; }

    [Display(Name = "Contraseña")]
    [RegularExpression(@"^[A-Za-z\d]{8}$", ErrorMessage = "La contraseña debe tener exactamente 8 caracteres y estar compuesta solo por letras o números.")]
    public string? Password { get; set; }

    [Display(Name = "Puesto de Trabajo")]
    public int? PuestoTrabajoId { get; set; }

    public virtual ICollection<AsignacionBono> AsignacionBonos { get; set; } = new List<AsignacionBono>();

    public virtual ICollection<AsignacionDescuento> AsignacionDescuentos { get; set; } = new List<AsignacionDescuento>();

    public virtual ICollection<ControlAsistencium> ControlAsistencia { get; set; } = new List<ControlAsistencium>();

    public virtual ICollection<EmpleadoPlanilla> EmpleadoPlanillas { get; set; } = new List<EmpleadoPlanilla>();

    public virtual ICollection<Empleado> InverseJefeInmediato { get; set; } = new List<Empleado>();

    public virtual Empleado? JefeInmediato { get; set; }

    public virtual PuestoTrabajo? PuestoTrabajo { get; set; }

    public virtual TipodeHorario? TipoDeHorario { get; set; }

    public virtual ICollection<Vacacion> Vacacions { get; set; } = new List<Vacacion>();
}
