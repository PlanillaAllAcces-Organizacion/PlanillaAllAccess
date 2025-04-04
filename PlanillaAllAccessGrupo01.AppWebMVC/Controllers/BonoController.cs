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
    //Autorización de acceso al apartado de Bonos
    [Authorize(Roles = "Recursos Humanos")]
    public class BonoController : Controller
    {
        private readonly PlanillaDbContext _context;

        public BonoController(PlanillaDbContext context)
        {
            _context = context;
        }

        // Este método permite listar y buscar bonos de manera dinámica según los filtros proporcionados. 
        // Los filtros opcionales incluyen nombre del bono, tipo de planilla, estado y operación. 
        // Los resultados se ordenan por ID (descendente) y pueden limitarse a un número específico de registros (topRegistro). 
        // Devuelve una lista de bonos a la vista de manera asíncrona.
        public async Task<IActionResult> Index(Bono bono, int topRegistro = 10)
        {
            var query = _context.Bonos.AsQueryable();

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

            if (!string.IsNullOrWhiteSpace(bono.NombreBono))
                query = query.Where(s => s.NombreBono.Contains(bono.NombreBono));

            if (bono.Planilla > 0)
                query = query.Where(s => s.Planilla == bono.Planilla);

            if (bono.Estado > 0)
                query = query.Where(s => s.Estado == bono.Estado);

            if (bono.Operacion > 0)
                query = query.Where(s => s.Operacion == bono.Operacion);

            query = query.OrderByDescending(e => e.Id);

            if (topRegistro > 0)
                query = query.Take(topRegistro);

            
            return View(await query.ToListAsync());
        }

        // Este método muestra los detalles de un bono específico basado en su ID. 
        // Valida que se proporcione un ID, busca el bono en la base de datos y lo envía a la vista. 
        // Si no se encuentra el bono o el ID no es válido, devuelve un error (404 Not Found).
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var bono = await _context.Bonos.FirstOrDefaultAsync(m => m.Id == id);
            if (bono == null) return NotFound();

            return View(bono);
        }

        public IActionResult Create()
        {
            return View();
        }

        // POST: Bono/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        // Este método gestiona la creación de un nuevo bono en la base de datos.
        // Valida manualmente que las fechas proporcionadas sean correctas y asegura que la fecha de expiración sea posterior a la de validación.
        // Utiliza el modelo enlazado para evitar sobrecargas y para proteger la integridad de los datos.
        // Si los datos son válidos, guarda el bono y redirige al listado general (Index). 
        // En caso de errores de validación, retorna el formulario con mensajes específicos para corrección.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NombreBono,Valor,Estado,FechaValidacion,FechaExpiracion,Operacion,Planilla")] Bono bono)
        {
            if (bono.FechaValidacion != null && bono.FechaExpiracion != null)
            {
                if (bono.FechaExpiracion <= bono.FechaValidacion)
                {
                    ModelState.AddModelError("FechaExpiracion", "La fecha de expiración debe ser mayor que la fecha de validación.");
                }
            }
            if (ModelState.IsValid)
            {
                _context.Add(bono);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(bono);
        }

        // Este método gestiona la edición de un bono existente en dos pasos:
        // 1. GET: Obtiene el bono por su ID y presenta los datos actuales en un formulario.
        // 2. POST: Valida y procesa las modificaciones enviadas desde el formulario.
        // El método maneja la concurrencia para evitar conflictos si otros usuarios modifican el mismo registro.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var bono = await _context.Bonos.FindAsync(id);
            if (bono == null)return NotFound();

            return View(bono);
        }

        // POST: Bono/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreBono,Valor,Estado,FechaValidacion,FechaExpiracion,Operacion,Planilla")] Bono bono)
        {
            if (id != bono.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bono);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BonoExists(bono.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(bono);
        }

        // Este método permite mostrar los detalles de un bono específico antes de proceder con su eliminación.
        // Primero, valida que el ID proporcionado sea válido y no nulo.
        // Luego, busca en la base de datos el bono asociado al ID.
        // Si el bono existe, lo envía a la vista para confirmar la eliminación. 
        // Si el bono no existe o el ID es inválido, devuelve un error (404 Not Found).
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bono = await _context.Bonos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (bono == null)
            {
                return NotFound();
            }

            return View(bono);
        }

        // Este método permite confirmar la eliminación de un bono en la base de datos. 
        // Primero verifica si el bono existe y si está asignado a algún empleado antes de proceder.
        // Si el bono está asignado, muestra un mensaje de error y no permite la eliminación.
        // Si el bono no está asignado, lo elimina de la base de datos y redirige al listado general (Index).
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Incluimos las asignaciones para poder comprobar si existen
            var bono = await _context.Bonos
                .Include(b => b.AsignacionBonos)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bono == null)
            {
                return NotFound();
            }

            if (bono.AsignacionBonos.Any())
            {
                TempData["ErrorMessage"] = "No se puede eliminar este bono porque está asignado a uno o más empleados.";
                return RedirectToAction(nameof(Index));
            }

            _context.Bonos.Remove(bono);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Este método verifica si existe un bono en la base de datos según su ID.
        // Utiliza LINQ para consultar la tabla de bonos y devuelve un valor booleano.
        // Si encuentra un registro con el ID especificado, retorna "true"; de lo contrario, retorna "false".
        private bool BonoExists(int id)
        {
            return _context.Bonos.Any(e => e.Id == id);
        }
    }
}
