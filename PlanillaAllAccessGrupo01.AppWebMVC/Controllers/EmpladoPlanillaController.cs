using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlanillaAllAccessGrupo01.AppWebMVC.Models;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Controllers
{

    public class EmpleadoPlanillaController : Controller
    {
        private readonly PlanillaDbContext _context;
        public EmpleadoPlanillaController(PlanillaDbContext context)
        {
            _context = context;
        }

        #region Vistas Básicas (CRUD)
        public async Task<IActionResult> Index()
        {
            var planillas = await _context.EmpleadoPlanillas
                .Include(e => e.Empleados)
                .Include(e => e.Planilla)
                .AsNoTracking()
                .ToListAsync();
            return View(planillas);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var empleadoPlanilla = await _context.EmpleadoPlanillas
                .Include(e => e.Empleados)
                .Include(e => e.Planilla)
                .FirstOrDefaultAsync(m => m.Id == id);

            return empleadoPlanilla == null ? NotFound() : View(empleadoPlanilla);
        }

        public IActionResult Create()
        {
            CargarListasDesplegables();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmpleadosId,PlanillaId,SueldoBase,TotalDiasTrabajados,TotalHorasExtra,TotalPagoHorasExtra,TotalPagoVacacion,SubTotal,LiquidoTotal")] EmpleadoPlanilla empleadoPlanilla)
        {
            if (ModelState.IsValid)
            {
                _context.Add(empleadoPlanilla);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Registro de planilla creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            CargarListasDesplegables();
            return View(empleadoPlanilla);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var empleadoPlanilla = await _context.EmpleadoPlanillas.FindAsync(id);
            if (empleadoPlanilla == null) return NotFound();

            CargarListasDesplegables();
            return View(empleadoPlanilla);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmpleadosId,PlanillaId,SueldoBase,TotalDiasTrabajados,TotalHorasExtra,TotalPagoHorasExtra,TotalPagoVacacion,SubTotal,LiquidoTotal")] EmpleadoPlanilla empleadoPlanilla)
        {
            if (id != empleadoPlanilla.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(empleadoPlanilla);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Registro de planilla actualizado exitosamente";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmpleadoPlanillaExists(empleadoPlanilla.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            CargarListasDesplegables();
            return View(empleadoPlanilla);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var empleadoPlanilla = await _context.EmpleadoPlanillas
                .Include(e => e.Empleados)
                .Include(e => e.Planilla)
                .FirstOrDefaultAsync(m => m.Id == id);

            return empleadoPlanilla == null ? NotFound() : View(empleadoPlanilla);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empleadoPlanilla = await _context.EmpleadoPlanillas.FindAsync(id);
            if (empleadoPlanilla != null)
            {
                _context.EmpleadoPlanillas.Remove(empleadoPlanilla);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Registro de planilla eliminado exitosamente";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool EmpleadoPlanillaExists(int id)
        {
            return _context.EmpleadoPlanillas.Any(e => e.Id == id);
        }
        #endregion

        #region Generación de Planillas
        [HttpPost]
        public async Task<IActionResult> BuscarInformacionEmpleados(DateTime fechaInicio, DateTime fechaFin)
        {

            if (!ValidarFechas(fechaInicio, fechaFin))
            {
                TempData["ErrorMessage"] = "Error: La fecha fin debe ser mayor o igual a la fecha inicio";
                CargarListasDesplegables();
                return View("Create");
            }

            if (!ValidarFechas(fechaInicio, fechaFin))
            {
                CargarListasDesplegables();
                return View("Create");
            }


            if (_context.Empleados.Any(e => e.Estado == 2))
            {
                TempData["WarningMessage"] = "Solo hay empleados inactivos en el sistema";
            }

            var empleados = await ObtenerEmpleadosConAsistencias(fechaInicio, fechaFin);

            if (!empleados.Any())
            {
                TempData["ErrorMessage"] = $"No se encontraron empleados activos con registros entre {fechaInicio:dd/MM/yyyy} y {fechaFin:dd/MM/yyyy}";
                CargarListasDesplegables();
                return View("Create");
            }

            var empleadosInfo = CalcularInformacionPlanilla(empleados, fechaInicio, fechaFin);
            ConfigurarViewBagsParaVista(fechaInicio, fechaFin, empleadosInfo);

            return View("CreateGeneral");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerarPlanillaGeneral(DateTime fechaInicio, DateTime fechaFin)
        {
            if (!ValidarFechas(fechaInicio, fechaFin, true))
                return RedirectToAction(nameof(Create));

            var empleados = await ObtenerEmpleadosConAsistencias(fechaInicio, fechaFin);
            if (!empleados.Any())
            {
                TempData["ErrorMessage"] = "No se encontraron empleados para generar la planilla";
                return RedirectToAction(nameof(Create));
            }

            var planilla = new Planilla
            {
                NombrePlanilla = $"Planilla Mens. {fechaInicio:MMMM}",
                TipoPlanillaId = 1,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Autorizacion = 0
            };

            _context.Planillas.Add(planilla);
            await _context.SaveChangesAsync();

            decimal totalGeneral = 0;
            foreach (var empleado in empleados)
            {
                var detallePlanilla = await GenerarDetallePlanilla(empleado, planilla.Id, fechaInicio, fechaFin);
                totalGeneral += detallePlanilla.LiquidoTotal;
            }

            planilla.TotalPago = totalGeneral;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Planilla generada exitosamente. Total: {totalGeneral:$0.00}";
            return RedirectToAction(nameof(Index), new { id = planilla.Id });
        }
        #endregion

        #region Métodos Privados
        private void CargarListasDesplegables()
        {
            ViewData["EmpleadosId"] = new SelectList(_context.Empleados, "Id", "Nombre");
            ViewData["PlanillaId"] = new SelectList(_context.Planillas, "Id", "NombrePlanilla");
        }

        private bool ValidarFechas(DateTime fechaInicio, DateTime fechaFin, bool usarTempData = false)
        {
            if (fechaFin >= fechaInicio) return true;

            var mensaje = "La fecha fin debe ser mayor o igual a la fecha inicio";
            if (usarTempData)
                TempData["ErrorMessage"] = mensaje;
            else
                ModelState.AddModelError("fechaFin", mensaje);

            return false;
        }

        private async Task<List<Empleado>> ObtenerEmpleadosConAsistencias(DateTime fechaInicio, DateTime fechaFin)
        {
            var fechaInicioDate = DateOnly.FromDateTime(fechaInicio);
            var fechaFinDate = DateOnly.FromDateTime(fechaFin);

            return await _context.Empleados
                .Where(e => e.Estado == 1 && e.TipoPlanillaId == 1)
                .Include(e => e.PuestoTrabajo)
                .Include(e => e.AsignacionBonos)
                    .ThenInclude(ab => ab.Bonos)
                .Include(e => e.AsignacionDescuentos)
                    .ThenInclude(ad => ad.Descuentos)
                .Include(e => e.Vacacions)
                .Include(e => e.ControlAsistencia
                    .Where(ca => ca.Fecha >= fechaInicioDate &&
                               ca.Fecha <= fechaFinDate))
                .AsNoTracking()
                .ToListAsync();
        }




        private List<object> CalcularInformacionPlanilla(List<Empleado> empleados, DateTime fechaInicio, DateTime fechaFin)
        {
            return empleados.Select(empleado =>
            {
                var asistencias = empleado.ControlAsistencia;
                var (diasTrabajados, horasExtras, minutosTardias, horasTrabajadasTotales) = CalcularResumenAsistencias(asistencias);

                // Calcular salario basado en asistencia
                decimal valorHoraNormal = empleado.PuestoTrabajo?.ValorxHora ?? empleado.SalarioBase / 30m / 8m;
                decimal salarioCalculado = horasTrabajadasTotales * valorHoraNormal;

                var (totalBonos, totalDescuentos) = CalcularBeneficios(empleado, minutosTardias, salarioCalculado);
                var (diasVacaciones, pagoVacaciones) = CalcularVacaciones(empleado);

                decimal totalPagoHorasExtra = horasExtras * valorHoraNormal * 1.5m;
                decimal horasTardias = minutosTardias / 60m;
                decimal subtotal = salarioCalculado + totalPagoHorasExtra + pagoVacaciones + totalBonos;
                decimal salarioNeto = subtotal - totalDescuentos;

                return new
                {
                    EmpleadoId = empleado.Id,
                    Nombre = empleado.Nombre,
                    PuestoTrabajo = empleado.PuestoTrabajo?.NombrePuesto,
                    SalarioBase = empleado.SalarioBase,
                    SalarioCalculado = salarioCalculado,
                    TotalBonos = totalBonos,
                    TotalDescuentos = totalDescuentos,
                    DiasVacaciones = diasVacaciones,
                    DiasTrabajados = diasTrabajados,
                    HorasExtras = horasExtras,
                    TotalPagoHorasExtra = totalPagoHorasExtra,
                    HorasTardias = horasTardias,
                    PagoVacaciones = pagoVacaciones,
                    SubTotal = subtotal,
                    SalarioNeto = salarioNeto,
                    Asistencias = asistencias.Select(a => new
                    {
                        Fecha = a.Fecha.ToString("dd/MM/yyyy"),
                        Entrada = a.Entrada.ToString(@"hh\:mm"),
                        Salida = a.Salida.ToString(@"hh\:mm"),
                        Estado = a.Asistencia
                    })
                };
            }).ToList<object>();
        }

        private (int DiasTrabajados, decimal HorasExtras, int HorasTardias, decimal HorasTrabajadas) CalcularResumenAsistencias(ICollection<ControlAsistencium> asistencias)
        {
            int dias = 0, horasTardias = 0;
            decimal horasExtras = 0, horasTrabajadasTotales = 0;
            var jornadaNormal = TimeSpan.FromHours(8);

            foreach (var asistencia in asistencias.Where(a => a.Asistencia == "Asistió"))
            {
                dias++;

                // Sumar directamente las horas tardías registradas
                if (asistencia.HoraTardia.HasValue)
                {
                    horasTardias += asistencia.HoraTardia.Value; // Se guarda tal cual en horas
                }

                // Cálculo de horas trabajadas (considerando horario de almuerzo)
                var horasTrabajadas = asistencia.Salida - asistencia.Entrada;

                // Descontar 1 hora de almuerzo si trabajó más de 5 horas
                if (horasTrabajadas > TimeSpan.FromHours(5))
                {
                    horasTrabajadas -= TimeSpan.FromHours(1);
                }

                horasTrabajadasTotales += (decimal)horasTrabajadas.TotalHours;

                // Cálculo de horas extra
                if (asistencia.HorasExtra.HasValue && asistencia.HorasExtra > 0)
                {
                    horasExtras += asistencia.HorasExtra.Value;
                }
                else if (horasTrabajadas > jornadaNormal)
                {
                    horasExtras += (decimal)(horasTrabajadas - jornadaNormal).TotalHours;
                }
            }

            return (dias, horasExtras, horasTardias, horasTrabajadasTotales);
        }
        private async Task<EmpleadoPlanilla> GenerarDetallePlanilla(Empleado empleado, int planillaId, DateTime fechaInicio, DateTime fechaFin)
        {
            var asistencias = empleado.ControlAsistencia;
            var (diasTrabajados, horasExtras, horasTardias, horasTrabajadasTotales) = CalcularResumenAsistencias(asistencias);

            // Convertir horas tardías a minutos
            var minutosTardias = (int)(horasTardias * 60);

            // Calcular valores basados en el puesto de trabajo
            decimal valorHoraNormal = empleado.PuestoTrabajo?.ValorxHora ?? empleado.SalarioBase / 30m / 8m;
            decimal valorHoraExtra = empleado.PuestoTrabajo?.ValorExtra ?? valorHoraNormal * 1.5m; // Usar ValorExtra si existe

            decimal salarioBaseCalculado = horasTrabajadasTotales * valorHoraNormal;
            decimal totalPagoHorasExtra = horasExtras * valorHoraExtra;

            var (totalBonos, totalDescuentos) = CalcularBeneficios(empleado, minutosTardias, salarioBaseCalculado);

            // Obtener el ID del descuento planilla
            var asignacionDescuentoIds = empleado.AsignacionDescuentos.Select(ad => ad.Id).ToList();
            var descuentoPlanillaId = await _context.DescuentoPlanillas
                .Where(dp => asignacionDescuentoIds.Contains(dp.AsignacionDescuentoId))
                .Select(dp => dp.Id)
                .FirstOrDefaultAsync();

            decimal subtotal = salarioBaseCalculado + totalPagoHorasExtra + totalBonos;
            decimal salarioNeto = subtotal - totalDescuentos;

            var empleadoPlanilla = new EmpleadoPlanilla
            {
                EmpleadosId = empleado.Id,
                PlanillaId = planillaId,
                DescuentoPlanillaId = descuentoPlanillaId,
                SueldoBase = salarioBaseCalculado,
                TotalDiasTrabajados = diasTrabajados,
                TotalHorasExtra = (int)horasExtras,
                TotalHorasTardias = horasTardias,
                TotalHorasTrabajadas = (int)horasTrabajadasTotales,
                ValorDeHorasExtra = valorHoraExtra, // Guardamos el valor exacto por hora extra
                TotalPagoHorasExtra = totalPagoHorasExtra,
                TotalDevengos = totalBonos,
                TotalDescuentos = totalDescuentos,
                SubTotal = subtotal,
                LiquidoTotal = salarioNeto
            };

            _context.EmpleadoPlanillas.Add(empleadoPlanilla);
            await _context.SaveChangesAsync();
            return empleadoPlanilla;
        }

        private (decimal TotalBonos, decimal TotalDescuentos) CalcularBeneficios(Empleado empleado, int minutosTardias, decimal salarioCalculado)
        {
            decimal valorMinuto = salarioCalculado / 30m / 8m / 60m;
            decimal bonos = 0, descuentos = 0;

            // Cálculo de TODOS los bonos (sin filtrar por estado)
            foreach (var asignacionBono in empleado.AsignacionBonos.Where(ab => ab.Bonos != null))
            {
                var bono = asignacionBono.Bonos;

                if (bono.Operacion == 1) // Bono con operación fija
                {
                    bonos += bono.Valor;
                }
                else // Bono porcentual
                {
                    bonos += salarioCalculado * (bono.Valor / 100m);
                }
            }

            // Cálculo de descuentos
            foreach (var asignacionDescuento in empleado.AsignacionDescuentos.Where(ad => ad.Descuentos != null))
            {
                var descuento = asignacionDescuento.Descuentos;

                if (descuento.Operacion == 1) // Descuento fijo
                {
                    descuentos += descuento.Valor;
                }
                else // Descuento porcentual
                {
                    descuentos += salarioCalculado * (descuento.Valor / 100m);
                }

                // Guardar en DescuentoPlanilla
                var descuentoPlanilla = new DescuentoPlanilla
                {
                    AsignacionDescuentoId = asignacionDescuento.Id,
                    Estado = 1 // Asumiendo que 1 es activo
                };
                _context.DescuentoPlanillas.Add(descuentoPlanilla);
            }

            // Guardar los cambios en la base de datos
            _context.SaveChanges();

            return (bonos, descuentos);
        }
        private (int DiasVacaciones, decimal PagoVacaciones) CalcularVacaciones(Empleado empleado)
        {
            var vacacion = empleado.Vacacions.FirstOrDefault();
            if (vacacion == null) return (0, 0);

            TimeSpan diferencia = vacacion.DiaFin - vacacion.DiaInicio;
            int dias = diferencia.Days;
            decimal pago = dias * (empleado.SalarioBase / 30m);

            return (dias, pago);
        }

        private decimal CalcularSalarioNeto(Empleado empleado, decimal totalBonos, decimal totalDescuentos, decimal pagoVacaciones, decimal horasExtras)
        {
            decimal valorHora = empleado.SalarioBase / 30m / 8m;
            decimal valorHorasExtras = horasExtras * valorHora * 1.5m;

            return empleado.SalarioBase +
                   valorHorasExtras +
                   pagoVacaciones +
                   totalBonos -
                   totalDescuentos;
        }

        private void ConfigurarViewBagsParaVista(DateTime fechaInicio, DateTime fechaFin, List<object> empleadosInfo)
        {
            ViewBag.EmpleadosInfo = empleadosInfo;
            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;

            // Totales generales
            ViewBag.SalarioBaseGeneral = empleadosInfo.Sum(e => (decimal)e.GetType().GetProperty("SalarioBase").GetValue(e));
            ViewBag.TotalBonosGeneral = empleadosInfo.Sum(e => (decimal)e.GetType().GetProperty("TotalBonos").GetValue(e));
            ViewBag.TotalDescuentosGeneral = empleadosInfo.Sum(e => (decimal)e.GetType().GetProperty("TotalDescuentos").GetValue(e));
            ViewBag.TotalHorasExtrasGeneral = empleadosInfo.Sum(e => (decimal)e.GetType().GetProperty("HorasExtras").GetValue(e));
            ViewBag.TotalPagoHorasExtraGeneral = empleadosInfo.Sum(e => (decimal)e.GetType().GetProperty("TotalPagoHorasExtra").GetValue(e));
            ViewBag.TotalHorasTardiasGeneral = empleadosInfo.Sum(e => (decimal)e.GetType().GetProperty("HorasTardias").GetValue(e));
            ViewBag.TotalSalarioNetoGeneral = empleadosInfo.Sum(e => (decimal)e.GetType().GetProperty("SalarioNeto").GetValue(e));
        }
        #endregion
    }
}
