using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlanillaAllAccessGrupo01.AppWebMVC.Models;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Controllers
{
    /// <summary>
    /// Controlador para gestionar las planillas de empleados
    /// </summary>
    public class EmpleadoPlanillaController : Controller
    {
        private readonly PlanillaDbContext _context; // Contexto de base de datos para operaciones CRUD

        /// <summary>
        /// Constructor que inicializa el contexto de base de datos
        /// </summary>
        public EmpleadoPlanillaController(PlanillaDbContext context)
        {
            _context = context;
        }

        #region Vistas Básicas (CRUD)

        /// <summary>
        /// Muestra la lista de todas las planillas de empleados
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // Obtiene todas las planillas con información relacionada de empleados y planillas
            var planillas = await _context.EmpleadoPlanillas
                .Include(e => e.Empleados)          // Incluye datos de empleados
                .Include(e => e.Planilla)           // Incluye datos de planillas
                .AsNoTracking()                     // Mejora rendimiento para operaciones de solo lectura
                .ToListAsync();                     // Ejecuta la consulta asíncronamente

            return View(planillas);
        }

        /// <summary>
        /// Muestra los detalles de una planilla específica
        /// </summary>
        /// <param name="id">ID de la planilla a mostrar</param>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound(); // Validación básica de parámetro

            // Busca la planilla con información relacionada
            var empleadoPlanilla = await _context.EmpleadoPlanillas
                .Include(e => e.Empleados)
                .Include(e => e.Planilla)
                .FirstOrDefaultAsync(m => m.Id == id);

            return empleadoPlanilla == null ? NotFound() : View(empleadoPlanilla);
        }

        /// <summary>
        /// Muestra el formulario para crear una nueva planilla
        /// </summary>
        public IActionResult Create()
        {
            CargarListasDesplegables(); // Prepara las listas para dropdowns
            return View();
        }

        /// <summary>
        /// Procesa el formulario de creación de planilla
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken] // Protección contra CSRF
        public async Task<IActionResult> Create([Bind("Id,EmpleadosId,PlanillaId,SueldoBase,TotalDiasTrabajados,TotalHorasExtra,TotalPagoHorasExtra,TotalPagoVacacion,SubTotal,LiquidoTotal")] EmpleadoPlanilla empleadoPlanilla)
        {
            if (ModelState.IsValid)
            {
                _context.Add(empleadoPlanilla);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Registro de planilla creado exitosamente";
                return RedirectToAction("Index", "Planilla");
            }
            CargarListasDesplegables();
            return View(empleadoPlanilla);
        }

        /// <summary>
        /// Muestra el formulario para editar una planilla existente
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var empleadoPlanilla = await _context.EmpleadoPlanillas.FindAsync(id);
            if (empleadoPlanilla == null) return NotFound();

            CargarListasDesplegables();
            return View(empleadoPlanilla);
        }

        /// <summary>
        /// Procesa el formulario de edición de planilla
        /// </summary>
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

        /// <summary>
        /// Muestra el formulario de confirmación para eliminar una planilla
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var empleadoPlanilla = await _context.EmpleadoPlanillas
                .Include(e => e.Empleados)
                .Include(e => e.Planilla)
                .FirstOrDefaultAsync(m => m.Id == id);

            return empleadoPlanilla == null ? NotFound() : View(empleadoPlanilla);
        }

        /// <summary>
        /// Procesa la eliminación de una planilla
        /// </summary>
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

        /// <summary>
        /// Verifica si existe una planilla con el ID especificado
        /// </summary>
        private bool EmpleadoPlanillaExists(int id)
        {
            return _context.EmpleadoPlanillas.Any(e => e.Id == id);
        }
        #endregion

        #region Generación de Planillas

        /// <summary>
        /// Busca información de empleados para generar planillas
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BuscarInformacionEmpleados(DateTime fechaInicio, DateTime fechaFin)
        {
            // Validación de fechas
            if (!ValidarFechas(fechaInicio, fechaFin))
            {
                TempData["ErrorMessage"] = "Error: La fecha fin debe ser mayor o igual a la fecha inicio";
                CargarListasDesplegables();
                return View("Create");
            }

            // Advertencia si solo hay empleados inactivos
            if (_context.Empleados.Any(e => e.Estado == 2)) // Magic number - sería mejor usar una constante
            {
                TempData["WarningMessage"] = "Solo hay empleados inactivos en el sistema";
            }

            // Obtiene empleados con sus asistencias en el rango de fechas
            var empleados = await ObtenerEmpleadosConAsistencias(fechaInicio, fechaFin);

            if (!empleados.Any())
            {
                TempData["ErrorMessage"] = $"No se encontraron empleados activos con registros entre {fechaInicio:dd/MM/yyyy} y {fechaFin:dd/MM/yyyy}";
                CargarListasDesplegables();
                return View("Create");
            }

            // Calcula información para la planilla
            var empleadosInfo = CalcularInformacionPlanilla(empleados, fechaInicio, fechaFin);
            ConfigurarViewBagsParaVista(fechaInicio, fechaFin, empleadosInfo);

            return View("CreateGeneral");
        }

        /// <summary>
        /// Genera una planilla general para todos los empleados
        /// </summary>
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

            // Crea el registro de planilla
            var planilla = new Planilla
            {
                NombrePlanilla = $"Planilla Mens. {fechaInicio:MMMM}",
                TipoPlanillaId = 1, // Magic number - sería mejor usar una constante
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Autorizacion = 0 // 0 = No autorizado
            };

            _context.Planillas.Add(planilla);
            await _context.SaveChangesAsync();

            // Genera detalles para cada empleado
            decimal totalGeneral = 0;
            foreach (var empleado in empleados)
            {
                var detallePlanilla = await GenerarDetallePlanilla(empleado, planilla.Id, fechaInicio, fechaFin);
                totalGeneral += (decimal)detallePlanilla.LiquidoTotal;
            }

            // Actualiza el total de la planilla
            planilla.TotalPago = totalGeneral;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Planilla generada exitosamente. Total: {totalGeneral:$0.00}";
            return RedirectToAction("Index", "Planilla");
        }
        #endregion

        #region Métodos Privados

        /// <summary>
        /// Carga las listas para dropdowns en las vistas
        /// </summary>
        private void CargarListasDesplegables()
        {
            ViewData["EmpleadosId"] = new SelectList(_context.Empleados, "Id", "Nombre");
            ViewData["PlanillaId"] = new SelectList(_context.Planillas, "Id", "NombrePlanilla");
        }

        /// <summary>
        /// Valida que el rango de fechas sea cronológicamente correcto.
        /// </summary>
        /// <param name="fechaInicio">Fecha inicial del período</param>
        /// <param name="fechaFin">Fecha final del período</param>
        /// <param name="usarTempData">
        /// True para almacenar mensajes en TempData (redirecciones),
        /// False para usar ModelState (formularios)
        /// </param>
        /// <returns>
        /// True si fechaFin es mayor o igual a fechaInicio, False en caso contrario
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Se lanza cuando fechaInicio o fechaFin son DateTime.MinValue o DateTime.MaxValue
        /// </exception>
        private bool ValidarFechas(DateTime fechaInicio, DateTime fechaFin, bool usarTempData = false)
        {
            // Validación de valores extremos
            if (fechaInicio == DateTime.MinValue || fechaInicio == DateTime.MaxValue ||
                fechaFin == DateTime.MinValue || fechaFin == DateTime.MaxValue)
            {
                throw new ArgumentException("Las fechas no pueden tener valores extremos");
            }

            if (fechaFin >= fechaInicio) return true;

            const string mensajeError = "La fecha final debe ser mayor o igual a la fecha inicial";

            if (usarTempData)
            {
                TempData["ErrorMessage"] = mensajeError;
            }
            else
            {
                ModelState.AddModelError(nameof(fechaFin), mensajeError);
            }

            return false;
        }

        /// <summary>
        /// Obtiene empleados activos con sus datos relacionados para el cálculo de planillas.
        /// </summary>
        /// <param name="fechaInicio">Fecha inicial del período</param>
        /// <param name="fechaFin">Fecha final del período</param>
        /// <returns>
        /// Lista de empleados con sus puestos, bonos, descuentos, vacaciones y asistencias
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Se lanza cuando las fechas no son válidas o el rango es incorrecto
        /// </exception>
        private async Task<List<Empleado>> ObtenerEmpleadosConAsistencias(DateTime fechaInicio, DateTime fechaFin)
        {
            // Validación previa del rango de fechas
            if (!ValidarFechas(fechaInicio, fechaFin))
            {
                throw new ArgumentException("El rango de fechas no es válido");
            }

            // Configuración de constantes para mejor legibilidad
            const int empleadoActivo = 1;
            const int tipoPlanillaMensual = 1;

            try
            {
                var fechaInicioDate = DateOnly.FromDateTime(fechaInicio);
                var fechaFinDate = DateOnly.FromDateTime(fechaFin);

                return await _context.Empleados
                    .Where(e => e.Estado == empleadoActivo &&
                               e.TipoPlanillaId == tipoPlanillaMensual)
                    .Select(e => new Empleado
                    {
                        // Seleccionamos solo las propiedades necesarias
                        Id = e.Id,
                        Nombre = e.Nombre,
                        SalarioBase = e.SalarioBase,
                        Estado = e.Estado,
                        TipoPlanillaId = e.TipoPlanillaId,

                        // Cargamos relaciones necesarias
                        PuestoTrabajo = e.PuestoTrabajo,

                        AsignacionBonos = e.AsignacionBonos
                            .Where(ab => ab.Bonos != null)
                            .ToList(),

                        AsignacionDescuentos = e.AsignacionDescuentos
                            .Where(ad => ad.Descuentos != null)
                            .ToList(),

                        Vacacions = e.Vacacions.ToList(),

                        ControlAsistencia = e.ControlAsistencia
                            .Where(ca => ca.Fecha >= fechaInicioDate &&
                                        ca.Fecha <= fechaFinDate)
                            .ToList()
                    })
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Loggear el error (implementación depende de tu sistema de logging)
                // _logger.LogError(ex, "Error al obtener empleados con asistencias");
                throw new ApplicationException("Error al recuperar datos de empleados", ex);
            }
        }

        /// <summary>
        /// Calcula la información de planilla para cada empleado
        /// </summary>
        private List<object> CalcularInformacionPlanilla(List<Empleado> empleados, DateTime fechaInicio, DateTime fechaFin)
        {
            return empleados.Select(empleado =>
            {
                var asistencias = empleado.ControlAsistencia;
                var (diasTrabajados, horasExtras, minutosTardias, horasTrabajadasTotales) = CalcularResumenAsistencias(asistencias);

                // Cálculo de valores monetarios
                decimal valorHoraNormal = (decimal)(empleado.PuestoTrabajo?.ValorxHora ?? empleado.SalarioBase / 30m / 8m);
                decimal salarioCalculado = horasTrabajadasTotales * valorHoraNormal;

                var (totalBonos, totalDescuentos) = CalcularBeneficios(empleado, minutosTardias, salarioCalculado);
                var (diasVacaciones, pagoVacaciones) = CalcularVacaciones(empleado);

                decimal totalPagoHorasExtra = horasExtras * valorHoraNormal * 1.5m;
                decimal horasTardias = minutosTardias / 60m;
                decimal subtotal = salarioCalculado + totalPagoHorasExtra + pagoVacaciones + totalBonos;
                decimal salarioNeto = subtotal - totalDescuentos;

                // Prepara objeto con toda la información
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

        /// <summary>
        /// Calcula el resumen de días trabajados, horas extras y tardías de un empleado
        /// </summary>
        /// <returns>
        /// Tupla con:
        /// - DiasTrabajados: Total de días con asistencia registrada
        /// - HorasExtras: Horas extras acumuladas (decimal para precisión)
        /// - HorasTardias: Minutos de tardía acumulados (en minutos)
        /// - HorasTrabajadas: Horas netas trabajadas (descontando almuerzo)
        /// </returns>
        private (int DiasTrabajados, decimal HorasExtras, int HorasTardias, decimal HorasTrabajadas)
            CalcularResumenAsistencias(ICollection<ControlAsistencium> asistencias)
        {
            // Constantes para configuración
            const int minutosParaAlmuerzo = 60;
            const int horasMinimasParaAlmuerzo = 5;
            var jornadaNormal = TimeSpan.FromHours(8);

            // Variables de acumulación
            int dias = 0, minutosTardias = 0;
            decimal horasExtras = 0m, horasTrabajadasTotales = 0m;

            // Filtramos solo asistencias válidas
            var asistenciasValidas = asistencias?
                .Where(a => a?.Asistencia == "Asistió" && a.Entrada != default && a.Salida != default)
                .ToList() ?? new List<ControlAsistencium>();

            foreach (var asistencia in asistenciasValidas)
            {
                dias++;

                // Acumular minutos de tardía
                minutosTardias += asistencia.HoraTardia ?? 0;

                // Calcular horas trabajadas (entrada a salida)
                var horasTrabajadas = asistencia.Salida - asistencia.Entrada;

                // Descontar almuerzo si corresponde
                if (horasTrabajadas.TotalHours > horasMinimasParaAlmuerzo)
                {
                    horasTrabajadas = horasTrabajadas.Subtract(TimeSpan.FromMinutes(minutosParaAlmuerzo));
                }

                // Acumular horas trabajadas
                horasTrabajadasTotales += (decimal)horasTrabajadas.TotalHours;

                // Calcular horas extras (explícitas o por exceso de jornada)
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

        /// <summary>
        /// Genera el detalle de planilla para un empleado
        /// </summary>
        private async Task<EmpleadoPlanilla> GenerarDetallePlanilla(Empleado empleado, int planillaId, DateTime fechaInicio, DateTime fechaFin)
        {
            var asistencias = empleado.ControlAsistencia;
            var (diasTrabajados, horasExtras, horasTardias, horasTrabajadasTotales) = CalcularResumenAsistencias(asistencias);

            // Convertir horas tardías a minutos
            var minutosTardias = (int)(horasTardias * 60);

            // Cálculo de valores monetarios
            decimal valorHoraNormal = (decimal)(empleado.PuestoTrabajo?.ValorxHora ?? empleado.SalarioBase / 30m / 8m);
            decimal valorHoraExtra = empleado.PuestoTrabajo?.ValorExtra ?? valorHoraNormal * 1.5m;

            decimal salarioBaseCalculado = horasTrabajadasTotales * valorHoraNormal;
            decimal totalPagoHorasExtra = horasExtras * valorHoraExtra;

            var (totalBonos, totalDescuentos) = CalcularBeneficios(empleado, minutosTardias, salarioBaseCalculado);

            // Obtener ID de descuento planilla
            var asignacionDescuentoIds = empleado.AsignacionDescuentos.Select(ad => ad.Id).ToList();
            var descuentoPlanillaId = await _context.DescuentoPlanillas
                .Where(dp => asignacionDescuentoIds.Contains(dp.AsignacionDescuentoId))
                .Select(dp => dp.Id)
                .FirstOrDefaultAsync();

            decimal subtotal = salarioBaseCalculado + totalPagoHorasExtra + totalBonos;
            decimal salarioNeto = subtotal - totalDescuentos;

            // Crea el registro de detalle
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

        /// <summary>
        /// Calcula bonos y descuentos para un empleado
        /// </summary>
        private (decimal TotalBonos, decimal TotalDescuentos) CalcularBeneficios(Empleado empleado, int minutosTardias, decimal salarioCalculado)
        {
            decimal valorMinuto = salarioCalculado / 30m / 8m / 60m;
            decimal bonos = 0, descuentos = 0;

            // Cálculo de bonos
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

                // Registra el descuento en planilla
                var descuentoPlanilla = new DescuentoPlanilla
                {
                    AsignacionDescuentoId = asignacionDescuento.Id,
                    Estado = 1 // Activo
                };
                _context.DescuentoPlanillas.Add(descuentoPlanilla);
            }

            _context.SaveChanges();

            return (bonos, descuentos);
        }

        /// <summary>
        /// Calcula días y pago de vacaciones
        /// </summary>
        private (int DiasVacaciones, decimal PagoVacaciones) CalcularVacaciones(Empleado empleado)
        {
            var vacacion = empleado.Vacacions.FirstOrDefault();
            if (vacacion == null) return (0, 0);

            TimeSpan diferencia = vacacion.DiaFin - vacacion.DiaInicio;
            int dias = diferencia.Days;
            decimal pago = (decimal)(dias * (empleado.SalarioBase / 30m));

            return (dias, pago);
        }

        /// <summary>
        /// Configura ViewBag con información para la vista
        /// </summary>
        private void ConfigurarViewBagsParaVista(DateTime fechaInicio, DateTime fechaFin, List<object> empleadosInfo)
        {
            ViewBag.EmpleadosInfo = empleadosInfo;
            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;

            // Totales generales usando reflexión (podría mejorarse con tipos fuertes)
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