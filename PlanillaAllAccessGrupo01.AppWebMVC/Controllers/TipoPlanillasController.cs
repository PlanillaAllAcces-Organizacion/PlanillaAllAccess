using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlanillaAllAccessGrupo01.AppWebMVC.Models;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Controllers
{
    //Autorización para tener acceso a este apartado de Tipo de Planilla
    [Authorize(Roles = "Recursos Humanos")]
    public class TipoPlanillasController : Controller
    {
        private readonly PlanillaDbContext _context;

        public TipoPlanillasController(PlanillaDbContext context)
        {
            _context = context;
        }

        // Acción para mostrar la lista de tipos de planilla con la opción de filtrado por nombre.
        // Recibe un objeto tipoPlanilla para obtener el valor de filtrado (NombreTipo) desde el formulario.
        // Si se especifica un nombre de tipo de planilla, filtra los resultados que contengan ese nombre.
        // Los resultados se ordenan en orden descendente por el campo "Id".
        // Finalmente, la lista de resultados se devuelve a la vista.
        public async Task<IActionResult> Index(TipoPlanilla tipoPlanila)
        {
            var query = _context.TipoPlanillas.AsQueryable(); // Se crea una consulta base de todos los tipos de planilla

            // Si el campo NombreTipo no está vacío o nulo, se agrega un filtro para buscar por nombre
            if (!string.IsNullOrWhiteSpace(tipoPlanila.NombreTipo))
                query = query.Where(s => s.NombreTipo.Contains(tipoPlanila.NombreTipo)); // Filtra los registros donde el NombreTipo contiene el valor ingresado

            query = query.OrderByDescending(s => s.Id); // Ordena los resultados en orden descendente por el campo "Id"

            // Ejecuta la consulta, la convierte en una lista y la pasa a la vista
            return View(await query.ToListAsync());
        }

        // Acción que devuelve los detalles de un tipo de planilla específico.
        // Recibe un parámetro "id" para identificar cuál tipo de planilla mostrar.
        // Si el parámetro "id" es nulo, retorna un error 404 (NotFound).
        // Si no se encuentra un tipo de planilla con el id especificado, también retorna NotFound.
        // Si se encuentra el tipo de planilla, se pasa a la vista para mostrar los detalles.
        public async Task<IActionResult> Details(int? id)
        {
            // Verifica si el id es nulo. Si es nulo, retorna NotFound.
            if (id == null)
            {
                return NotFound();
            }
            // Busca el tipo de planilla en la base de datos utilizando el id proporcionado.
            // Utiliza FirstOrDefaultAsync para obtener el primer resultado o nulo si no lo encuentra.
            var tipoPlanilla = await _context.TipoPlanillas
                .FirstOrDefaultAsync(m => m.Id == id);
            // Si no se encuentra el tipo de planilla con el id especificado, retorna NotFound.
            if (tipoPlanilla == null)
            {
                return NotFound();
            }
            // Si se encuentra el tipo de planilla, se pasa a la vista para mostrar los detalles.
            return View(tipoPlanilla);
        }

        // Acción que devuelve la vista para crear un nuevo tipo de planilla (GET)
        public IActionResult Create()
        {
            return View(); // Devuelve la vista "Create" sin ningún dato predefinido
        }

        // Acción que maneja la solicitud POST para crear un nuevo tipo de planilla
        // Utiliza [HttpPost] para indicar que se ejecuta cuando se envían datos a través de un formulario
        // [ValidateAntiForgeryToken] se utiliza para prevenir ataques CSRF (Cross-Site Request Forgery)        
        // POST: TipoPlanillas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NombreTipo")] TipoPlanilla tipoPlanilla)
        {
            // Verifica si el modelo que contiene los datos del formulario es válido
            // ModelState.IsValid es una propiedad que comprueba las validaciones de datos del modelo
            if (ModelState.IsValid)
            {
                _context.Add(tipoPlanilla);// Agrega el nuevo objeto tipoPlanilla al contexto para guardar en la base de datos
                await _context.SaveChangesAsync();// Guarda los cambios en la base de datos de manera asíncrona
                return RedirectToAction(nameof(Index));// Redirige a la acción "Index" para mostrar todos los tipos de planillas
            }
            // Si el modelo no es válido, retorna a la vista "Create" con los datos del formulario para corregirlos
            return View(tipoPlanilla);
        }


        // Acción GET para editar un tipo de planilla.
        // Verifica si el parámetro id es nulo. Si es nulo, devuelve un error 404 (NotFound).
        // Si el tipo de planilla no se encuentra, también devuelve NotFound.
        // Si el tipo de planilla es encontrado, devuelve la vista con el modelo (tipoPlanilla) para ser editado.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();// Si id es nulo, retorna NotFound
            }

            var tipoPlanilla = await _context.TipoPlanillas.FindAsync(id); // Busca el tipo de planilla por id
            if (tipoPlanilla == null)
            {
                return NotFound(); // Si no se encuentra el tipo de planilla, retorna NotFound
            }
            return View(tipoPlanilla); // Retorna la vista con los datos del tipo de planilla encontrado
        }

        // Acción POST para guardar los cambios de edición de un tipo de planilla.
        // Se utiliza [HttpPost] para manejar las solicitudes POST que envían datos desde un formulario.
        // Se utiliza [ValidateAntiForgeryToken] para proteger contra ataques CSRF (Cross-Site Request Forgery).
        // POST: TipoPlanillas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreTipo")] TipoPlanilla tipoPlanilla)
        {
            // Verifica si el id proporcionado en la URL coincide con el id del objeto tipoPlanilla
            // Si no coinciden, retorna NotFound.
            if (id != tipoPlanilla.Id)
            {
                return NotFound();
            }
            // Si el estado del modelo es válido, procede a intentar guardar los cambios
            if (ModelState.IsValid)
            {
                try
                {
                    // Busca el tipo de planilla existente en la base de datos
                    var existingTipoPlanilla = await _context.TipoPlanillas.FindAsync(id);
                    if (existingTipoPlanilla != null)
                    {
                        // Actualiza las propiedades del tipo de planilla con los nuevos valores
                        existingTipoPlanilla.NombreTipo = tipoPlanilla.NombreTipo;
                        existingTipoPlanilla.FechaModificacion = DateTime.Now;// Establece la fecha de modificación
                        _context.Update(existingTipoPlanilla); // Marca el objeto para actualizar en el contexto
                        await _context.SaveChangesAsync(); // Guarda los cambios en la base de datos
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Si ocurre una excepción de concurrencia, verifica si el tipo de planilla aún existe
                    if (!TipoPlanillaExists(tipoPlanilla.Id))
                    {
                        return NotFound();// Si no existe, retorna NotFound
                    }
                    else
                    {
                        throw;// Si hay otro tipo de error, vuelve a lanzar la excepción
                    }
                }
                return RedirectToAction(nameof(Index));// Redirige a la acción Index tras guardar los cambios
            }
            return View(tipoPlanilla);// Si el modelo no es válido, regresa a la vista con los errores
        }

        // Acción que maneja la eliminación de un tipo de planilla.
        // Recibe un parámetro "id" para identificar el registro a eliminar.
        // Si el "id" es nulo, retorna un error 404 (NotFound).
        // Busca en la base de datos el tipo de planilla con el "id" proporcionado.
        // Si no se encuentra, retorna un error 404.
        // Si se encuentra, envía la información a la vista de confirmación antes de eliminarlo.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Retorna un error 404 si el ID es nulo.
            }

            var tipoPlanilla = await _context.TipoPlanillas
                .FirstOrDefaultAsync(m => m.Id == id); // Busca el tipo de planilla con el ID proporcionado.

            if (tipoPlanilla == null)
            {
                return NotFound(); // Retorna un error 404 si el tipo de planilla no se encuentra.
            }

            return View(tipoPlanilla); // Envía la información del tipo de planilla a la vista de confirmación.
        }

        // POST: TipoPlanillas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var tipoPlanilla = await _context.TipoPlanillas.FindAsync(id);
            if (tipoPlanilla != null)
            {
                _context.TipoPlanillas.Remove(tipoPlanilla);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TipoPlanillaExists(int id)
        {
            return _context.TipoPlanillas.Any(e => e.Id == id);
        }
    }
}
