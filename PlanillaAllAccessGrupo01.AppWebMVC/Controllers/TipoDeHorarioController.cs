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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TipodeHorario tipoDeHorario)
        {
            if (id != tipoDeHorario.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var tipoDeHorarioActual = await _context.TipodeHorarios
                        .Include(t => t.Horarios)
                        .FirstOrDefaultAsync(t => t.Id == id);

                    if (tipoDeHorarioActual == null)
                    {
                        return NotFound();
                    }

                    tipoDeHorarioActual.NombreHorario = tipoDeHorario.NombreHorario;


                    foreach (var horario in tipoDeHorario.Horarios)
                    {

                        if (horario.Id > 0)
                        {
                            var horarioExistente = tipoDeHorarioActual.Horarios.FirstOrDefault(h => h.Id == horario.Id);
                            if (horarioExistente != null)
                            {
                                horarioExistente.Dias = horario.Dias;
                                horarioExistente.HorasxDia = horario.HorasxDia;
                                horarioExistente.HorasEntrada = horario.HorasEntrada;
                                horarioExistente.HorasSalida = horario.HorasSalida;
                            }
                        }
                        else
                        {

                            tipoDeHorarioActual.Horarios.Add(horario);
                        }
                    }

                    var horariosAEliminar = tipoDeHorarioActual.Horarios
                        .Where(h => !tipoDeHorario.Horarios.Any(nh => nh.Id == h.Id))
                        .ToList();

                    foreach (var horario in horariosAEliminar)
                    {
                        _context.Horarios.Remove(horario);
                    }


                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TipoDeHorarioExists(tipoDeHorario.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }


            ViewBag.TipoDeHorarioId = new SelectList(_context.TipodeHorarios, "Id", "NombreHorario", tipoDeHorario.Id);
            return View(tipoDeHorario);
        }


        private bool TipoDeHorarioExists(int id)
        {
            return _context.TipodeHorarios.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tipoDeHorario = await _context.TipodeHorarios
                 .Include(h => h.Horarios)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (tipoDeHorario == null)
            {
                return NotFound();
            }

            return View(tipoDeHorario);
        }


    }
}
