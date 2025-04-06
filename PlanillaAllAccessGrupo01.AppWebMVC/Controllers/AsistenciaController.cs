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
            // Obtiene la lista de empleados que son supervisores para mostrarla en la vista.
            ViewBag.Jefes = _context.Empleados.Where(e => e.PuestoTrabajo.NombrePuesto == "Supervisor").ToList();

            // Obtiene la lista de todos los tipos de horario para mostrarlos en la vista.
            ViewBag.TiposDeHorario = _context.TipodeHorarios.ToList();

            // Obtiene la lista de todos los empleados, incluyendo su jefe inmediato y el puesto de trabajo del jefe.
            var empleados = _context.Empleados
               .Include(e => e.JefeInmediato)
               .ThenInclude(j => j.PuestoTrabajo)
               .ToList();

            // Imprime en la consola información sobre cada empleado y su jefe (para depuración o registro).
            foreach (var emp in empleados)
            {
                Console.WriteLine($"Empleado: {emp.Nombre} {emp.Apellido}, Jefe: {emp.JefeInmediato?.Nombre}, Puesto del Jefe: {emp.JefeInmediato?.PuestoTrabajo?.NombrePuesto}");
            }
            // Retorna la vista Index.
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> GetEmpleadosPorJefeYHorario(int jefeId, int tipoHorarioId, string diaSeleccionado)
        {

            // Busca empleados que coincidan con el ID del jefe, el ID del tipo de horario y cuyo jefe sea un supervisor.
            // Selecciona los datos necesarios del empleado y el horario específico para el día seleccionado.
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
            // Proyecta los resultados para incluir la hora de entrada, hora de salida y el día del horario específico.
            var resultado = empleados.Select(e => new
            {
                e.Id,
                e.Nombre,
                e.Apellido,
                HoraEntrada = e.Horario?.HorasEntrada, // Usar el horario específico
                HoraSalida = e.Horario?.HorasSalida, // Usar el horario específico
                Dias = e.Horario?.Dias // Usar el horario específico
            });
            // Retorna los resultados en formato JSON.
            return Json(resultado);
        }

        [HttpGet]
        public async Task<IActionResult> GetDiasPorHorario(int tipoHorarioId)
        {

            // Obtiene la lista de días distintos asociados a un tipo de horario específico.
            var dias = await _context.Horarios
                .Where(h => h.TipoDeHorarioId == tipoHorarioId)
                .Select(h => h.Dias)
                .Distinct()// Asegura que no haya días duplicados en la lista.
                .ToListAsync();

            // Retorna la lista de días en formato JSON.
            return Json(dias);
        }



        [HttpGet]
        public async Task<IActionResult> GetAsistenciaEmpleado(int empleadoId, DateTime fecha)
        {
            // Busca el registro de asistencia de un empleado específico en una fecha determinada.
            // Selecciona los datos relevantes del registro de asistencia.
            var asistencia = await _context.ControlAsistencia
                .Where(a => a.EmpleadosId == empleadoId && a.Fecha == DateOnly.FromDateTime(fecha))
                .Select(a => new
                {
                    a.Id,
                    a.Asistencia,
                    a.HorasExtra
                })
                .FirstOrDefaultAsync();// Obtiene el primer registro que coincida o null si no hay ninguno.

            // Retorna el registro de asistencia en formato JSON.
            return Json(asistencia);
        }


        [HttpPost]
        public async Task<IActionResult> GuardarAsistencia(int empleadoId, string dia, TimeSpan entrada, TimeSpan salida, DateTime fecha, string asistencia, int? horasTardia, int? horasExtra)
        {
            // Busca si ya existe un registro de asistencia para el empleado en la fecha especificada.
            var registro = await _context.ControlAsistencia
                .FirstOrDefaultAsync(a => a.EmpleadosId == empleadoId && a.Fecha == DateOnly.FromDateTime(fecha));

            // Si no existe un registro, crea uno nuevo.
            if (registro == null)
            {
                registro = new ControlAsistencium
                {
                    EmpleadosId = empleadoId,
                    Dia = dia,
                    Entrada = entrada,
                    Salida = salida,
                    Fecha = DateOnly.FromDateTime(fecha),
                    Asistencia = asistencia,
                    HoraTardia = horasTardia,
                    HorasExtra = horasExtra
                };
                // Agrega el nuevo registro al contexto.
                _context.ControlAsistencia.Add(registro);
            }
            // Si ya existe un registro, actualiza sus propiedades.
            else
            {
                registro.Asistencia = asistencia;
                registro.HoraTardia = horasTardia;
                registro.HorasExtra = horasExtra;
            }
            // Guarda los cambios en la base de datos.
            await _context.SaveChangesAsync();
            // Retorna una respuesta JSON indicando que la operación fue exitosa.
            return Json(new { success = true });
        }


        public IActionResult VistaListaAsistencia()
        {
            // Retorna la vista para la lista de asistencia.
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> BuscarAsistencia(DateTime fechaInicio, DateTime fechaFin, string? nombre = null)
        {
            // Inicia una consulta para buscar registros de asistencia dentro de un rango de fechas.
            var query = _context.ControlAsistencia
                .Include(a => a.Empleados) // Incluye la información del empleado en los resultados.
                .Where(a => a.Fecha >= DateOnly.FromDateTime(fechaInicio) && a.Fecha <= DateOnly.FromDateTime(fechaFin));

            // Si se proporciona un nombre, filtra los resultados para incluir solo los empleados cuyo nombre completo (nombre + apellido) contenga el nombre buscado.
            if (!string.IsNullOrEmpty(nombre))
            {
                query = query.Where(a => (a.Empleados.Nombre + " " + a.Empleados.Apellido).Contains(nombre));
            }
            // Ejecuta la consulta y selecciona los datos necesarios para mostrar en la lista de asistencia.
            var asistencias = await query
                .Select(a => new
                {
                    a.Id,
                    NombreEmpleado = a.Empleados.Nombre + " " + a.Empleados.Apellido,
                    a.Fecha,
                    a.Asistencia,
                    a.HoraTardia,
                    a.HorasExtra
                })
                .ToListAsync();

            // Retorna los resultados de la búsqueda en formato JSON.
            return Json(asistencias);
        }

        [HttpPost]
        public async Task<IActionResult> ModificarAsistencia(int id, string asistencia, int? horaTardia, int? horasExtra)
        {
            // Busca un registro de asistencia por su ID.
            var registro = await _context.ControlAsistencia.FindAsync(id);
            // Si no se encuentra el registro, retorna un error JSON.
            if (registro == null)
            {
                return Json(new { success = false, message = "Registro no encontrado" });
            }
            // Actualiza las propiedades del registro de asistencia.
            registro.Asistencia = asistencia;
            registro.HoraTardia = horaTardia;
            registro.HorasExtra = horasExtra;
            // Guarda los cambios en la base de datos.
            await _context.SaveChangesAsync();
            return Json(new { success = true });
            // Retorna una respuesta JSON indicando que la modificación fue exitosa.
        }
    }


}


