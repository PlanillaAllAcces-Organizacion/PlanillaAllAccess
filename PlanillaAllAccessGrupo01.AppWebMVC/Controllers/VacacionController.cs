using System;
using System.Collections.Generic;
using System.Globalization;
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

        public async Task<IActionResult> Index(Vacacion vacacion, int topRegis = 10)
        {
            var query = _context.Vacacions
                .Include(v => v.Empleados).AsQueryable();

            if (!string.IsNullOrWhiteSpace(vacacion.Empleados?.Nombre))
                query = query.Where(s => s.Empleados.Nombre.Contains(vacacion.Empleados.Nombre));

            if (!string.IsNullOrWhiteSpace(vacacion.MesVacaciones))
                query = query.Where(v => v.MesVacaciones.Contains(vacacion.MesVacaciones));

            if (!string.IsNullOrWhiteSpace(vacacion.AnnoVacacion))
                query = query.Where(a => a.AnnoVacacion.Contains(vacacion.AnnoVacacion));

            if (!string.IsNullOrWhiteSpace(vacacion.MesVacaciones))
                query = query.Where(m => m.MesVacaciones.Contains(vacacion.MesVacaciones));

            if (vacacion.Estado.HasValue)
                query = query.Where(e => e.Estado == vacacion.Estado.Value);

            query = query.OrderByDescending(e => e.Id);

            if (topRegis != 0)
                query = query.Take(topRegis);


            return View(await query.ToListAsync());
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
        public async Task<IActionResult> Create([Bind("EmpleadosId,MesVacaciones,AnnoVacacion,Estado,VacacionPagada,PagoVacaciones,FechaPago")] Vacacion vacacion,
        int diaInicio, int diaFin)
        {
            try
            {
                // Validar que el año sea un número válido
                if (!int.TryParse(vacacion.AnnoVacacion, out int year) || year < 1900 || year > 2100)
                {
                    ModelState.AddModelError("AnnoVacacion", "El año debe ser un valor entre 1900 y 2100");
                }

                // Validar que el mes sea válido
                if (string.IsNullOrEmpty(vacacion.MesVacaciones))
                {
                    ModelState.AddModelError("MesVacaciones", "Debe seleccionar un mes");
                }

                // Convercion de string a int
                int monthNumber = 0;
                try
                {
                    monthNumber = DateTime.ParseExact(vacacion.MesVacaciones, "MMMM", CultureInfo.CurrentCulture).Month;
                }
                catch
                {
                    ModelState.AddModelError("MesVacaciones", "Mes no válido");
                }

                // valida días solo si mes y año son válidos
                if (monthNumber > 0 && year > 0)
                {
                    int daysInMonth = DateTime.DaysInMonth(year, monthNumber);

                    // valida que el dia de iuinisio este bien
                    if (diaInicio < 1 || diaInicio > daysInMonth)
                    {
                        ModelState.AddModelError("", $"El día de inicio debe estar entre 1 y {daysInMonth} para {vacacion.MesVacaciones}");
                    }

                    // Validar día de fin
                    if (diaFin < 1 || diaFin > daysInMonth)
                    {
                        ModelState.AddModelError("", $"El día de fin debe estar entre 1 y {daysInMonth} para {vacacion.MesVacaciones}");
                    }
                    else if (diaInicio > diaFin)
                    {
                        ModelState.AddModelError("", "El día de fin debe ser mayor o igual al día de inicio");
                    }

                    // Solo intentar crear las fechas si todo es válido
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            vacacion.DiaInicio = new DateTime(year, monthNumber, diaInicio);
                            vacacion.DiaFin = new DateTime(year, monthNumber, diaFin);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            ModelState.AddModelError("", "La combinación de día, mes y año no es válida");
                        }
                    }
                }

                // Validación adicional para vacaciones pagadas
                if (vacacion.VacacionPagada == 1)
        {
            if (vacacion.PagoVacaciones == null || vacacion.FechaPago == null)
            {
                if (vacacion.PagoVacaciones == null)
                    ModelState.AddModelError("PagoVacaciones", "El campo Pago de Vacaciones es requerido.");
                if (vacacion.FechaPago == null)
                    ModelState.AddModelError("FechaPago", "El campo Fecha de Pago es requerido.");
            }
        }

        if (ModelState.IsValid)
        {
            _context.Add(vacacion);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Empleado");
        }
    }
    catch (Exception ex)
    {
        ModelState.AddModelError("", $"Ocurrió un error inesperado: {ex.Message}");
    }

    ViewData["EmpleadosId"] = new SelectList(_context.Empleados, "Id", "Nombre", vacacion.EmpleadosId);
    return View(vacacion);
}

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vacacion = await _context.Vacacions
                .Include(v => v.Empleados)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vacacion == null)
            {
                return NotFound();
            }

            return View(vacacion);
        }


        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vacacion = await _context.Vacacions.FindAsync(id);
            if (vacacion == null)
            {
                return NotFound();
            }
            ViewData["EmpleadosId"] = new SelectList(_context.Empleados, "Id", "Nombre", vacacion.EmpleadosId);
            return View(vacacion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmpleadosId,MesVacaciones,AnnoVacacion,DiaInicio,DiaFin,Estado,VacacionPagada,PagoVacaciones,FechaPago")] Vacacion vacacion)
        {
            if (id != vacacion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vacacion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VacacionExists(vacacion.Id))
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
            ViewData["EmpleadosId"] = new SelectList(_context.Empleados, "Id", "Nombre", vacacion.EmpleadosId);
            return View(vacacion);
        }



        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vacacion = await _context.Vacacions
                .Include(v => v.Empleados)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (vacacion == null)
            {
                return NotFound();
            }

            return View(vacacion);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vacacion = await _context.Vacacions.FindAsync(id);
            if (vacacion != null)
            {
                _context.Vacacions.Remove(vacacion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        private bool VacacionExists(int id)
        {
            return _context.Vacacions.Any(e => e.Id == id);
        }


    }
}
