using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Models;

public partial class Horario
{
    public int Id { get; set; }

    public int TipoDeHorarioId { get; set; }


    [Required(ErrorMessage = "Las Horas por Día son obligatorias.")]
    [Range(1, 24, ErrorMessage = "Las Horas por Día deben estar entre 1 y 24.")]
    [Display(Name = "Horas por Día")]
    public int HorasxDia { get; set; }


    [Required(ErrorMessage = "Los Días son obligatorios.")]
    [StringLength(255, ErrorMessage = "Los Días no pueden exceder los 255 caracteres.")]
    [Display(Name = "Días")]
    [RegularExpression(@"^[a-zA-Z\s,]+$", ErrorMessage = "Los días solo pueden contener letras, espacios y comas.")]
    public string Dias { get; set; } = null!;


    [Required(ErrorMessage = "La Hora de Entrada es obligatoria.")]
    [Display(Name = "Hora de Entrada")]

    public TimeSpan HorasEntrada { get; set; }


    [Required(ErrorMessage = "La Hora de Salida es obligatoria.")]
    [Display(Name = "Hora de Salida")]
    public TimeSpan HorasSalida { get; set; }

    public virtual TipodeHorario? TipoDeHorario { get; set; } = null!;
}
