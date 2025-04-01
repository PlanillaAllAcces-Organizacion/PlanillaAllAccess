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

        // GET: AsignacionDescuento/Create
        public IActionResult Create()
        {
            ViewData["DescuentosId"] = new SelectList(_context.Descuentos, "Id", "Nombre");
            ViewData["EmpleadosId"] = new SelectList(_context.Empleados, "Id", "Apellido");
            return View();
        }

        // POST: AsignacionDescuento/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmpleadosId,DescuentosId")] AsignacionDescuento asignacionDescuento)
        {
            if (ModelState.IsValid)
            {
                _context.Add(asignacionDescuento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DescuentosId"] = new SelectList(_context.Descuentos, "Id", "Nombre", asignacionDescuento.DescuentosId);
            ViewData["EmpleadosId"] = new SelectList(_context.Empleados, "Id", "Apellido", asignacionDescuento.EmpleadosId);
            return View(asignacionDescuento);
        }

        // GET: AsignacionDescuento/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asignacionDescuento = await _context.AsignacionDescuentos.FindAsync(id);
            if (asignacionDescuento == null)
            {
                return NotFound();
            }
            ViewData["DescuentosId"] = new SelectList(_context.Descuentos, "Id", "Nombre", asignacionDescuento.DescuentosId);
            ViewData["EmpleadosId"] = new SelectList(_context.Empleados, "Id", "Apellido", asignacionDescuento.EmpleadosId);
            return View(asignacionDescuento);
        }

        // POST: AsignacionDescuento/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmpleadosId,DescuentosId")] AsignacionDescuento asignacionDescuento)
        {
            if (id != asignacionDescuento.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(asignacionDescuento);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AsignacionDescuentoExists(asignacionDescuento.Id))
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
            ViewData["DescuentosId"] = new SelectList(_context.Descuentos, "Id", "Nombre", asignacionDescuento.DescuentosId);
            ViewData["EmpleadosId"] = new SelectList(_context.Empleados, "Id", "Apellido", asignacionDescuento.EmpleadosId);
            return View(asignacionDescuento);
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
