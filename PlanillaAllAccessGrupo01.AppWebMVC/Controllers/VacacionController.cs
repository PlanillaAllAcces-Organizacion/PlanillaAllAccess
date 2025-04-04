using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlanillaAllAccessGrupo01.AppWebMVC.Models;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Controllers
{
    [Authorize(Roles = "Recursos Humanos")]    // Requiere autenticación para acceder a este controlador
    public class VacacionController : Controller
    {
        private readonly PlanillaDbContext _context; // Contexto de base de datos

        // Constructor que inyecta el contexto de la base de datos
        public VacacionController(PlanillaDbContext context)
        {
            _context = context;
        }

        // Acción para mostrar el listado de vacaciones con filtros
        public async Task<IActionResult> Index(Vacacion vacacion, int topRegis = 10)
        {
            // Consulta base que incluye la relación con Empleados
            var query = _context.Vacacions
                .Include(v => v.Empleados).AsQueryable();

            // Aplicar filtros según los parámetros recibidos
            if (!string.IsNullOrWhiteSpace(vacacion.Empleados?.Nombre))
                query = query.Where(s => s.Empleados.Nombre.Contains(vacacion.Empleados.Nombre));

            if (!string.IsNullOrWhiteSpace(vacacion.MesVacaciones))
                query = query.Where(v => v.MesVacaciones.Contains(vacacion.MesVacaciones));

            if (!string.IsNullOrWhiteSpace(vacacion.AnnoVacacion))
                query = query.Where(a => a.AnnoVacacion.Contains(vacacion.AnnoVacacion));

            if (vacacion.Estado.HasValue)
                query = query.Where(e => e.Estado == vacacion.Estado.Value);

            // Ordenar por ID descendente
            query = query.OrderByDescending(e => e.Id);

            // Limitar cantidad de registros si se especificó
            if (topRegis != 0)
                query = query.Take(topRegis);

            return View(await query.ToListAsync());
        }

        // Acción para mostrar el formulario de creación de vacaciones
        public IActionResult Create(int id)
        {
            // Buscar el empleado por ID
            var empleado = _context.Empleados.Find(id);
            if (empleado == null)
            {
                return NotFound();
            }

            // Preparar lista de empleados para el dropdown
            ViewData["EmpleadosId"] = new SelectList(_context.Empleados, "Id", "Nombre", id);
            return View(new Vacacion { EmpleadosId = id });
        }

        // Acción para procesar el formulario de creación de vacaciones
        [HttpPost]
        [ValidateAntiForgeryToken] // Protección contra CSRF
        public async Task<IActionResult> Create(
            [Bind("EmpleadosId,MesVacaciones,AnnoVacacion,Estado,VacacionPagada,PagoVacaciones,FechaPago")] Vacacion vacacion,
            int diaInicio, int diaFin)
        {
            try
            {
                // Validación del año (debe ser entre 1900 y 2100)
                if (!int.TryParse(vacacion.AnnoVacacion, out int year) || year < 1900 || year > 2100)
                {
                    ModelState.AddModelError("AnnoVacacion", "El año debe ser un valor entre 1900 y 2100");
                }

                // Validación del mes (no puede estar vacío)
                if (string.IsNullOrEmpty(vacacion.MesVacaciones))
                {
                    ModelState.AddModelError("MesVacaciones", "Debe seleccionar un mes");
                }

                // Conversión del nombre del mes a número
                int monthNumber = 0;
                try
                {
                    monthNumber = DateTime.ParseExact(vacacion.MesVacaciones, "MMMM", CultureInfo.CurrentCulture).Month;
                }
                catch
                {
                    ModelState.AddModelError("MesVacaciones", "Mes no válido");
                }

                // Validación de días solo si mes y año son válidos
                if (monthNumber > 0 && year > 0)
                {
                    int daysInMonth = DateTime.DaysInMonth(year, monthNumber);

                    // Validación del día de inicio
                    if (diaInicio < 1 || diaInicio > daysInMonth)
                    {
                        ModelState.AddModelError("", $"El día de inicio debe estar entre 1 y {daysInMonth} para {vacacion.MesVacaciones}");
                    }

                    // Validación del día de fin
                    if (diaFin < 1 || diaFin > daysInMonth)
                    {
                        ModelState.AddModelError("", $"El día de fin debe estar entre 1 y {daysInMonth} para {vacacion.MesVacaciones}");
                    }
                    else if (diaInicio > diaFin)
                    {
                        ModelState.AddModelError("", "El día de fin debe ser mayor o igual al día de inicio");
                    }

                    // Crear fechas si todo es válido
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

                // Si todas las validaciones son correctas, guardar en BD
                if (ModelState.IsValid)
                {
                    _context.Add(vacacion);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index", "Empleado");
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores inesperados
                ModelState.AddModelError("", $"Ocurrió un error inesperado: {ex.Message}");
            }

            // Si hay errores, recargar la vista con los datos ingresados
            ViewData["EmpleadosId"] = new SelectList(_context.Empleados, "Id", "Nombre", vacacion.EmpleadosId);
            return View(vacacion);
        }

        // Acción para mostrar los detalles de una vacación
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Buscar la vacación incluyendo los datos del empleado
            var vacacion = await _context.Vacacions
                .Include(v => v.Empleados)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (vacacion == null)
            {
                return NotFound();
            }

            return View(vacacion);
        }

        // Acción para mostrar el formulario de edición
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Buscar la vacación por ID
            var vacacion = await _context.Vacacions.FindAsync(id);
            if (vacacion == null)
            {
                return NotFound();
            }

            // Preparar lista de empleados para el dropdown
            ViewData["EmpleadosId"] = new SelectList(_context.Empleados, "Id", "Nombre", vacacion.EmpleadosId);
            return View(vacacion);
        }

        // Acción para procesar el formulario de edición
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
          [Bind("Id,EmpleadosId,MesVacaciones,AnnoVacacion,Estado,VacacionPagada,PagoVacaciones,FechaPago")] Vacacion vacacion,
          int diaInicio, int diaFin)
        {
            if (id != vacacion.Id)
            {
                return NotFound();
            }

            try
            {
                // Validación del año (debe ser entre 1900 y 2100)
                if (!int.TryParse(vacacion.AnnoVacacion, out int year) || year < 1900 || year > 2100)
                {
                    ModelState.AddModelError("AnnoVacacion", "El año debe ser un valor entre 1900 y 2100");
                }

                // Validación del mes (no puede estar vacío)
                if (string.IsNullOrEmpty(vacacion.MesVacaciones))
                {
                    ModelState.AddModelError("MesVacaciones", "Debe seleccionar un mes");
                }

                // Conversión del nombre del mes a número
                int monthNumber = 0;
                try
                {
                    monthNumber = DateTime.ParseExact(vacacion.MesVacaciones, "MMMM", CultureInfo.CurrentCulture).Month;
                }
                catch
                {
                    ModelState.AddModelError("MesVacaciones", "Mes no válido");
                }

                // Validación de días solo si mes y año son válidos
                if (monthNumber > 0 && year > 0)
                {
                    int daysInMonth = DateTime.DaysInMonth(year, monthNumber);

                    // Validación del día de inicio
                    if (diaInicio < 1 || diaInicio > daysInMonth)
                    {
                        ModelState.AddModelError("", $"El día de inicio debe estar entre 1 y {daysInMonth} para {vacacion.MesVacaciones}");
                    }

                    // Validación del día de fin
                    if (diaFin < 1 || diaFin > daysInMonth)
                    {
                        ModelState.AddModelError("", $"El día de fin debe estar entre 1 y {daysInMonth} para {vacacion.MesVacaciones}");
                    }
                    else if (diaInicio > diaFin)
                    {
                        ModelState.AddModelError("", "El día de fin debe ser mayor o igual al día de inicio");
                    }

                    // Crear fechas si todo es válido
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
                    try
                    {
                        // Cargar la entidad existente
                        var vacacionExistente = await _context.Vacacions.FindAsync(id);
                        if (vacacionExistente == null)
                        {
                            return NotFound();
                        }

                        // Actualizar propiedades
                        vacacionExistente.MesVacaciones = vacacion.MesVacaciones;
                        vacacionExistente.AnnoVacacion = vacacion.AnnoVacacion;
                        vacacionExistente.DiaInicio = vacacion.DiaInicio;
                        vacacionExistente.DiaFin = vacacion.DiaFin;
                        vacacionExistente.Estado = vacacion.Estado;
                        vacacionExistente.VacacionPagada = vacacion.VacacionPagada;
                        vacacionExistente.PagoVacaciones = vacacion.PagoVacaciones;
                        vacacionExistente.FechaPago = vacacion.FechaPago;

                        // Marcar como modificado y guardar
                        _context.Update(vacacionExistente);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
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
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ocurrió un error inesperado: {ex.Message}");
            }

            ViewData["EmpleadosId"] = new SelectList(_context.Empleados, "Id", "Nombre", vacacion.EmpleadosId);
            return View(vacacion);
        }
        // Acción para mostrar la confirmación de eliminación
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Buscar la vacación incluyendo los datos del empleado
            var vacacion = await _context.Vacacions
                .Include(v => v.Empleados)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (vacacion == null)
            {
                return NotFound();
            }

            return View(vacacion);
        }

        // Acción para confirmar la eliminación (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Buscar y eliminar la vacación
            var vacacion = await _context.Vacacions.FindAsync(id);
            if (vacacion != null)
            {
                _context.Vacacions.Remove(vacacion);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Método auxiliar para verificar si existe una vacación
        private bool VacacionExists(int id)
        {
            return _context.Vacacions.Any(e => e.Id == id);
        }
    }
}