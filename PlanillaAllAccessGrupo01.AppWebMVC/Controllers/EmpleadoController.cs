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
    public class EmpleadoController : Controller
    {
        private readonly PlanillaDbContext _context;

        public EmpleadoController(PlanillaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var planillaDbContext = _context.Empleados.Include(e => e.JefeInmediato).Include(e => e.PuestoTrabajo).Include(e => e.TipoDeHorario);
            return View(await planillaDbContext.ToListAsync());
        }

    } 
}
