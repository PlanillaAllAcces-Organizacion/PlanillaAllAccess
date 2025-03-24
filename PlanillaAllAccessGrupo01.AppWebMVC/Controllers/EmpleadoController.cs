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

        public IActionResult Create()
        {
            ViewData["JefeInmediatoId"] = new SelectList(_context.Empleados, "Id", "Id");
            ViewData["PuestoTrabajoId"] = new SelectList(_context.PuestoTrabajos, "Id", "Id");
            ViewData["TipoDeHorarioId"] = new SelectList(_context.TipodeHorarios, "Id", "Id");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,JefeInmediatoId,TipoDeHorarioId,Dui,Nombre,Apellido,Telefono,Correo,Estado,SalarioBase,FechaContraInicial,FechaContraFinal,Usuario,Password,PuestoTrabajoId")] Empleado empleado)
        {
            if (ModelState.IsValid)
            {
                _context.Add(empleado);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["JefeInmediatoId"] = new SelectList(_context.Empleados, "Id", "Id", empleado.JefeInmediatoId);
            ViewData["PuestoTrabajoId"] = new SelectList(_context.PuestoTrabajos, "Id", "Id", empleado.PuestoTrabajoId);
            ViewData["TipoDeHorarioId"] = new SelectList(_context.TipodeHorarios, "Id", "Id", empleado.TipoDeHorarioId);
            return View(empleado);
        }

    } 
}
