using Microsoft.AspNetCore.Mvc;
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
    }
}
