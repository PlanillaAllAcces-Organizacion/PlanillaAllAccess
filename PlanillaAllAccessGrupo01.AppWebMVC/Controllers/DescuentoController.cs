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
    // Restringe el acceso solo a usuarios con rol "Recursos Humanos"
    [Authorize(Roles = "Recursos Humanos")]
    public class DescuentoController : Controller
    {
        private readonly PlanillaDbContext _context; // Contexto de base de datos

        // Constructor que inyecta el contexto de la base de datos
        public DescuentoController(PlanillaDbContext context)
        {
            _context = context;
        }

        // GET: Descuento
        // Muestra la lista de descuentos con capacidades de filtrado
        public async Task<IActionResult> Index(Descuento descuento, int topRegistro = 10)
        {
            // Consulta base para los descuentos
            var query = _context.Descuentos.AsQueryable();

            // Consulta adicional para mostrar asignaciones de descuentos a empleados
            var planillaDbContext = _context.AsignacionDescuentos
               .Include(a => a.Descuentos)
               .Include(a => a.Empleados)
               .Select(a => new
               {
                   a.Id,
                   EmpleadoNombre = a.Empleados.Nombre,
                   DescuentoNombre = a.Descuentos.Nombre,
                   ValorDescuento = a.Descuentos.Valor,
                   EsOperacionFija = a.Descuentos.Operacion
               });

            // Aplicación de filtros según los parámetros recibidos
            if (!string.IsNullOrWhiteSpace(descuento.Nombre))
                query = query.Where(s => s.Nombre.Contains(descuento.Nombre));

            if (descuento.Planilla > 0)
                query = query.Where(s => s.Planilla == descuento.Planilla);

            if (descuento.Estado > 0)
                query = query.Where(s => s.Estado == descuento.Estado);

            if (descuento.Operacion > 0)
                query = query.Where(s => s.Operacion == descuento.Operacion);

            // Ordenamiento y limitación de resultados
            query = query.OrderByDescending(e => e.Id);

            if (topRegistro > 0)
                query = query.Take(topRegistro);

            // Ejecución de la consulta
            var listaDescuentos = await query.ToListAsync();

            // Asignación de fechas por defecto si no están establecidas
            foreach (var descuentoItem in listaDescuentos)
            {
                descuentoItem.FechaValidacion = DateOnly.FromDateTime(DateTime.Now);
                descuentoItem.FechaExpiracion = DateOnly.FromDateTime(DateTime.Now.AddMonths(1));
            }

            return View(listaDescuentos);
        }

        // GET: Descuento/Details/5
        // Muestra los detalles de un descuento específico
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) // Validación de ID nulo
            {
                return NotFound();
            }

            // Búsqueda del descuento
            var descuento = await _context.Descuentos
                .FirstOrDefaultAsync(m => m.Id == id);

            if (descuento == null) // Validación de existencia
            {
                return NotFound();
            }

            // Asignación de fechas por defecto si no están establecidas
            descuento.FechaValidacion ??= DateOnly.FromDateTime(DateTime.Now);
            descuento.FechaExpiracion ??= DateOnly.FromDateTime(DateTime.Now.AddMonths(1));

            return View(descuento);
        }

        // GET: Descuento/Create
        // Muestra el formulario de creación de descuentos
        public IActionResult Create()
        {
            return View();
        }

        // POST: Descuento/Create
        // Procesa el formulario de creación de descuentos
        [HttpPost]
        [ValidateAntiForgeryToken] // Protección contra CSRF
        public async Task<IActionResult> Create([Bind("Id,Nombre,Valor,Estado,FechaValidacion,FechaExpiracion,Operacion,Planilla")] Descuento descuento)
        {
            // Validación personalizada de fechas
            if (descuento.FechaValidacion != null && descuento.FechaExpiracion != null)
            {
                if (descuento.FechaExpiracion <= descuento.FechaValidacion)
                {
                    ModelState.AddModelError("FechaExpiracion", "La fecha de expiración debe ser mayor que la fecha de validación.");
                }
            }

            // Si el modelo es válido, guarda el descuento
            if (ModelState.IsValid)
            {
                _context.Add(descuento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(descuento);
        }

        // GET: Descuento/Edit/5
        // Muestra el formulario de edición de descuentos
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) // Validación de ID nulo
            {
                return NotFound();
            }

            // Búsqueda del descuento
            var descuento = await _context.Descuentos.FindAsync(id);
            if (descuento == null) // Validación de existencia
            {
                return NotFound();
            }

            // Asignación de fechas por defecto si no están establecidas
            descuento.FechaValidacion ??= DateOnly.FromDateTime(DateTime.Now);
            descuento.FechaExpiracion ??= DateOnly.FromDateTime(DateTime.Now.AddMonths(1));

            return View(descuento);
        }

        // POST: Descuento/Edit/5
        // Procesa el formulario de edición de descuentos
        [HttpPost]
        [ValidateAntiForgeryToken] // Protección contra CSRF
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Valor,Estado,FechaValidacion,FechaExpiracion,Operacion,Planilla")] Descuento descuento)
        {
            if (id != descuento.Id) // Validación de coincidencia de IDs
            {
                return NotFound();
            }

            // Validación personalizada de fechas
            if (descuento.FechaValidacion != null && descuento.FechaExpiracion != null)
            {
                if (descuento.FechaExpiracion <= descuento.FechaValidacion)
                {
                    ModelState.AddModelError("FechaExpiracion", "La fecha de expiración debe ser mayor que la fecha de validación.");
                }
            }

            // Si el modelo es válido, actualiza el descuento
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(descuento);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DescuentoExists(descuento.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(descuento);
        }

        // GET: Descuento/Delete/5
        // Muestra la confirmación de eliminación
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) // Validación de ID nulo
            {
                return NotFound();
            }

            // Búsqueda del descuento
            var descuento = await _context.Descuentos
                .FirstOrDefaultAsync(m => m.Id == id);

            if (descuento == null) // Validación de existencia
            {
                return NotFound();
            }

            // Asignación de fechas por defecto si no están establecidas
            descuento.FechaValidacion ??= DateOnly.FromDateTime(DateTime.Now);
            descuento.FechaExpiracion ??= DateOnly.FromDateTime(DateTime.Now.AddMonths(1));

            return View(descuento);
        }

        // POST: Descuento/Delete/5
        // Procesa la eliminación del descuento
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken] // Protección contra CSRF
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Búsqueda del descuento incluyendo sus asignaciones
            var descuento = await _context.Descuentos
                .Include(b => b.AsignacionDescuentos)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (descuento == null) // Validación de existencia
            {
                return NotFound();
            }

            // Validación de que el descuento no esté asignado a empleados
            if (descuento.AsignacionDescuentos.Any())
            {
                TempData["ErrorMessage"] = "No se puede eliminar este descuento porque está asignado a uno o más empleados.";
                return RedirectToAction(nameof(Index));
            }

            // Eliminación del descuento
            _context.Descuentos.Remove(descuento);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Método auxiliar para verificar existencia de descuento
        private bool DescuentoExists(int id)
        {
            return _context.Descuentos.Any(e => e.Id == id);
        }
    }
}