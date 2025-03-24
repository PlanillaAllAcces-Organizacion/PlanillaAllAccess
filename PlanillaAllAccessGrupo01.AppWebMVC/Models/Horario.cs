using System;
using System.Collections.Generic;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Models;

public partial class Horario
{
    public int Id { get; set; }

    public int TipoDeHorarioId { get; set; }

    public int HorasxDia { get; set; }

    public string Dias { get; set; } = null!;

    public TimeOnly HorasEntrada { get; set; }

    public TimeOnly HorasSalida { get; set; }

    public virtual TipodeHorario TipoDeHorario { get; set; } = null!;
}
