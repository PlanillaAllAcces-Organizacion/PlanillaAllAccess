using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanillaAllAccessGrupo01.AppWebMVC.Models;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Controllers
{
    [Authorize(Roles = "Supervisor")]
    public class AsistenciaController : Controller
    {

        private readonly PlanillaDbContext _context;

        public AsistenciaController(PlanillaDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.Jefes = _context.Empleados.Where(e => e.PuestoTrabajo.NombrePuesto == "Supervisor").ToList();
            ViewBag.TiposDeHorario = _context.TipodeHorarios.ToList();

            var empleados = _context.Empleados
               .Include(e => e.JefeInmediato)
               .ThenInclude(j => j.PuestoTrabajo)
               .ToList();

            foreach (var emp in empleados)
            {
                Console.WriteLine($"Empleado: {emp.Nombre} {emp.Apellido}, Jefe: {emp.JefeInmediato?.Nombre}, Puesto del Jefe: {emp.JefeInmediato?.PuestoTrabajo?.NombrePuesto}");
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetEmpleadosPorJefeYHorario(int jefeId, int tipoHorarioId, string diaSeleccionado)
        {
            Console.WriteLine($"Buscando empleados con Jefe ID: {jefeId}, Tipo de Horario ID: {tipoHorarioId} y Día: {diaSeleccionado}");

            var empleados = await _context.Empleados
                .Include(e => e.JefeInmediato)
                    .ThenInclude(j => j.PuestoTrabajo)
                .Include(e => e.TipoDeHorario)
                    .ThenInclude(th => th.Horarios)
                .Where(e => e.JefeInmediatoId == jefeId &&
                            e.TipoDeHorarioId == tipoHorarioId &&
                            e.JefeInmediato.PuestoTrabajo.NombrePuesto == "Supervisor")
                .Select(e => new
                {
                    e.Id,
                    e.Nombre,
                    e.Apellido,
                    Horario = e.TipoDeHorario.Horarios.FirstOrDefault(h => h.Dias == diaSeleccionado) // Obtener el horario específico para el día
                })
                .ToListAsync();

            var resultado = empleados.Select(e => new
            {
                e.Id,
                e.Nombre,
                e.Apellido,
                HoraEntrada = e.Horario?.HorasEntrada, // Usar el horario específico
                HoraSalida = e.Horario?.HorasSalida, // Usar el horario específico
                Dias = e.Horario?.Dias // Usar el horario específico
            });

            return Json(resultado);
        }

        [HttpGet]
        public async Task<IActionResult> GetDiasPorHorario(int tipoHorarioId)
        {
            var dias = await _context.Horarios
                .Where(h => h.TipoDeHorarioId == tipoHorarioId)
                .Select(h => h.Dias)
                .Distinct()
                .ToListAsync();

            return Json(dias);
        }



        [HttpGet]
        public async Task<IActionResult> GetAsistenciaEmpleado(int empleadoId, DateTime fecha)
        {
            var asistencia = await _context.ControlAsistencia
                .Where(a => a.EmpleadosId == empleadoId && a.Fecha == DateOnly.FromDateTime(fecha))
                .Select(a => new
                {
                    a.Id,
                    a.Asistencia,
                    a.HorasExtra
                })
                .FirstOrDefaultAsync();

            return Json(asistencia);
        }



    }
}
