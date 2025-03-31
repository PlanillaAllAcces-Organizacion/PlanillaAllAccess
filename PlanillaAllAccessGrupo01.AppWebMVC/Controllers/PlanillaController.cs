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
    public class PlanillaController : Controller
    {
        private readonly PlanillaDbContext _context;

        public PlanillaController(PlanillaDbContext context)
        {
            _context = context;
        }

        //Modificación del metódo Index para que permita buscar mediannte los diferentes filtros, los registros que se vayan guardando
        // GET: Planilla
        public async Task<IActionResult> Index(string nombrePlanilla, int tipoPlanilla, byte? autorizacion, int topRegistro = 10)
        {
            var query = _context.Planillas.Include(p => p.TipoPlanilla).AsQueryable();

            if (!string.IsNullOrEmpty(nombrePlanilla))//Busqueda por nombre.
            {
                query = query.Where(p => p.NombrePlanilla.Contains(nombrePlanilla));
            }

            if (tipoPlanilla > 0)//Busqueda por Tipo de planilla.
            {
                query = query.Where(p => p.TipoPlanillaId == tipoPlanilla);
            }

            // Filtrar por estado de autorización si se proporciona un valor válido (1 o 2)
            if (autorizacion.HasValue && (autorizacion == 1 || autorizacion == 2))//Busqueda por autorización.
            {
                query = query.Where(p => p.Autorizacion == autorizacion.Value);
            }

            query = query.OrderByDescending(e => e.Id);//Ordena los registros que se van ingresando de forma descendente.

            if (topRegistro > 0)
                query = query.Take(topRegistro);//Busqueda por cantidad de registros.

            var planillas = await query.ToListAsync();
            return View(planillas);
        }

        //La funcionalidad del metlódo es poder visualizar el detalle de la planilla que se  seleccione
        // GET: Planilla/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var planilla = await _context.Planillas
                .Include(p => p.TipoPlanilla)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (planilla == null)
            {
                return NotFound();
            }

            return View(planilla);
        }

        //En el metódo GET de Crear, se manda el ViewData de Tipo de Planilla para que lo muestre en la vista
        // GET: Planilla/Create
        public IActionResult Create()
        {
            ViewData["TipoPlanillaId"] = new SelectList(_context.TipoPlanillas, "Id", "NombreTipo");
            return View();
        }

        // POST: Planilla/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        //En el metódo de Crear POST, se valida la fecha fin y cada uno de los campos que se vayan a ingresar para que pueda guardar el registro.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NombrePlanilla,TipoPlanillaId,FechaInicio,FechaFin,Autorizacion,TotalPago")] Planilla planilla)
        {
            if (planilla.FechaFin <= planilla.FechaInicio)
            {
                ModelState.AddModelError("FechaFin", "La fecha de fin debe ser posterior a la fecha de inicio.");
            }

            if (ModelState.IsValid)
            {
                planilla.Autorizacion = 0;
                _context.Add(planilla);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Planilla creada con éxito.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["TipoPlanillaId"] = new SelectList(_context.TipoPlanillas, "Id", "NombreTipo", planilla.TipoPlanillaId);
            return View(planilla);
        }

        //En el metódo de Editar GETT se colocá el ViewData para que en la vista me mustre los ttipos de planilla. 
        //Tambien me busca el registro que se editara y actualizará mediante el id.
        // GET: Planilla/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var planilla = await _context.Planillas.FindAsync(id);
            if (planilla == null)
            {
                return NotFound();
            }
            ViewData["TipoPlanillaId"] = new SelectList(_context.TipoPlanillas, "Id", "NombreTipo", planilla.TipoPlanillaId);
            return View(planilla);
        }

        // POST: Planilla/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //Se colocó la funcionalidad para que pudiera actualizar registros si se modificaban.
        //Se le colocó también la validación de la fecha fin.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombrePlanilla,TipoPlanillaId,FechaInicio,FechaFin,Autorizacion,TotalPago")] Planilla planilla)
        {
            if (id != planilla.Id)
            {
                return NotFound();
            }

            if (planilla.FechaFin <= planilla.FechaInicio)
            {
                ModelState.AddModelError("FechaFin", "La fecha de fin debe ser posterior a la fecha de inicio.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(planilla);
                    await _context.SaveChangesAsync();
                    TempData["Mensaje"] = "Planilla actualizada con éxito.";

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlanillaExists(planilla.Id))
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
            ViewData["TipoPlanillaId"] = new SelectList(_context.TipoPlanillas, "Id", "NombreTipo", planilla.TipoPlanillaId);
            return View(planilla);
        }

        // GET: Planilla/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var planilla = await _context.Planillas
                .Include(p => p.TipoPlanilla)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (planilla == null)
            {
                return NotFound();
            }

            return View(planilla);
        }

        // POST: Planilla/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var planilla = await _context.Planillas.FindAsync(id);
            if (planilla != null)
            {
                _context.Planillas.Remove(planilla);
            }

            await _context.SaveChangesAsync();
            TempData["Mensaje"] = "Planilla eliminada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private bool PlanillaExists(int id)
        {
            return _context.Planillas.Any(e => e.Id == id);
        }
    }
}
