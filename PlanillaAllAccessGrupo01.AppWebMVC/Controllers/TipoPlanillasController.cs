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
    public class TipoPlanillasController : Controller
    {
        private readonly PlanillaDbContext _context;

        public TipoPlanillasController(PlanillaDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index(TipoPlanilla tipoPlanila)
        {
            var query = _context.TipoPlanillas.AsQueryable();
            if (!string.IsNullOrWhiteSpace(tipoPlanila.NombreTipo))
                query = query.Where(s => s.NombreTipo.Contains(tipoPlanila.NombreTipo));
                
            query = query.OrderByDescending(s => s.Id);

            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoPlanilla = await _context.TipoPlanillas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tipoPlanilla == null)
            {
                return NotFound();
            }

            return View(tipoPlanilla);
        }

        public IActionResult Create()
        {
            return View();
        }

        // POST: TipoPlanillas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NombreTipo")] TipoPlanilla tipoPlanilla)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tipoPlanilla);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tipoPlanilla);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoPlanilla = await _context.TipoPlanillas.FindAsync(id);
            if (tipoPlanilla == null)
            {
                return NotFound();
            }
            return View(tipoPlanilla);
        }

        // POST: TipoPlanillas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombreTipo")] TipoPlanilla tipoPlanilla)
        {
            if (id != tipoPlanilla.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var existingTipoPlanilla = await _context.TipoPlanillas.FindAsync(id);
                    if (existingTipoPlanilla != null)
                    {
                        existingTipoPlanilla.NombreTipo = tipoPlanilla.NombreTipo;
                        existingTipoPlanilla.FechaModificacion = DateTime.Now;
                        _context.Update(existingTipoPlanilla);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TipoPlanillaExists(tipoPlanilla.Id))
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
            return View(tipoPlanilla);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoPlanilla = await _context.TipoPlanillas
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tipoPlanilla == null)
            {
                return NotFound();
            }

            return View(tipoPlanilla);
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
