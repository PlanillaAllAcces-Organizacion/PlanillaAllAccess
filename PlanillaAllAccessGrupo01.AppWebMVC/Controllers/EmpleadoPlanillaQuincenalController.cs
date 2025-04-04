using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlanillaAllAccessGrupo01.AppWebMVC.Models;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Controllers
{
    public class EmpleadoPlanillaQuincenalController : Controller
    {
        private readonly PlanillaDbContext _context;

        public EmpleadoPlanillaQuincenalController(PlanillaDbContext context)
        {
            _context = context;
        }

        #region Vistas Básicas (CRUD)
        public async Task<IActionResult> Index()
        {
            var planillas = await _context.EmpleadoPlanillas
                .Include(e => e.Empleados)
                .Include(e => e.Planilla)
                .Where(p => p.Planilla.TipoPlanillaId == 2) // Filtro para planillas quincenales
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

        #region Generación de Planillas Quincenales
        [HttpPost]
        public async Task<IActionResult> BuscarInformacionEmpleados(DateTime fechaInicio, DateTime fechaFin)
        {
            if (!ValidarFechasQuincenales(fechaInicio, fechaFin))
            {
                TempData["ErrorMessage"] = "Error: El período debe ser exactamente 15 días";
                CargarListasDesplegables();
                return View("Create");
            }

            var empleados = await ObtenerEmpleadosConAsistencias(fechaInicio, fechaFin);

            if (!empleados.Any())
            {
                TempData["ErrorMessage"] = $"No se encontraron empleados activos con registros entre {fechaInicio:dd/MM/yyyy} y {fechaFin:dd/MM/yyyy}";
                CargarListasDesplegables();
                return View("Create");
            }

            var empleadosInfo = CalcularInformacionPlanillaQuincenal(empleados, fechaInicio, fechaFin);
            ConfigurarViewBagsParaVista(fechaInicio, fechaFin, empleadosInfo);

            return View("CreateGeneral");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerarPlanillaGeneral(DateTime fechaInicio, DateTime fechaFin)
        {
            if (!ValidarFechasQuincenales(fechaInicio, fechaFin, true))
                return RedirectToAction(nameof(Create));

            var empleados = await ObtenerEmpleadosConAsistencias(fechaInicio, fechaFin);
            if (!empleados.Any())
            {
                TempData["ErrorMessage"] = "No se encontraron empleados para generar la planilla";
                return RedirectToAction(nameof(Create));
            }

            var planilla = new Planilla
            {
                NombrePlanilla = $"Planilla Quinc. {fechaInicio:MMMM}",
                TipoPlanillaId = 2,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Autorizacion = 0
            };

            _context.Planillas.Add(planilla);
            await _context.SaveChangesAsync();

            decimal totalGeneral = 0;
            foreach (var empleado in empleados)
            {
                var detallePlanilla = await GenerarDetallePlanillaQuincenal(empleado, planilla.Id, fechaInicio, fechaFin);
                totalGeneral += detallePlanilla.LiquidoTotal;
            }

            planilla.TotalPago = totalGeneral;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Planilla quincenal generada exitosamente. Total: {totalGeneral:$0.00}";
            return RedirectToAction(nameof(Index), new { id = planilla.Id });
        }

        #endregion

        #region Métodos Privados
        private void CargarListasDesplegables()
        {
            ViewData["EmpleadosId"] = new SelectList(_context.Empleados, "Id", "Nombre");
            ViewData["PlanillaId"] = new SelectList(_context.Planillas.Where(p => p.TipoPlanillaId == 2), "Id", "NombrePlanilla");
        }

        private bool ValidarFechasQuincenales(DateTime fechaInicio, DateTime fechaFin, bool usarTempData = false)
        {
            // Validación básica de rango
            if (fechaFin < fechaInicio)
            {
                var mensaje = "La fecha fin debe ser mayor o igual a la fecha inicio";
                if (usarTempData)
                    TempData["ErrorMessage"] = mensaje;
                else
                    ModelState.AddModelError("fechaFin", mensaje);
                return false;
            }

            // Validación de quincena (15 días exactos)
            var dias = (fechaFin - fechaInicio).Days + 1;
            if (dias != 15)
            {
                var mensaje = "El período quincenal debe ser exactamente de 15 días";
                if (usarTempData)
                    TempData["ErrorMessage"] = mensaje;
                else
                    ModelState.AddModelError("fechaFin", mensaje);
                return false;
            }

            return true;
        }

        private async Task<List<Empleado>> ObtenerEmpleadosConAsistencias(DateTime fechaInicio, DateTime fechaFin)
        {
            var fechaInicioDate = DateOnly.FromDateTime(fechaInicio);
            var fechaFinDate = DateOnly.FromDateTime(fechaFin);

            return await _context.Empleados
                .Where(e => e.Estado == 1 && e.TipoPlanillaId == 2) // Filtro para empleados de planilla quincenal
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

        private List<object> CalcularInformacionPlanillaQuincenal(List<Empleado> empleados, DateTime fechaInicio, DateTime fechaFin)
        {
            return empleados.Select(empleado =>
            {
                var asistencias = empleado.ControlAsistencia;
                var (diasTrabajados, horasExtras, minutosTardias, horasTrabajadasTotales) = CalcularResumenAsistencias(asistencias);

                // Calcular salario basado en quincena (mitad del salario mensual)
                decimal salarioQuincenal = empleado.SalarioBase / 2m;
                decimal valorHoraNormal = salarioQuincenal / 15m / 8m;
                decimal valorHoraExtra = empleado.PuestoTrabajo?.ValorExtra ?? valorHoraNormal * 1.5m;

                var (totalBonos, totalDescuentos) = CalcularBeneficiosQuincenales(empleado, minutosTardias, salarioQuincenal);
                var (diasVacaciones, pagoVacaciones) = CalcularVacaciones(empleado);

                decimal totalPagoHorasExtra = horasExtras * valorHoraExtra;
                decimal horasTardias = minutosTardias / 60m;
                decimal subtotal = salarioQuincenal + totalPagoHorasExtra + pagoVacaciones + totalBonos;
                decimal salarioNeto = subtotal - totalDescuentos;

                return new
                {
                    EmpleadoId = empleado.Id,
                    Nombre = empleado.Nombre,
                    PuestoTrabajo = empleado.PuestoTrabajo?.NombrePuesto,
                    SalarioBase = salarioQuincenal,
                    SalarioCalculado = salarioQuincenal,
                    TotalBonos = totalBonos,
                    TotalDescuentos = totalDescuentos,
                    DiasVacaciones = diasVacaciones,
                    DiasTrabajados = diasTrabajados,
                    HorasExtras = horasExtras,
                    TotalPagoHorasExtra = totalPagoHorasExtra,
                    HorasTardias = horasTardias,
                    PagoVacaciones = pagoVacaciones,
                    SubTotal = subtotal,
                    SalarioNeto = salarioNeto
                };
            }).ToList<object>();
        }

        private (int DiasTrabajados, decimal HorasExtras, int MinutosTardias, decimal HorasTrabajadas) CalcularResumenAsistencias(ICollection<ControlAsistencium> asistencias)
        {
            int dias = 0, minutosTardias = 0;
            decimal horasExtras = 0, horasTrabajadasTotales = 0;
            var jornadaNormal = TimeSpan.FromHours(8);

            foreach (var asistencia in asistencias.Where(a => a.Asistencia == "Asistió"))
            {
                dias++;

                // Sumar minutos de tardanza directamente (como en el controlador original)
                if (asistencia.HoraTardia.HasValue)
                {
                    minutosTardias += asistencia.HoraTardia.Value;
                }

                // Resto del cálculo permanece igual
                var horasTrabajadas = asistencia.Salida - asistencia.Entrada;

                if (horasTrabajadas > TimeSpan.FromHours(5))
                {
                    horasTrabajadas -= TimeSpan.FromHours(1);
                }

                horasTrabajadasTotales += (decimal)horasTrabajadas.TotalHours;

                if (asistencia.HorasExtra.HasValue && asistencia.HorasExtra > 0)
                {
                    horasExtras += asistencia.HorasExtra.Value;
                }
                else if (horasTrabajadas > jornadaNormal)
                {
                    horasExtras += (decimal)(horasTrabajadas - jornadaNormal).TotalHours;
                }
            }

            return (dias, horasExtras, minutosTardias, horasTrabajadasTotales);
        }

        private async Task<EmpleadoPlanilla> GenerarDetallePlanillaQuincenal(Empleado empleado, int planillaId, DateTime fechaInicio, DateTime fechaFin)
        {
            var asistencias = empleado.ControlAsistencia;
            var (diasTrabajados, horasExtras, minutosTardias, horasTrabajadasTotales) = CalcularResumenAsistencias(asistencias);

            // Calcular valores basados en el puesto de trabajo
            decimal valorHoraNormal = empleado.PuestoTrabajo?.ValorxHora ?? empleado.SalarioBase / 30m / 8m;
            decimal valorHoraExtra = empleado.PuestoTrabajo?.ValorExtra ?? valorHoraNormal * 1.5m;
            decimal salarioBaseCalculado = horasTrabajadasTotales * valorHoraNormal;

            var (totalBonos, totalDescuentos) = CalcularBeneficiosQuincenales(empleado, minutosTardias, salarioBaseCalculado);

            // Obtener el ID del descuento planilla
            var asignacionDescuentoIds = empleado.AsignacionDescuentos.Select(ad => ad.Id).ToList();
            var descuentoPlanillaId = await _context.DescuentoPlanillas
                .Where(dp => asignacionDescuentoIds.Contains(dp.AsignacionDescuentoId))
                .Select(dp => dp.Id)
                .FirstOrDefaultAsync();

            decimal totalPagoHorasExtra = horasExtras * valorHoraExtra;
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
                TotalHorasTardias = minutosTardias,
                TotalHorasTrabajadas = (int)horasTrabajadasTotales,
                ValorDeHorasExtra = valorHoraExtra,
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


        private (decimal TotalBonos, decimal TotalDescuentos) CalcularBeneficiosQuincenales(Empleado empleado, int minutosTardias, decimal salarioCalculado)
        {
            decimal valorMinuto = salarioCalculado / 15m / 8m / 60m;
            decimal bonos = 0, descuentos = 0;

            // Cálculo de TODOS los bonos (sin dividir)
            foreach (var asignacionBono in empleado.AsignacionBonos.Where(ab => ab.Bonos != null))
            {
                var bono = asignacionBono.Bonos;

                if (bono.Operacion == 1) // Bono fijo
                {
                    bonos += bono.Valor;
                }
                else // Bono porcentual
                {
                    bonos += salarioCalculado * (bono.Valor / 100m);
                }
            }

            // Cálculo de descuentos (sin dividir)
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
                    Estado = 1
                };
                _context.DescuentoPlanillas.Add(descuentoPlanilla);
            }

            // Descuento por tardanzas (50% del valor del minuto)
            descuentos += minutosTardias * valorMinuto * 0.5m;

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

