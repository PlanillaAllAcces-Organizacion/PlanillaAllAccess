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
    //Autorización de acceso a asignación de bonos
    [Authorize(Roles = "Recursos Humanos")]
    public class AsignacionBonoController : Controller
    {
        private readonly PlanillaDbContext _context;

        public AsignacionBonoController(PlanillaDbContext context)
        {
            _context = context;
        }

        // GET: AsignacionBono
        public async Task<IActionResult> Index()
        {
            var planillaDbContext = _context.AsignacionBonos.Include(a => a.Bonos).Include(a => a.Empleados);
            return View(await planillaDbContext.ToListAsync());
        }

        // GET: AsignacionBono/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asignacionBono = await _context.AsignacionBonos
                .Include(a => a.Bonos)
                .Include(a => a.Empleados)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (asignacionBono == null)
            {
                return NotFound();
            }

            return View(asignacionBono);
        }




        //GET CREATE ASIGNACION DE BONOS, muestra la información y la lista de bonos al empleado seleccionado
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
            ViewBag.Bonos = _context.Bonos.ToList();


            return View();
        }

        //POST CREATE ASIGNACION BONO, funciona para la lógica de guardar los bonos seleccionados para ese empleado

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int empleadoId, List<int> bonosSeleccionados)
        {
            if (empleadoId == 0 || bonosSeleccionados == null || !bonosSeleccionados.Any())
            {
                TempData["ErrorMessage"] = "Debe seleccionar al menos un bono.";
                return RedirectToAction("Create", new { empleadoId });
            }

            foreach (var bonoId in bonosSeleccionados)
            {
                var asignacionBono = new AsignacionBono
                {
                    EmpleadosId = empleadoId,
                    BonosId = bonoId,

                };
                _context.AsignacionBonos.Add(asignacionBono);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Bonos asignados correctamente.";
            return RedirectToAction("Index", "Empleado");
        }


        //GET EDIT Permite la visualizacion de datos del empleado y los bonos que tiene asignados.

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asignacionBono = await _context.AsignacionBonos
                .Include(a => a.Bonos)
                .Include(a => a.Empleados)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (asignacionBono == null)
            {
                return NotFound();
            }

            // Obtener todos los bonos disponibles
            var bonosDisponibles = _context.Bonos.ToList();

            // Obtener bonos ya asignados al empleado
            var bonosAsignados = _context.AsignacionBonos
                .Where(a => a.EmpleadosId == asignacionBono.EmpleadosId)
                .Select(a => a.BonosId)
                .ToList();

            ViewBag.BonosDisponibles = bonosDisponibles;
            ViewBag.BonosAsignados = bonosAsignados;
            ViewBag.EmpleadoNombre = $"{asignacionBono.Empleados.Nombre} {asignacionBono.Empleados.Apellido}";

            return View(asignacionBono);
        }

        //POST EDIT Permite agregar o quitar bonos y actualizar la lista de descuentos seleccionados

        // POST: AsignacionBono/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, List<int> bonosSeleccionados)
        {
            var asignacionBono = await _context.AsignacionBonos
                .Where(a => a.Id == id)
                .FirstOrDefaultAsync();

            if (asignacionBono == null)
            {
                return NotFound();
            }

            // Eliminar los bonos previamente asignados
            var bonosPrevios = _context.AsignacionBonos
                .Where(a => a.EmpleadosId == asignacionBono.EmpleadosId)
                .ToList();

            _context.AsignacionBonos.RemoveRange(bonosPrevios);

            // Agregar los bonos nuevos seleccionados
            foreach (var bonoId in bonosSeleccionados)
            {
                _context.AsignacionBonos.Add(new AsignacionBono
                {
                    EmpleadosId = asignacionBono.EmpleadosId,
                    BonosId = bonoId,
                    Estado = asignacionBono.Estado
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Empleado");
        }






        // GET: AsignacionBono/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asignacionBono = await _context.AsignacionBonos
                .Include(a => a.Bonos)
                .Include(a => a.Empleados)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (asignacionBono == null)
            {
                return NotFound();
            }

            return View(asignacionBono);
        }

        // POST: AsignacionBono/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var asignacionBono = await _context.AsignacionBonos.FindAsync(id);
            if (asignacionBono != null)
            {
                _context.AsignacionBonos.Remove(asignacionBono);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AsignacionBonoExists(int id)
        {
            return _context.AsignacionBonos.Any(e => e.Id == id);
        }
    }
}
