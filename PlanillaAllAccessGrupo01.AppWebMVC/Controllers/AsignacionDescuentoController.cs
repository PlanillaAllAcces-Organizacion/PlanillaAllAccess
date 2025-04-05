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
    [Authorize(Roles = "Recursos Humanos")] // Restringe el acceso solo a usuarios con rol "Recursos Humanos"
    public class AsignacionDescuentoController : Controller
    {
        private readonly PlanillaDbContext _context; // Contexto de base de datos

        // Constructor que inyecta el contexto de la base de datos
        public AsignacionDescuentoController(PlanillaDbContext context)
        {
            _context = context;
        }

        // GET: AsignacionDescuento
        // Muestra la lista de todas las asignaciones de descuentos
        public async Task<IActionResult> Index()
        {
            // Incluye los datos relacionados de Descuentos y Empleados
            var planillaDbContext = _context.AsignacionDescuentos
                .Include(a => a.Descuentos)
                .Include(a => a.Empleados);
            return View(await planillaDbContext.ToListAsync());
        }

        // GET: AsignacionDescuento/Details/5
        // Muestra los detalles de una asignación específica
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) // Verifica si el ID es nulo
            {
                return NotFound();
            }

            // Busca la asignación incluyendo datos relacionados
            var asignacionDescuento = await _context.AsignacionDescuentos
                .Include(a => a.Descuentos)
                .Include(a => a.Empleados)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (asignacionDescuento == null) // Verifica si se encontró la asignación
            {
                return NotFound();
            }

            return View(asignacionDescuento);
        }

        // GET: AsignacionDescuento/Create
        // Muestra el formulario para crear nuevas asignaciones de descuentos
        public IActionResult Create(int? empleadoId)
        {
            if (empleadoId == null) // Verifica si se proporcionó un ID de empleado
            {
                return NotFound();
            }

            // Obtiene los datos del empleado incluyendo su puesto de trabajo
            var empleado = _context.Empleados
                .Include(e => e.PuestoTrabajo)
                .FirstOrDefault(e => e.Id == empleadoId);

            // Pasa los datos del empleado a la vista usando ViewBag
            ViewBag.EmpleadoId = empleado.Id;
            ViewBag.EmpleadoNombre = empleado.Nombre;
            ViewBag.EmpleadoDUI = empleado.Dui;
            ViewBag.EmpleadoPuesto = empleado.PuestoTrabajo.NombrePuesto;
            ViewBag.EmpleadoSalario = empleado.SalarioBase;
            ViewBag.Descuentos = _context.Descuentos.ToList(); // Lista de todos los descuentos disponibles

            return View();
        }

        // POST: AsignacionDescuento/Create
        // Procesa el formulario de creación de asignaciones de descuentos
        [HttpPost]
        [ValidateAntiForgeryToken] // Protección contra CSRF
        public async Task<IActionResult> Create(int empleadoId, List<int> descuentosSeleccionados)
        {
            // Valida que se hayan seleccionado descuentos
            if (descuentosSeleccionados == null || !descuentosSeleccionados.Any())
            {
                TempData["ErrorMessage"] = "Debe seleccionar al menos un descuento.";
                return RedirectToAction("Create", new { empleadoId });
            }

            // Crea una nueva asignación por cada descuento seleccionado
            foreach (var descuentoId in descuentosSeleccionados)
            {
                var asignacion = new AsignacionDescuento
                {
                    EmpleadosId = empleadoId,
                    DescuentosId = descuentoId
                };
                _context.AsignacionDescuentos.Add(asignacion);
            }

            await _context.SaveChangesAsync(); // Guarda los cambios en la base de datos
            TempData["SuccessMessage"] = "Descuentos asignados correctamente.";
            return RedirectToAction("Index", "Empleado"); // Redirige al listado de empleados
        }

        // GET: AsignacionDescuento/Edit/5
        // Muestra el formulario para editar asignaciones de descuentos
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) // Verifica si el ID es nulo
            {
                return NotFound();
            }

            // Obtiene la asignación de descuento con datos relacionados
            var asignacionDescuento = await _context.AsignacionDescuentos
                .Include(a => a.Descuentos)
                .Include(a => a.Empleados)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (asignacionDescuento == null) // Verifica si se encontró la asignación
            {
                return NotFound();
            }

            // Obtiene todos los descuentos disponibles
            var descuentosDisponibles = await _context.Descuentos.ToListAsync();

            // Obtiene los IDs de los descuentos ya asignados al empleado
            var descuentosAsignados = await _context.AsignacionDescuentos
                .Where(a => a.EmpleadosId == asignacionDescuento.EmpleadosId)
                .Select(a => a.DescuentosId)
                .ToListAsync();

            // Pasa los datos a la vista
            ViewBag.DescuentosDisponibles = descuentosDisponibles;
            ViewBag.DescuentosAsignados = descuentosAsignados;
            ViewBag.EmpleadoNombre = $"{asignacionDescuento.Empleados.Nombre} {asignacionDescuento.Empleados.Apellido}";

            return View(asignacionDescuento);
        }

        // POST: AsignacionDescuento/Edit/5
        // Procesa el formulario de edición de asignaciones de descuentos
        [HttpPost]
        [ValidateAntiForgeryToken] // Protección contra CSRF
        public async Task<IActionResult> Edit(int id, List<int> descuentosSeleccionados)
        {
            // Obtiene la asignación existente
            var asignacionDescuento = await _context.AsignacionDescuentos
                .FirstOrDefaultAsync(a => a.Id == id);

            if (asignacionDescuento == null) // Verifica si se encontró la asignación
            {
                return NotFound();
            }

            // Elimina todas las asignaciones previas del empleado
            var descuentosPrevios = _context.AsignacionDescuentos
                .Where(a => a.EmpleadosId == asignacionDescuento.EmpleadosId)
                .ToList();

            _context.AsignacionDescuentos.RemoveRange(descuentosPrevios);

            // Crea nuevas asignaciones con los descuentos seleccionados
            foreach (var descuentoId in descuentosSeleccionados)
            {
                _context.AsignacionDescuentos.Add(new AsignacionDescuento
                {
                    EmpleadosId = asignacionDescuento.EmpleadosId,
                    DescuentosId = descuentoId
                });
            }

            await _context.SaveChangesAsync(); // Guarda los cambios
            return RedirectToAction("Index", "Empleado"); // Redirige al listado de empleados
        }

        // GET: AsignacionDescuento/Delete/5
        // Muestra la confirmación para eliminar una asignación
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) // Verifica si el ID es nulo
            {
                return NotFound();
            }

            // Obtiene la asignación con datos relacionados
            var asignacionDescuento = await _context.AsignacionDescuentos
                .Include(a => a.Descuentos)
                .Include(a => a.Empleados)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (asignacionDescuento == null) // Verifica si se encontró la asignación
            {
                return NotFound();
            }

            return View(asignacionDescuento);
        }

        // POST: AsignacionDescuento/Delete/5
        // Procesa la eliminación de una asignación
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken] // Protección contra CSRF
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Busca la asignación a eliminar
            var asignacionDescuento = await _context.AsignacionDescuentos.FindAsync(id);
            if (asignacionDescuento != null) // Verifica si se encontró
            {
                _context.AsignacionDescuentos.Remove(asignacionDescuento); // Marca para eliminación
            }

            await _context.SaveChangesAsync(); // Ejecuta la eliminación en la base de datos
            return RedirectToAction(nameof(Index)); // Redirige al listado
        }

        // Método auxiliar para verificar si existe una asignación
        private bool AsignacionDescuentoExists(int id)
        {
            return _context.AsignacionDescuentos.Any(e => e.Id == id);
        }
    }
}