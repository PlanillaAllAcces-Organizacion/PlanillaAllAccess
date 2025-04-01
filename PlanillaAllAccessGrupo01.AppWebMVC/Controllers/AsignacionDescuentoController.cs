using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlanillaAllAccessGrupo01.AppWebMVC.Models;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Controllers
{
    public class AsignacionDescuentoController : Controller
    {
        private readonly PlanillaDbContext _context;

        public AsignacionDescuentoController(PlanillaDbContext context)
        {
            _context = context;
        }

        // GET: AsignacionDescuento
        public async Task<IActionResult> Index()
        {
            var planillaDbContext = _context.AsignacionDescuentos.Include(a => a.Descuentos).Include(a => a.Empleados);
            return View(await planillaDbContext.ToListAsync());
        }

        // GET: AsignacionDescuento/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asignacionDescuento = await _context.AsignacionDescuentos
                .Include(a => a.Descuentos)
                .Include(a => a.Empleados)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (asignacionDescuento == null)
            {
                return NotFound();
            }

            return View(asignacionDescuento);
        }


        public IActionResult Create(int? empleadoId)
        {
            if (empleadoId == null)
            {
                return NotFound();
            }

            var empleado = _context.Empleados.Include(e => e.PuestoTrabajo).FirstOrDefault(e => e.Id == empleadoId);


            ViewBag.EmpleadoId = empleado.Id;
            ViewBag.EmpleadoNombre = empleado.Nombre;
            ViewBag.EmpleadoDUI = empleado.Dui;
            ViewBag.EmpleadoPuesto = empleado.PuestoTrabajo.NombrePuesto;
            ViewBag.EmpleadoSalario = empleado.SalarioBase;
            ViewBag.Descuentos = _context.Descuentos.ToList();

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int empleadoId, List<int> descuentosSeleccionados)
        {
            if (descuentosSeleccionados == null || !descuentosSeleccionados.Any())
            {
                TempData["ErrorMessage"] = "Debe seleccionar al menos un descuento.";
                return RedirectToAction("Create", new { empleadoId });
            }

            foreach (var descuentoId in descuentosSeleccionados)
            {
                var asignacion = new AsignacionDescuento
                {
                    EmpleadosId = empleadoId,
                    DescuentosId = descuentoId
                };
                _context.AsignacionDescuentos.Add(asignacion);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Descuentos asignados correctamente.";
            return RedirectToAction("Index", "Empleado");
        }



        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Obtener la asignación de descuento
            var asignacionDescuento = await _context.AsignacionDescuentos
                .Include(a => a.Descuentos)
                .Include(a => a.Empleados)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (asignacionDescuento == null)
            {
                return NotFound();
            }

            // Obtener todos los descuentos disponibles
            var descuentosDisponibles = await _context.Descuentos.ToListAsync();

            // Obtener los descuentos ya asignados al empleado
            var descuentosAsignados = await _context.AsignacionDescuentos
                .Where(a => a.EmpleadosId == asignacionDescuento.EmpleadosId)
                .Select(a => a.DescuentosId)
                .ToListAsync();

            // Pasar los datos a la vista
            ViewBag.DescuentosDisponibles = descuentosDisponibles;
            ViewBag.DescuentosAsignados = descuentosAsignados;
            ViewBag.EmpleadoNombre = $"{asignacionDescuento.Empleados.Nombre} {asignacionDescuento.Empleados.Apellido}";

            return View(asignacionDescuento);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, List<int> descuentosSeleccionados)
        {
            // Obtener la asignación de descuento existente
            var asignacionDescuento = await _context.AsignacionDescuentos
                .FirstOrDefaultAsync(a => a.Id == id);

            if (asignacionDescuento == null)
            {
                return NotFound();
            }

            // Eliminar los descuentos previamente asignados
            var descuentosPrevios = _context.AsignacionDescuentos
                .Where(a => a.EmpleadosId == asignacionDescuento.EmpleadosId)
                .ToList();

            _context.AsignacionDescuentos.RemoveRange(descuentosPrevios);

            // Agregar los nuevos descuentos seleccionados
            foreach (var descuentoId in descuentosSeleccionados)
            {
                _context.AsignacionDescuentos.Add(new AsignacionDescuento
                {
                    EmpleadosId = asignacionDescuento.EmpleadosId,
                    DescuentosId = descuentoId
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Empleado");
        }


        // GET: AsignacionDescuento/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asignacionDescuento = await _context.AsignacionDescuentos
                .Include(a => a.Descuentos)
                .Include(a => a.Empleados)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (asignacionDescuento == null)
            {
                return NotFound();
            }

            return View(asignacionDescuento);
        }

        // POST: AsignacionDescuento/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var asignacionDescuento = await _context.AsignacionDescuentos.FindAsync(id);
            if (asignacionDescuento != null)
            {
                _context.AsignacionDescuentos.Remove(asignacionDescuento);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AsignacionDescuentoExists(int id)
        {
            return _context.AsignacionDescuentos.Any(e => e.Id == id);
        }
    }
}
