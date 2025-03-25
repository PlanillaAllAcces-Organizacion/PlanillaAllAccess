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
    public class PuestoTrabajoController : Controller
    {
        private readonly PlanillaDbContext _context;

        public PuestoTrabajoController(PlanillaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string nombrePuesto, byte estado = 2, int top = 10)
        {
            var estados = new Dictionary<byte, string>
            {
                  { 1, "Activo" },
                  { 0, "Inactivo" }
             };

            ViewBag.Estados = estados;

            var query = _context.PuestoTrabajos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(nombrePuesto))
                query = query.Where(p => p.NombrePuesto.Contains(nombrePuesto));

            if (estado != 2) // 2 indica "Todos los estados"
                query = query.Where(p => p.Estado == estado);

            query = query.Take(top);

            return View(await query.ToListAsync());
        }


    }
}
