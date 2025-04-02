using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlanillaAllAccessGrupo01.AppWebMVC.Models;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Controllers
{
    [Authorize]
    public class TipoDeHorarioController : Controller
    {
        private readonly PlanillaDbContext _context;

        public TipoDeHorarioController(PlanillaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(TipodeHorario tipodeHorario)
        {
            // Esta acción HTTP GET muestra una lista de tipos de horarios.

            // Crea una consulta LINQ para obtener los tipos de horarios de la base de datos.
            var query = _context.TipodeHorarios.AsQueryable();

            // Si se proporciona un nombre de horario en el modelo, filtra la consulta para buscar coincidencias.
            if (!string.IsNullOrWhiteSpace(tipodeHorario.NombreHorario))
                query = query.Where(s => s.NombreHorario.Contains(tipodeHorario.NombreHorario));

            // Ordena los resultados por ID de forma descendente.
            query = query.OrderByDescending(s => s.Id);

            // Ejecuta la consulta y retorna la vista "Index" con los resultados.
            return View(await query.ToListAsync());
        }


        public IActionResult Create()
        {
            // Esta acción HTTP GET simplemente muestra la vista de creación de un nuevo tipo de horario.
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NombreHorario")] TipodeHorario tipoDeHorario)
        {

            // Verifica si el modelo es válido.
            if (ModelState.IsValid)
            {
                // Agrega el nuevo tipo de horario a la base de datos.
                _context.Add(tipoDeHorario);
                // Guarda los cambios en la base de datos de forma asíncrona.
                await _context.SaveChangesAsync();
                // Redirige a la acción "Index" para mostrar la lista actualizada de tipos de horarios.
                return RedirectToAction(nameof(Index));
            }
            return View(tipoDeHorario);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            // Esta acción HTTP GET muestra el formulario de edición de un tipo de horario existente.

            // Verifica si el 'id' proporcionado es nulo.
            if (id == null)
            {
                // Si 'id' es nulo, retorna un resultado NotFound (código de estado 404).
                return NotFound();
            }

            // Realiza una consulta asíncrona a la base de datos para obtener el tipo de horario con el 'id' especificado.
            // Incluye la relación 'Horarios' para cargar los horarios relacionados.
            var tipoDeHorario = await _context.TipodeHorarios
                .Include(t => t.Horarios)
                .FirstOrDefaultAsync(t => t.Id == id);

            // Verifica si se encontró el tipo de horario.
            if (tipoDeHorario == null)
            {
                // Si no se encontró, retorna un resultado NotFound (código de estado 404).
                return NotFound();
            }
            // Prepara los datos para el dropdown en la vista (TipoDeHorarioId).
            ViewBag.TipoDeHorarioId = new SelectList(_context.TipodeHorarios, "Id", "NombreHorario", tipoDeHorario.Id);
            return View(tipoDeHorario);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TipodeHorario tipoDeHorario)
        {
            // Esta acción HTTP POST maneja la edición de un tipo de horario existente.

            // Verifica si el 'id' proporcionado coincide con el 'id' del tipo de horario en el modelo.
            if (id != tipoDeHorario.Id)
            {
                // Si no coinciden, retorna un resultado NotFound (código de estado 404).
                return NotFound();
            }
            // Verifica si el modelo es válido.
            if (ModelState.IsValid)
            {
                try
                {
                    // Realiza una consulta asíncrona a la base de datos para obtener el tipo de horario actual con el 'id' especificado.
                    // Incluye la relación 'Horarios' para cargar los horarios relacionados.
                    var tipoDeHorarioActual = await _context.TipodeHorarios
                        .Include(t => t.Horarios)
                        .FirstOrDefaultAsync(t => t.Id == id);

                    // Verifica si se encontró el tipo de horario actual.
                    if (tipoDeHorarioActual == null)
                    {
                        // Si no se encontró, retorna un resultado NotFound (código de estado 404).
                        return NotFound();
                    }

                    // Actualiza el nombre del tipo de horario actual con el nombre del tipo de horario modificado.
                    tipoDeHorarioActual.NombreHorario = tipoDeHorario.NombreHorario;

                    // Itera a través de los horarios del tipo de horario modificado.
                    foreach (var horario in tipoDeHorario.Horarios)
                    {
                        // Verifica si el horario tiene un ID mayor que 0 (es decir, ya existe en la base de datos).
                        if (horario.Id > 0)
                        {
                            // Busca el horario existente en la lista de horarios del tipo de horario actual.
                            var horarioExistente = tipoDeHorarioActual.Horarios.FirstOrDefault(h => h.Id == horario.Id);

                            // Si se encontró el horario existente, actualiza sus propiedades.
                            if (horarioExistente != null)
                            {
                                // Si el horario no existe en la base de datos, lo agrega a la lista de horarios del tipo de horario actual.
                                horarioExistente.Dias = horario.Dias;
                                horarioExistente.HorasxDia = horario.HorasxDia;
                                horarioExistente.HorasEntrada = TimeSpan.Parse(horario.HorasEntrada.ToString("hh\\:mm"));
                                horarioExistente.HorasSalida = TimeSpan.Parse(horario.HorasSalida.ToString("hh\\:mm"));
                            }
                        }
                        else
                        {
                            // Si el horario no existe en la base de datos, lo agrega a la lista de horarios del tipo de horario actual.
                            tipoDeHorarioActual.Horarios.Add(horario);
                        }
                    }

                    // Obtiene una lista de horarios que deben eliminarse (es decir, horarios que existen en el tipo de horario actual pero no en el tipo de horario modificado).
                    var horariosAEliminar = tipoDeHorarioActual.Horarios
                        .Where(h => !tipoDeHorario.Horarios.Any(nh => nh.Id == h.Id))
                        .ToList();

                    // Itera a través de los horarios que deben eliminarse y los elimina de la base de datos.
                    foreach (var horario in horariosAEliminar)
                    {
                        _context.Horarios.Remove(horario);
                    }

                    // Guarda los cambios en la base de datos de forma asíncrona.
                    await _context.SaveChangesAsync();

                    // Redirige a la acción "Index" para mostrar la lista actualizada de tipos de horarios.
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Maneja la excepción de concurrencia si ocurre.
                    if (!TipoDeHorarioExists(tipoDeHorario.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Si el modelo no es válido, prepara los datos para el dropdown en la vista (TipoDeHorarioId) y retorna la vista "Edit" con el objeto 'tipoDeHorario' y los errores de validación.
            ViewBag.TipoDeHorarioId = new SelectList(_context.TipodeHorarios, "Id", "NombreHorario", tipoDeHorario.Id);
            return View(tipoDeHorario);
        }


        private bool TipoDeHorarioExists(int id)
        {
            // Esta función privada verifica si existe un tipo de horario con el 'id' especificado en la base de datos.
            return _context.TipodeHorarios.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Details(int? id)
        { 
            // Esta acción HTTP GET muestra los detalles de un tipo de horario específico.

            // Verifica si el 'id' proporcionado es nulo.
            if (id == null)
            {
                // Si 'id' es nulo, retorna un resultado NotFound (código de estado 404).
                return NotFound();
            }

            // Realiza una consulta asíncrona a la base de datos para obtener el tipo de horario con el 'id' especificado.
            // Incluye la relación 'Horarios' para cargar los horarios relacionados.
            var tipoDeHorario = await _context.TipodeHorarios
                 .Include(h => h.Horarios)
                .FirstOrDefaultAsync(h => h.Id == id);

            // Verifica si se encontró el tipo de horario.
            if (tipoDeHorario == null)
            {
                // Si no se encontró, retorna un resultado NotFound (código de estado 404).
                return NotFound();
            }

            return View(tipoDeHorario);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            // Esta acción HTTP GET muestra la vista de confirmación de eliminación de un tipo de horario.

            // Verifica si el 'id' proporcionado es nulo.
            if (id == null)
            {
                // Si 'id' es nulo, retorna un resultado NotFound (código de estado 404).
                return NotFound();
            }
            // Realiza una consulta asíncrona a la base de datos para obtener el tipo de horario con el 'id' especificado.
            // Incluye la relación 'Horarios' para cargar los horarios relacionados.
            var tipoDeHorario = await _context.TipodeHorarios
                .Include(h => h.Horarios)
                .FirstOrDefaultAsync(h => h.Id == id);

            // Verifica si se encontró el tipo de horario.
            if (tipoDeHorario == null)
            {
                // Si no se encontró, retorna un resultado NotFound (código de estado 404).
                return NotFound();
            }

            return View(tipoDeHorario);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Esta acción HTTP POST realiza la eliminación del tipo de horario.

            // Realiza una consulta asíncrona a la base de datos para obtener el tipo de horario con el 'id' especificado.
            var tipoDeHorario = await _context.TipodeHorarios.FindAsync(id);

            // Verifica si se encontró el tipo de horario.
            if (tipoDeHorario != null)
            {
                // Si se encontró, elimina el tipo de horario de la base de datos.
                _context.TipodeHorarios.Remove(tipoDeHorario);
            }
            // Guarda los cambios en la base de datos de forma asíncrona.
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
