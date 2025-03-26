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
    public class VacacionController : Controller
    {
        private readonly PlanillaDbContext _context;

        public VacacionController(PlanillaDbContext context)
        {
            _context = context;
        }

        public IActionResult Create(int id)
        {
            var empleado = _context.Empleados.Find(id);
            if (empleado == null)
            {
                return NotFound();
            }

            ViewData["EmpleadosId"] = new SelectList(_context.Empleados, "Id", "Nombre", id);
            return View(new Vacacion { EmpleadosId = id });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmpleadosId,MesVacaciones,AnnoVacacion,DiaInicio,DiaFin,Estado,VacacionPagada,PagoVacaciones,FechaPago")] Vacacion vacacion)
        {

            vacacion.Id = 0;

            if (vacacion.VacacionPagada == 1)
            {
                if (vacacion.PagoVacaciones == null && vacacion.FechaPago == null)
                {
                    ModelState.AddModelError("PagoVacaciones", "El campo Pago de Vacaciones es requerido.");
                    ModelState.AddModelError("FechaPago", "El campo Fecha de Pago es requerido.");
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(vacacion);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Empleado");
            }
            ViewData["EmpleadosId"] = new SelectList(_context.Empleados, "Id", "Nombre", vacacion.EmpleadosId);
            return View(vacacion);
        }

       
    }
}
