using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlanillaAllAccessGrupo01.AppWebMVC.Models;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Controllers
{
    public class TipoDeHorarioController : Controller
    {
        private readonly PlanillaDbContext _context;

        public TipoDeHorarioController(PlanillaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(TipodeHorario tipodeHorario)
        {
            var query = _context.TipodeHorarios.AsQueryable();
            if (!string.IsNullOrWhiteSpace(tipodeHorario.NombreHorario))
                query = query.Where(s => s.NombreHorario.Contains(tipodeHorario.NombreHorario));

            query = query.OrderByDescending(s => s.Id);

            return View(await query.ToListAsync());
        }


        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,NombreHorario")] TipodeHorario tipoDeHorario)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tipoDeHorario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tipoDeHorario);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoDeHorario = await _context.TipodeHorarios
                .Include(t => t.Horarios)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tipoDeHorario == null)
            {
                return NotFound();
            }

            ViewBag.TipoDeHorarioId = new SelectList(_context.TipodeHorarios, "Id", "NombreHorario", tipoDeHorario.Id);
            return View(tipoDeHorario);
        }

    }
}
