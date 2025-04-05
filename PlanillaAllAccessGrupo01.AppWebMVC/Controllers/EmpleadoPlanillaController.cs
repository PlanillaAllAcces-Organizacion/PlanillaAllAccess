using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlanillaAllAccessGrupo01.AppWebMVC.Models;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Controllers
{
    //Autorización para tener acceso a este apartado de Gestión de Planilla
    [Authorize(Roles = "Administrador de Nómina")]
    public class EmpleadoPlanillaController : Controller
    {
        private readonly PlanillaDbContext _context;
        public EmpleadoPlanillaController(PlanillaDbContext context)
        {
            _context = context;
        }

        #region Vistas Básicas (CRUD)

        // Muestra la lista de registros de planillas de empleados
        public async Task<IActionResult> Index()
        {
            // Obtiene todos los registros de EmpleadoPlanilla, incluyendo los datos del empleado y la planilla relacionada.
            var planillas = await _context.EmpleadoPlanillas
                .Include(e => e.Empleados)
                .Include(e => e.Planilla)
                .AsNoTracking() // No rastrea los cambios, mejora el rendimiento al solo mostrar los datos
                .ToListAsync();

            return View(planillas); // Retorna la vista con la lista de planillas
        }


        // Muestra el detalle de un registro específico
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound(); // Si no se proporciona el ID, devuelve error 404

            // Busca el registro con el ID proporcionado, incluyendo sus relaciones
            var empleadoPlanilla = await _context.EmpleadoPlanillas
                .Include(e => e.Empleados)
                .Include(e => e.Planilla)
                .FirstOrDefaultAsync(m => m.Id == id);

            return empleadoPlanilla == null ? NotFound() : View(empleadoPlanilla); // Si no se encuentra, muestra error 404
        }


        // Muestra el formulario para crear un nuevo registro de planilla
        public IActionResult Create()
        {
            CargarListasDesplegables(); // Carga las listas desplegables (empleados y planillas)
            return View(); // Retorna la vista vacía del formulario
        }


        // Recibe los datos del formulario para crear un nuevo registro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmpleadosId,PlanillaId,SueldoBase,TotalDiasTrabajados,TotalHorasExtra,TotalPagoHorasExtra,TotalPagoVacacion,SubTotal,LiquidoTotal")] EmpleadoPlanilla empleadoPlanilla)
        {
            if (ModelState.IsValid) // Verifica que los datos recibidos sean válidos
            {
                _context.Add(empleadoPlanilla); // Agrega el nuevo registro al contexto
                await _context.SaveChangesAsync(); // Guarda los cambios en la base de datos
                TempData["SuccessMessage"] = "Registro de planilla creado exitosamente"; // Mensaje de confirmación
                return RedirectToAction("Index", "Planilla"); // Redirige a la lista de planillas
            }

            CargarListasDesplegables(); // Si hubo error, vuelve a cargar las listas
            return View(empleadoPlanilla); // Devuelve la vista con los datos ingresados
        }


        // Muestra el formulario para editar un registro de planilla existente
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound(); // Si no se proporciona el ID, retorna error 404

            var empleadoPlanilla = await _context.EmpleadoPlanillas.FindAsync(id); // Busca el registro
            if (empleadoPlanilla == null) return NotFound(); // Si no se encuentra, error 404

            CargarListasDesplegables(); // Carga listas desplegables
            return View(empleadoPlanilla); // Retorna la vista con los datos actuales
        }


        // Guarda los cambios realizados en la edición del registro
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmpleadosId,PlanillaId,SueldoBase,TotalDiasTrabajados,TotalHorasExtra,TotalPagoHorasExtra,TotalPagoVacacion,SubTotal,LiquidoTotal")] EmpleadoPlanilla empleadoPlanilla)
        {
            if (id != empleadoPlanilla.Id) return NotFound(); // Si el ID no coincide, error 404

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(empleadoPlanilla); // Actualiza el registro
                    await _context.SaveChangesAsync(); // Guarda los cambios
                    TempData["SuccessMessage"] = "Registro de planilla actualizado exitosamente"; // Mensaje de éxito
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Si el registro ya no existe, error 404
                    if (!EmpleadoPlanillaExists(empleadoPlanilla.Id))
                        return NotFound();
                    else
                        throw; // Si es otro error, lo lanza
                }
                return RedirectToAction(nameof(Index)); // Redirige al listado
            }

            CargarListasDesplegables(); // Si hubo errores, recarga las listas
            return View(empleadoPlanilla); // Retorna la vista con los datos ingresados
        }


        // Muestra el formulario de confirmación para eliminar un registro
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var empleadoPlanilla = await _context.EmpleadoPlanillas
                .Include(e => e.Empleados)
                .Include(e => e.Planilla)
                .FirstOrDefaultAsync(m => m.Id == id); // Busca el registro

            return empleadoPlanilla == null ? NotFound() : View(empleadoPlanilla); // Si no se encuentra, error 404
        }


        // Elimina el registro seleccionado de la base de datos
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empleadoPlanilla = await _context.EmpleadoPlanillas.FindAsync(id); // Busca el registro
            if (empleadoPlanilla != null)
            {
                _context.EmpleadoPlanillas.Remove(empleadoPlanilla); // Lo elimina del contexto
                await _context.SaveChangesAsync(); // Guarda los cambios
                TempData["SuccessMessage"] = "Registro de planilla eliminado exitosamente"; // Mensaje de éxito
            }
            return RedirectToAction(nameof(Index)); // Redirige a la lista
        }


        // Verifica si un registro de planilla existe en la base de datos
        private bool EmpleadoPlanillaExists(int id)
        {
            return _context.EmpleadoPlanillas.Any(e => e.Id == id); // Devuelve true si existe
        }

        #endregion

        #region Generación de Planillas
        // Este método busca información de empleados con base en un rango de fechas
        [HttpPost]
        public async Task<IActionResult> BuscarInformacionEmpleados(DateTime fechaInicio, DateTime fechaFin)
        {
            // Verifica si la fecha final es mayor o igual a la fecha de inicio
            if (!ValidarFechas(fechaInicio, fechaFin))
            {
                TempData["ErrorMessage"] = "Error: La fecha fin debe ser mayor o igual a la fecha inicio";
                CargarListasDesplegables(); // Carga nuevamente las listas desplegables necesarias para la vista
                return View("Create"); // Regresa al formulario de creación
            }

            // Esta validación se repite innecesariamente, podría eliminarse
            if (!ValidarFechas(fechaInicio, fechaFin))
            {
                CargarListasDesplegables();
                return View("Create");
            }

            // Verifica si todos los empleados están inactivos (Estado == 2)
            if (_context.Empleados.Any(e => e.Estado == 2))
            {
                TempData["WarningMessage"] = "Solo hay empleados inactivos en el sistema";
            }

            // Obtiene la lista de empleados activos con registros de asistencia en el rango de fechas
            var empleados = await ObtenerEmpleadosConAsistencias(fechaInicio, fechaFin);

            // Si no se encontró ningún empleado con asistencia en ese rango de fechas
            if (!empleados.Any())
            {
                TempData["ErrorMessage"] = $"No se encontraron empleados activos con registros entre {fechaInicio:dd/MM/yyyy} y {fechaFin:dd/MM/yyyy}";
                CargarListasDesplegables();
                return View("Create");
            }

            // Calcula la información necesaria para mostrar en la vista (sueldos, horas, etc.)
            var empleadosInfo = CalcularInformacionPlanilla(empleados, fechaInicio, fechaFin);

            // Prepara los ViewBag para enviar la información a la vista
            ConfigurarViewBagsParaVista(fechaInicio, fechaFin, empleadosInfo);

            // Muestra la vista con el resumen general de la planilla
            return View("CreateGeneral");
        }


        // Este método genera la planilla general de empleados para un rango de fechas dado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerarPlanillaGeneral(DateTime fechaInicio, DateTime fechaFin)
        {
            // Verifica que las fechas sean válidas. Si no lo son, redirige al formulario de creación
            if (!ValidarFechas(fechaInicio, fechaFin, true))
                return RedirectToAction(nameof(Create));

            // Obtiene los empleados con asistencias dentro del rango de fechas
            var empleados = await ObtenerEmpleadosConAsistencias(fechaInicio, fechaFin);
            if (!empleados.Any())
            {
                TempData["ErrorMessage"] = "No se encontraron empleados para generar la planilla";
                return RedirectToAction(nameof(Create));
            }

            // Crea una nueva planilla con los datos básicos
            var planilla = new Planilla
            {
                NombrePlanilla = $"Planilla Mens. {fechaInicio:MMMM}", // Ejemplo: Planilla Mens. Abril
                TipoPlanillaId = 1, // Puede referirse a un tipo fijo como "Mensual"
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Autorizacion = 0 // 0 puede significar "no autorizada" aún
            };

            // Guarda la nueva planilla en la base de datos
            _context.Planillas.Add(planilla);
            await _context.SaveChangesAsync();

            decimal totalGeneral = 0;

            // Por cada empleado, se genera un detalle de planilla (línea con cálculos)
            foreach (var empleado in empleados)
            {
                var detallePlanilla = await GenerarDetallePlanilla(empleado, planilla.Id, fechaInicio, fechaFin);
                totalGeneral += (decimal)detallePlanilla.LiquidoTotal; // Acumula el total general a pagar
            }

            // Se guarda el total a pagar en la planilla general
            planilla.TotalPago = totalGeneral;
            await _context.SaveChangesAsync(); // Guarda los cambios

            // Mensaje de confirmación con el total generado
            TempData["SuccessMessage"] = $"Planilla generada exitosamente. Total: {totalGeneral:$0.00}";

            return RedirectToAction("Index", "Planilla"); // Redirige a la lista de planillas
        }

        #endregion

        #region Métodos Privados
        // Carga los datos para los dropdowns (empleados y planillas)
        private void CargarListasDesplegables()
        {
            ViewData["EmpleadosId"] = new SelectList(_context.Empleados, "Id", "Nombre");
            ViewData["PlanillaId"] = new SelectList(_context.Planillas, "Id", "NombrePlanilla");
        }


        // Valida que la fecha fin no sea menor que la fecha inicio
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


        // Obtiene empleados activos con asistencias dentro del rango de fechas
        private async Task<List<Empleado>> ObtenerEmpleadosConAsistencias(DateTime fechaInicio, DateTime fechaFin)
        {
            var fechaInicioDate = DateOnly.FromDateTime(fechaInicio);
            var fechaFinDate = DateOnly.FromDateTime(fechaFin);

            return await _context.Empleados
                .Where(e => e.Estado == 1 && e.TipoPlanillaId == 1)
                .Include(e => e.PuestoTrabajo)
                .Include(e => e.AsignacionBonos).ThenInclude(ab => ab.Bonos)
                .Include(e => e.AsignacionDescuentos).ThenInclude(ad => ad.Descuentos)
                .Include(e => e.Vacacions)
                .Include(e => e.ControlAsistencia
                    .Where(ca => ca.Fecha >= fechaInicioDate && ca.Fecha <= fechaFinDate))
                .AsNoTracking()
                .ToListAsync();
        }





        // Método modificado para pasar las fechas al cálculo de vacaciones
        private List<object> CalcularInformacionPlanilla(List<Empleado> empleados, DateTime fechaInicio, DateTime fechaFin)
        {
            return empleados.Select(empleado =>
            {
                var asistencias = empleado.ControlAsistencia;

                var (diasTrabajados, horasExtras, minutosTardias, horasTrabajadasTotales) =
                    CalcularResumenAsistencias(asistencias);

                decimal valorHoraNormal = (decimal)(empleado.PuestoTrabajo?.ValorxHora ?? empleado.SalarioBase / 30m / 8m);
                decimal salarioCalculado = horasTrabajadasTotales * valorHoraNormal;

                var (totalBonos, totalDescuentos) = CalcularBeneficios(empleado, minutosTardias, salarioCalculado);

                // Cambio aquí: Pasamos las fechas al cálculo de vacaciones
                var (diasVacaciones, pagoVacaciones) = CalcularVacaciones(empleado, fechaInicio, fechaFin);

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


        // Calcula el resumen de asistencias de un empleado
        private (int DiasTrabajados, decimal HorasExtras, int HorasTardias, decimal HorasTrabajadas) CalcularResumenAsistencias(ICollection<ControlAsistencium> asistencias)
        {
            int dias = 0, horasTardias = 0;
            decimal horasExtras = 0, horasTrabajadasTotales = 0;
            var jornadaNormal = TimeSpan.FromHours(8);

            foreach (var asistencia in asistencias.Where(a => a.Asistencia == "Asistió"))
            {
                dias++;

                // Sumar las horas de tardanza si existen
                if (asistencia.HoraTardia.HasValue)
                {
                    horasTardias += asistencia.HoraTardia.Value;
                }

                // Calcular horas trabajadas netas (descontando almuerzo si >5h)
                var horasTrabajadas = asistencia.Salida - asistencia.Entrada;
                if (horasTrabajadas > TimeSpan.FromHours(5))
                {
                    horasTrabajadas -= TimeSpan.FromHours(1);
                }

                horasTrabajadasTotales += (decimal)horasTrabajadas.TotalHours;

                // Calcular horas extra si se excede jornada normal
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

        // Método modificado para generar el detalle de planilla con el cálculo correcto de vacaciones
        private async Task<EmpleadoPlanilla> GenerarDetallePlanilla(Empleado empleado, int planillaId, DateTime fechaInicio, DateTime fechaFin)
        {
            var asistencias = empleado.ControlAsistencia ?? new List<ControlAsistencium>();
            var (diasTrabajados, horasExtras, minutosTardias, horasTrabajadasTotales) = CalcularResumenAsistencias(asistencias);

            decimal valorHoraNormal = (decimal)(empleado.PuestoTrabajo?.ValorxHora ?? empleado.SalarioBase / 30m / 8m);
            decimal salarioCalculado = horasTrabajadasTotales * valorHoraNormal;
            decimal valorHoraExtra = empleado.PuestoTrabajo?.ValorExtra ?? valorHoraNormal * 1.5m;
            decimal totalPagoHorasExtra = horasExtras * valorHoraExtra;

            var (totalBonos, totalDescuentos) = CalcularBeneficios(empleado, minutosTardias, salarioCalculado);

            // Cambio aquí: Pasamos las fechas al cálculo de vacaciones
            var (diasVacaciones, pagoVacaciones) = CalcularVacaciones(empleado, fechaInicio, fechaFin);

            decimal subtotal = salarioCalculado + totalPagoHorasExtra + pagoVacaciones + totalBonos;
            decimal salarioNeto = subtotal - totalDescuentos;

            var empleadoPlanilla = new EmpleadoPlanilla
            {
                EmpleadosId = empleado.Id,
                PlanillaId = planillaId,
                DescuentoPlanillaId = null,
                SueldoBase = salarioCalculado,
                TotalDiasTrabajados = diasTrabajados,
                TotalHorasExtra = (int)horasExtras,
                TotalHorasTardias = minutosTardias,
                TotalHorasTrabajadas = (int)horasTrabajadasTotales,
                ValorDeHorasExtra = valorHoraExtra,
                TotalPagoHorasExtra = totalPagoHorasExtra,
                TotalPagoVacacion = pagoVacaciones, // Aseguramos que se guarde el pago de vacaciones
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

            // Cálculo de bonos (sin cambios)
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

            // Cálculo de descuentos (sin crear DescuentoPlanilla)
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

                // NOTA: Eliminamos la creación de DescuentoPlanilla
            }

            // Aplicar descuento por tardías (si aplica)
            if (minutosTardias > 0)
            {
                decimal descuentoTardias = minutosTardias * valorMinuto;
                descuentos += descuentoTardias;
            }

            return (bonos, descuentos);
        }
        // Método modificado para calcular vacaciones considerando rango de fechas y pagadas
        private (int DiasVacaciones, decimal PagoVacaciones) CalcularVacaciones(Empleado empleado, DateTime fechaInicio, DateTime fechaFin)
        {
            // Obtener todas las vacaciones del empleado que estén dentro del rango y sean pagadas
            var vacacionesValidas = empleado.Vacacions
                .Where(v => v.VacacionPagada == 1 && // Solo vacaciones pagadas
                           v.DiaInicio >= fechaInicio &&
                           v.DiaFin <= fechaFin)
                .ToList();

            if (!vacacionesValidas.Any())
                return (0, 0);

            // Calcular días y pago total de todas las vacaciones válidas
            int diasTotales = 0;
            decimal pagoTotal = 0;

            foreach (var vacacion in vacacionesValidas)
            {
                TimeSpan diferencia = vacacion.DiaFin - vacacion.DiaInicio;
                int dias = diferencia.Days + 1; // +1 para incluir ambos días
                diasTotales += dias;

                // Si tiene un pago específico, usarlo, sino calcularlo
                if (vacacion.PagoVacaciones.HasValue)
                {
                    pagoTotal += vacacion.PagoVacaciones.Value;
                }
                else
                {
                    pagoTotal += (decimal)(dias * (empleado.SalarioBase / 30m));
                }
            }

            return (diasTotales, pagoTotal);
        }



        // Método que calcula el salario neto de un empleado sumando bonos, horas extras, vacaciones y restando descuentos
        private decimal CalcularSalarioNeto(Empleado empleado, decimal totalBonos, decimal totalDescuentos, decimal pagoVacaciones, decimal horasExtras)
        {
            // Calcular el valor por hora del empleado (asumiendo jornada de 8 horas diarias en 30 días)
            decimal valorHora = (decimal)(empleado.SalarioBase / 30m / 8m);

            // Calcular el valor de las horas extra (se pagan al 150%)
            decimal valorHorasExtras = horasExtras * valorHora * 1.5m;

            // Salario neto = salario base + horas extra + pago vacaciones + bonos - descuentos
            return (decimal)(empleado.SalarioBase +
                   valorHorasExtras +
                   pagoVacaciones +
                   totalBonos -
                   totalDescuentos);
        }


        // Método que configura los datos necesarios en ViewBag para ser usados en la vista de la planilla
        private void ConfigurarViewBagsParaVista(DateTime fechaInicio, DateTime fechaFin, List<object> empleadosInfo)
        {
            // Asignar lista de información de empleados y fechas seleccionadas
            ViewBag.EmpleadosInfo = empleadosInfo;
            ViewBag.FechaInicio = fechaInicio;
            ViewBag.FechaFin = fechaFin;

            // Cálculo de totales generales para mostrar en la vista resumen
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
