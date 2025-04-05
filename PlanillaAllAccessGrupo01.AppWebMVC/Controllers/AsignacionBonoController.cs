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
    public class AsignacionBonoController : Controller
    {
        private readonly PlanillaDbContext _context; // Contexto de base de datos

        // Constructor que inyecta el contexto de la base de datos
        public AsignacionBonoController(PlanillaDbContext context)
        {
            _context = context;
        }

        // GET: AsignacionBono
        // Muestra la lista de todas las asignaciones de bonos
        public async Task<IActionResult> Index()
        {
            // Incluye los datos relacionados de Bonos y Empleados
            var planillaDbContext = _context.AsignacionBonos
                .Include(a => a.Bonos)
                .Include(a => a.Empleados);
            return View(await planillaDbContext.ToListAsync());
        }

        // GET: AsignacionBono/Details/5
        // Muestra los detalles de una asignación específica
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) // Verifica si el ID es nulo
            {
                return NotFound();
            }

            // Busca la asignación incluyendo datos relacionados
            var asignacionBono = await _context.AsignacionBonos
                .Include(a => a.Bonos)
                .Include(a => a.Empleados)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (asignacionBono == null) // Verifica si se encontró la asignación
            {
                return NotFound();
            }

            return View(asignacionBono);
        }

        // GET: AsignacionBono/Create
        // Muestra el formulario para crear nuevas asignaciones de bonos
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
            ViewBag.Bonos = _context.Bonos.ToList(); // Lista de todos los bonos disponibles

            return View();
        }

        // POST: AsignacionBono/Create
        // Procesa el formulario de creación de asignaciones de bonos
        [HttpPost]
        [ValidateAntiForgeryToken] // Protección contra CSRF
        public async Task<IActionResult> Create(int empleadoId, List<int> bonosSeleccionados)
        {
            // Valida que se hayan seleccionado bonos y que el empleadoId sea válido
            if (empleadoId == 0 || bonosSeleccionados == null || !bonosSeleccionados.Any())
            {
                TempData["ErrorMessage"] = "Debe seleccionar al menos un bono.";
                return RedirectToAction("Create", new { empleadoId });
            }

            // Crea una nueva asignación por cada bono seleccionado
            foreach (var bonoId in bonosSeleccionados)
            {
                var asignacionBono = new AsignacionBono
                {
                    EmpleadosId = empleadoId,
                    BonosId = bonoId
                };
                _context.AsignacionBonos.Add(asignacionBono);
            }

            await _context.SaveChangesAsync(); // Guarda los cambios en la base de datos
            TempData["SuccessMessage"] = "Bonos asignados correctamente.";
            return RedirectToAction("Index", "Empleado"); // Redirige al listado de empleados
        }

        // GET: AsignacionBono/Edit/5
        // Muestra el formulario para editar asignaciones de bonos
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) // Verifica si el ID es nulo
            {
                return NotFound();
            }

            // Obtiene la asignación de bono con datos relacionados
            var asignacionBono = await _context.AsignacionBonos
                .Include(a => a.Bonos)
                .Include(a => a.Empleados)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (asignacionBono == null) // Verifica si se encontró la asignación
            {
                return NotFound();
            }

            // Obtiene todos los bonos disponibles
            var bonosDisponibles = _context.Bonos.ToList();

            // Obtiene los IDs de los bonos ya asignados al empleado
            var bonosAsignados = _context.AsignacionBonos
                .Where(a => a.EmpleadosId == asignacionBono.EmpleadosId)
                .Select(a => a.BonosId)
                .ToList();

            // Pasa los datos a la vista
            ViewBag.BonosDisponibles = bonosDisponibles;
            ViewBag.BonosAsignados = bonosAsignados;
            ViewBag.EmpleadoNombre = $"{asignacionBono.Empleados.Nombre} {asignacionBono.Empleados.Apellido}";

            return View(asignacionBono);
        }

        // POST: AsignacionBono/Edit/5
        // Procesa el formulario de edición de asignaciones de bonos
        [HttpPost]
        [ValidateAntiForgeryToken] // Protección contra CSRF
        public async Task<IActionResult> Edit(int id, List<int> bonosSeleccionados)
        {
            // Obtiene la asignación existente
            var asignacionBono = await _context.AsignacionBonos
                .Where(a => a.Id == id)
                .FirstOrDefaultAsync();

            if (asignacionBono == null) // Verifica si se encontró la asignación
            {
                return NotFound();
            }

            // Elimina todas las asignaciones previas del empleado
            var bonosPrevios = _context.AsignacionBonos
                .Where(a => a.EmpleadosId == asignacionBono.EmpleadosId)
                .ToList();

            _context.AsignacionBonos.RemoveRange(bonosPrevios);

            // Crea nuevas asignaciones con los bonos seleccionados
            foreach (var bonoId in bonosSeleccionados)
            {
                _context.AsignacionBonos.Add(new AsignacionBono
                {
                    EmpleadosId = asignacionBono.EmpleadosId,
                    BonosId = bonoId,
                    Estado = asignacionBono.Estado // Mantiene el estado original
                });
            }

            await _context.SaveChangesAsync(); // Guarda los cambios
            return RedirectToAction("Index", "Empleado"); // Redirige al listado de empleados
        }

        // GET: AsignacionBono/Delete/5
        // Muestra la confirmación para eliminar una asignación
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) // Verifica si el ID es nulo
            {
                return NotFound();
            }

            // Obtiene la asignación con datos relacionados
            var asignacionBono = await _context.AsignacionBonos
                .Include(a => a.Bonos)
                .Include(a => a.Empleados)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (asignacionBono == null) // Verifica si se encontró la asignación
            {
                return NotFound();
            }

            return View(asignacionBono);
        }

        // POST: AsignacionBono/Delete/5
        // Procesa la eliminación de una asignación
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken] // Protección contra CSRF
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Busca la asignación a eliminar
            var asignacionBono = await _context.AsignacionBonos.FindAsync(id);
            if (asignacionBono != null) // Verifica si se encontró
            {
                _context.AsignacionBonos.Remove(asignacionBono); // Marca para eliminación
            }

            await _context.SaveChangesAsync(); // Ejecuta la eliminación en la base de datos
            return RedirectToAction(nameof(Index)); // Redirige al listado
        }

        // Método auxiliar para verificar si existe una asignación
        private bool AsignacionBonoExists(int id)
        {
            return _context.AsignacionBonos.Any(e => e.Id == id);
        }
    }
}