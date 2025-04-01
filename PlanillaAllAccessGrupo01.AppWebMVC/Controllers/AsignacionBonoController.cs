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





        //public IActionResult Create()
        //{
        //    ViewData["BonosId"] = new SelectList(_context.Bonos, "Id", "NombreBono");
        //    ViewData["EmpleadosId"] = new SelectList(_context.Empleados, "Id", "Apellido");
        //    return View();
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,EmpleadosId,BonosId,Estado")] AsignacionBono asignacionBono)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _context.Add(asignacionBono);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["BonosId"] = new SelectList(_context.Bonos, "Id", "NombreBono", asignacionBono.BonosId);
        //    ViewData["EmpleadosId"] = new SelectList(_context.Empleados, "Id", "Apellido", asignacionBono.EmpleadosId);
        //    return View(asignacionBono);
        //}

        // GET: AsignacionBono/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asignacionBono = await _context.AsignacionBonos.FindAsync(id);
            if (asignacionBono == null)
            {
                return NotFound();
            }
            ViewData["BonosId"] = new SelectList(_context.Bonos, "Id", "NombreBono", asignacionBono.BonosId);
            ViewData["EmpleadosId"] = new SelectList(_context.Empleados, "Id", "Apellido", asignacionBono.EmpleadosId);
            return View(asignacionBono);
        }

        // POST: AsignacionBono/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmpleadosId,BonosId,Estado")] AsignacionBono asignacionBono)
        {
            if (id != asignacionBono.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(asignacionBono);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AsignacionBonoExists(asignacionBono.Id))
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
            ViewData["BonosId"] = new SelectList(_context.Bonos, "Id", "NombreBono", asignacionBono.BonosId);
            ViewData["EmpleadosId"] = new SelectList(_context.Empleados, "Id", "Apellido", asignacionBono.EmpleadosId);
            return View(asignacionBono);
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
