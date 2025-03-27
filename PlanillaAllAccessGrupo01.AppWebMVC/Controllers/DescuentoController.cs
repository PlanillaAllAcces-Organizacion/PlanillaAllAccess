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
    public class DescuentoController : Controller
    {
        private readonly PlanillaDbContext _context;

        public DescuentoController(PlanillaDbContext context)
        {
            _context = context;
        }

        // GET: Descuento
        public async Task<IActionResult> Index(Descuento descuento, int topRegistro = 10)
        {
            var query = _context.Descuentos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(descuento.Nombre))
                query = query.Where(s => s.Nombre.Contains(descuento.Nombre));

            if (descuento.Planilla > 0)
                query = query.Where(s => s.Planilla == descuento.Planilla);

            if (descuento.Estado > 0)
                query = query.Where(s => s.Estado == descuento.Estado);

            if (descuento.Operacion > 0)
                query = query.Where(s => s.Operacion == descuento.Operacion);

            if (topRegistro > 0)
                query = query.Take(topRegistro);

            var listaDescuentos = await query.ToListAsync();
            foreach (var descuentoItem in listaDescuentos)
            {
                descuentoItem.FechaValidacion = DateOnly.FromDateTime(DateTime.Now); // Asignar la fecha de validación
                descuentoItem.FechaExpiracion = DateOnly.FromDateTime(DateTime.Now.AddMonths(1)); // Asignar la fecha de expiración
            }
            return View(listaDescuentos); 
        }

        // GET: Descuento/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var descuento = await _context.Descuentos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (descuento == null)
            {
                return NotFound();
            }

            descuento.FechaValidacion ??= DateOnly.FromDateTime(DateTime.Now);
            descuento.FechaExpiracion ??= DateOnly.FromDateTime(DateTime.Now.AddMonths(1));

            return View(descuento);
        }

        // GET: Descuento/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Descuento/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Valor,Estado,FechaValidacion,FechaExpiracion,Operacion,Planilla")] Descuento descuento)
        {

            if (descuento.FechaValidacion != null && descuento.FechaExpiracion != null)
            {
                if (descuento.FechaExpiracion <= descuento.FechaValidacion)
                {
                    ModelState.AddModelError("FechaExpiracion", "La fecha de expiración debe ser mayor que la fecha de validación.");
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(descuento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(descuento);
        }

        // GET: Descuento/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var descuento = await _context.Descuentos.FindAsync(id);
            if (descuento == null)
            {
                return NotFound();
            }


            // Asignar valores temporales si las fechas están vacías
            descuento.FechaValidacion ??= DateOnly.FromDateTime(DateTime.Now); // Asignar fecha de validación si es nula
            descuento.FechaExpiracion ??= DateOnly.FromDateTime(DateTime.Now.AddMonths(1));

            return View(descuento);
        }

        // POST: Descuento/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Valor,Estado,FechaValidacion,FechaExpiracion,Operacion,Planilla")] Descuento descuento)
        {
            if (id != descuento.Id)
            {
                return NotFound();
            }

            if (descuento.FechaValidacion != null && descuento.FechaExpiracion != null)
            {
                if (descuento.FechaExpiracion <= descuento.FechaValidacion)
                {
                    ModelState.AddModelError("FechaExpiracion", "La fecha de expiración debe ser mayor que la fecha de validación.");
                }
            }

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
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var descuento = await _context.Descuentos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (descuento == null)
            {
                return NotFound();
            }

            return View(descuento);
        }

        // POST: Descuento/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var descuento = await _context.Descuentos.FindAsync(id);
            if (descuento != null)
            {
                _context.Descuentos.Remove(descuento);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DescuentoExists(int id)
        {
            return _context.Descuentos.Any(e => e.Id == id);
        }
    }
}
