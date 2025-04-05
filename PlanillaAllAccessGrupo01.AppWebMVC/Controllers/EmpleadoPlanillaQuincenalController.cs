using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlanillaAllAccessGrupo01.AppWebMVC.Models;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Controllers
{
    //Autorización para tener acceso a este apartado de Gestión de Planilla
    [Authorize(Roles = "Administrador de Nómina")]
    public class EmpleadoPlanillaQuincenalController : Controller
    {
        private readonly PlanillaDbContext _context;

        public EmpleadoPlanillaQuincenalController(PlanillaDbContext context)
        {
            _context = context;
        }

        #region Vistas Básicas (CRUD)
        // Acción que muestra el listado de registros de planillas quincenales por empleado
        public async Task<IActionResult> Index()
        {
            // Consulta a la base de datos todos los registros de EmpleadoPlanilla
            // Incluye los datos del empleado y de la planilla relacionada
            // Filtra solo las planillas que tienen TipoPlanillaId == 2 (planillas quincenales)
            var planillas = await _context.EmpleadoPlanillas
                .Include(e => e.Empleados)
                .Include(e => e.Planilla)
                .Where(p => p.Planilla.TipoPlanillaId == 2)
                .AsNoTracking() // Optimiza la lectura al no rastrear los cambios
                .ToListAsync();

            // Retorna la vista con la lista de planillas
            return View(planillas);
        }

        // Acción que muestra el detalle de un registro específico de planilla por empleado
        public async Task<IActionResult> Details(int? id)
        {
            // Validación de ID nulo
            if (id == null) return NotFound();

            // Busca el registro incluyendo relaciones con Empleado y Planilla
            var empleadoPlanilla = await _context.EmpleadoPlanillas
                .Include(e => e.Empleados)
                .Include(e => e.Planilla)
                .FirstOrDefaultAsync(m => m.Id == id);

            // Si no se encuentra el registro, retorna 404
            return empleadoPlanilla == null ? NotFound() : View(empleadoPlanilla);
        }


        // Acción que muestra el formulario para crear una nueva planilla por empleado
        public IActionResult Create()
        {
            // Carga listas desplegables necesarias para el formulario (empleados y planillas)
            CargarListasDesplegables();
            return View();
        }


        // Acción POST que recibe los datos del formulario y crea un nuevo registro de EmpleadoPlanilla
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,EmpleadosId,PlanillaId,SueldoBase,TotalDiasTrabajados,TotalHorasExtra,TotalPagoHorasExtra,TotalPagoVacacion,SubTotal,LiquidoTotal")] EmpleadoPlanilla empleadoPlanilla)
        {
            // Si el modelo es válido, guardar el nuevo registro
            if (ModelState.IsValid)
            {
                _context.Add(empleadoPlanilla);
                await _context.SaveChangesAsync();

                // Mensaje de éxito para mostrar en la vista
                TempData["SuccessMessage"] = "Registro de planilla creado exitosamente";

                // Redirige a la vista Index del controlador Planilla
                return RedirectToAction("Index", "Planilla");
            }

            // Si hubo errores, recarga listas desplegables y vuelve a mostrar el formulario
            CargarListasDesplegables();
            return View(empleadoPlanilla);
        }


        // Acción que muestra el formulario para editar un registro existente de EmpleadoPlanilla
        public async Task<IActionResult> Edit(int? id)
        {
            // Validación del parámetro
            if (id == null) return NotFound();

            // Busca el registro por ID
            var empleadoPlanilla = await _context.EmpleadoPlanillas.FindAsync(id);
            if (empleadoPlanilla == null) return NotFound();

            // Carga listas desplegables y muestra el formulario
            CargarListasDesplegables();
            return View(empleadoPlanilla);
        }


        // Acción POST que actualiza un registro existente de EmpleadoPlanilla
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,EmpleadosId,PlanillaId,SueldoBase,TotalDiasTrabajados,TotalHorasExtra,TotalPagoHorasExtra,TotalPagoVacacion,SubTotal,LiquidoTotal")] EmpleadoPlanilla empleadoPlanilla)
        {
            // Validación: el ID enviado por URL debe coincidir con el del modelo
            if (id != empleadoPlanilla.Id) return NotFound();

            // Si los datos son válidos, intenta actualizar
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
                    // Si el registro ya no existe en la base, devuelve NotFound
                    if (!EmpleadoPlanillaExists(empleadoPlanilla.Id))
                        return NotFound();
                    else
                        throw; // Otro tipo de error, se lanza la excepción
                }

                // Redirige al listado
                return RedirectToAction(nameof(Index));
            }

            // Si el modelo tiene errores, vuelve a mostrar el formulario
            CargarListasDesplegables();
            return View(empleadoPlanilla);
        }


        // Acción que muestra la vista de confirmación para eliminar un registro
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            // Obtiene el registro a eliminar con relaciones
            var empleadoPlanilla = await _context.EmpleadoPlanillas
                .Include(e => e.Empleados)
                .Include(e => e.Planilla)
                .FirstOrDefaultAsync(m => m.Id == id);

            return empleadoPlanilla == null ? NotFound() : View(empleadoPlanilla);
        }


        // Acción POST que confirma la eliminación del registro
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empleadoPlanilla = await _context.EmpleadoPlanillas.FindAsync(id);
            if (empleadoPlanilla != null)
            {
                _context.EmpleadoPlanillas.Remove(empleadoPlanilla);
                await _context.SaveChangesAsync();

                // Mensaje de éxito para la vista
                TempData["SuccessMessage"] = "Registro de planilla eliminado exitosamente";
            }

            // Redirige al listado
            return RedirectToAction(nameof(Index));
        }


        // Método privado que verifica si existe un EmpleadoPlanilla por ID
        private bool EmpleadoPlanillaExists(int id)
        {
            return _context.EmpleadoPlanillas.Any(e => e.Id == id);
        }

        #endregion

        #region Generación de Planillas Quincenales
        // Acción que se encarga de buscar empleados activos con registros de asistencia en un rango quincenal específico
        [HttpPost]
        public async Task<IActionResult> BuscarInformacionEmpleados(DateTime fechaInicio, DateTime fechaFin)
        {
            // Validar que las fechas sean exactamente un periodo de 15 días
            if (!ValidarFechasQuincenales(fechaInicio, fechaFin))
            {
                TempData["ErrorMessage"] = "Error: El período debe ser exactamente 15 días";
                CargarListasDesplegables(); // Vuelve a cargar los combos para la vista
                return View("Create"); // Retorna a la vista de creación
            }

            // Obtener los empleados que tienen asistencias registradas dentro del rango de fechas
            var empleados = await ObtenerEmpleadosConAsistencias(fechaInicio, fechaFin);

            // Si no se encontraron empleados con registros, se muestra un mensaje de error
            if (!empleados.Any())
            {
                TempData["ErrorMessage"] = $"No se encontraron empleados activos con registros entre {fechaInicio:dd/MM/yyyy} y {fechaFin:dd/MM/yyyy}";
                CargarListasDesplegables(); // Vuelve a cargar los combos para la vista
                return View("Create"); // Retorna a la vista de creación
            }

            // Calcula la información de la planilla para cada empleado en el período seleccionado
            var empleadosInfo = CalcularInformacionPlanillaQuincenal(empleados, fechaInicio, fechaFin);

            // Configura variables que se enviarán a la vista para mostrar los datos calculados
            ConfigurarViewBagsParaVista(fechaInicio, fechaFin, empleadosInfo);

            // Muestra la vista con la información de todos los empleados
            return View("CreateGeneral");
        }

        // Acción que genera y guarda la planilla quincenal en la base de datos
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerarPlanillaGeneral(DateTime fechaInicio, DateTime fechaFin)
        {
            // Validar que las fechas sean quincenales, si no lo son redirige a la vista de creación
            if (!ValidarFechasQuincenales(fechaInicio, fechaFin, true))
                return RedirectToAction(nameof(Create));

            // Obtiene los empleados con asistencia dentro del rango quincenal
            var empleados = await ObtenerEmpleadosConAsistencias(fechaInicio, fechaFin);

            // Si no hay empleados, muestra mensaje de error
            if (!empleados.Any())
            {
                TempData["ErrorMessage"] = "No se encontraron empleados para generar la planilla";
                return RedirectToAction(nameof(Create));
            }

            // Crea una nueva planilla quincenal
            var planilla = new Planilla
            {
                NombrePlanilla = $"Planilla Quinc. {fechaInicio:MMMM}", // Ejemplo: "Planilla Quinc. Abril"
                TipoPlanillaId = 2, // Identificador del tipo de planilla (2 = Quincenal)
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,
                Autorizacion = 0 // Aún no autorizada
            };

            // Agrega la nueva planilla a la base de datos
            _context.Planillas.Add(planilla);
            await _context.SaveChangesAsync(); // Guarda para que obtenga un ID

            decimal totalGeneral = 0;

            // Genera el detalle para cada empleado y acumula el total general
            foreach (var empleado in empleados)
            {
                var detallePlanilla = await GenerarDetallePlanillaQuincenal(empleado, planilla.Id, fechaInicio, fechaFin);
                totalGeneral += (decimal)detallePlanilla.LiquidoTotal;
            }

            // Guarda el total pagado en la planilla generada
            planilla.TotalPago = totalGeneral;
            await _context.SaveChangesAsync();

            // Muestra mensaje de éxito con el total generado
            TempData["SuccessMessage"] = $"Planilla quincenal generada exitosamente. Total: {totalGeneral:$0.00}";

            // Redirige a la vista principal de planillas
            return RedirectToAction("Index", "Planilla");
        }


        #endregion

        #region Métodos Privados
        // Carga los datos necesarios para los combos desplegables en la vista
        private void CargarListasDesplegables()
        {
            ViewData["EmpleadosId"] = new SelectList(_context.Empleados, "Id", "Nombre");
            ViewData["PlanillaId"] = new SelectList(_context.Planillas.Where(p => p.TipoPlanillaId == 2), "Id", "NombrePlanilla");
        }

        // Valida que el rango de fechas sea correcto y que dure exactamente 15 días (una quincena)
        private bool ValidarFechasQuincenales(DateTime fechaInicio, DateTime fechaFin, bool usarTempData = false)
        {
            if (fechaFin < fechaInicio)
            {
                var mensaje = "La fecha fin debe ser mayor o igual a la fecha inicio";
                if (usarTempData)
                    TempData["ErrorMessage"] = mensaje;
                else
                    ModelState.AddModelError("fechaFin", mensaje);
                return false;
            }

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

        // Obtiene los empleados activos que están asignados a planillas quincenales, y carga todas sus relaciones necesarias
        private async Task<List<Empleado>> ObtenerEmpleadosConAsistencias(DateTime fechaInicio, DateTime fechaFin)
        {
            var fechaInicioDate = DateOnly.FromDateTime(fechaInicio);
            var fechaFinDate = DateOnly.FromDateTime(fechaFin);

            return await _context.Empleados
                .Where(e => e.Estado == 1 && e.TipoPlanillaId == 2)
                .Include(e => e.PuestoTrabajo)
                .Include(e => e.AsignacionBonos).ThenInclude(ab => ab.Bonos)
                .Include(e => e.AsignacionDescuentos).ThenInclude(ad => ad.Descuentos)
                .Include(e => e.Vacacions)
                .Include(e => e.ControlAsistencia
                    .Where(ca => ca.Fecha >= fechaInicioDate && ca.Fecha <= fechaFinDate))
                .AsNoTracking()
                .ToListAsync();
        }

        // Calcula el resumen de la planilla quincenal para mostrar en una vista previa o listado general
        private List<object> CalcularInformacionPlanillaQuincenal(List<Empleado> empleados, DateTime fechaInicio, DateTime fechaFin)
        {
            return empleados.Select(empleado =>
            {
                var asistencias = empleado.ControlAsistencia;
                var (diasTrabajados, horasExtras, minutosTardias, horasTrabajadasTotales) = CalcularResumenAsistencias(asistencias);

                // Salario base quincenal (la mitad del mensual)
                decimal salarioQuincenal = (decimal)(empleado.SalarioBase / 2m);
                decimal valorHoraNormal = salarioQuincenal / 15m / 8m;
                decimal valorHoraExtra = empleado.PuestoTrabajo?.ValorExtra ?? valorHoraNormal * 1.5m;

                var (totalBonos, totalDescuentos) = CalcularBeneficiosQuincenales(empleado, minutosTardias, salarioQuincenal);
                var (diasVacaciones, pagoVacaciones) = CalcularVacaciones(empleado);

                // Calcula extras, subtotal y salario neto
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

        // Calcula los totales de asistencia: días, horas extras, minutos tarde, y horas trabajadas
        private (int DiasTrabajados, decimal HorasExtras, int MinutosTardias, decimal HorasTrabajadas) CalcularResumenAsistencias(ICollection<ControlAsistencium> asistencias)
        {
            int dias = 0, minutosTardias = 0;
            decimal horasExtras = 0, horasTrabajadasTotales = 0;
            var jornadaNormal = TimeSpan.FromHours(8);

            foreach (var asistencia in asistencias.Where(a => a.Asistencia == "Asistió"))
            {
                dias++;

                // Se suman los minutos de tardanza
                if (asistencia.HoraTardia.HasValue)
                {
                    minutosTardias += asistencia.HoraTardia.Value;
                }

                // Se calculan las horas trabajadas reales (descontando una hora de almuerzo si trabajó más de 5h)
                var horasTrabajadas = asistencia.Salida - asistencia.Entrada;

                if (horasTrabajadas > TimeSpan.FromHours(5))
                {
                    horasTrabajadas -= TimeSpan.FromHours(1);
                }

                horasTrabajadasTotales += (decimal)horasTrabajadas.TotalHours;

                // Se detectan horas extras si hay campo explícito o si trabajó más de la jornada normal
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

        // Genera y guarda el detalle de la planilla quincenal para un empleado en la base de datos
        private async Task<EmpleadoPlanilla> GenerarDetallePlanillaQuincenal(Empleado empleado, int planillaId, DateTime fechaInicio, DateTime fechaFin)
        {
            var asistencias = empleado.ControlAsistencia;
            var (diasTrabajados, horasExtras, minutosTardias, horasTrabajadasTotales) = CalcularResumenAsistencias(asistencias);

            // Se calcula el valor por hora según el puesto o el salario base
            decimal valorHoraNormal = (decimal)(empleado.PuestoTrabajo?.ValorxHora ?? empleado.SalarioBase / 30m / 8m);
            decimal valorHoraExtra = empleado.PuestoTrabajo?.ValorExtra ?? valorHoraNormal * 1.5m;
            decimal salarioBaseCalculado = horasTrabajadasTotales * valorHoraNormal;

            var (totalBonos, totalDescuentos) = CalcularBeneficiosQuincenales(empleado, minutosTardias, salarioBaseCalculado);
            var (diasVacaciones, pagoVacaciones) = CalcularVacaciones(empleado); // 👉 OJO: este pago no se está sumando todavía

            decimal totalPagoHorasExtra = horasExtras * valorHoraExtra;

            // Se puede incluir el pago de vacaciones aquí si se desea:
            decimal subtotal = salarioBaseCalculado + totalPagoHorasExtra + totalBonos + pagoVacaciones;
            decimal salarioNeto = subtotal - totalDescuentos;

            var empleadoPlanilla = new EmpleadoPlanilla
            {
                EmpleadosId = empleado.Id,
                PlanillaId = planillaId,
                DescuentoPlanillaId = null,
                SueldoBase = Math.Round(salarioBaseCalculado, 2),
                TotalDiasTrabajados = diasTrabajados,
                TotalHorasExtra = (int)horasExtras,
                TotalHorasTardias = minutosTardias,
                TotalHorasTrabajadas = (int)horasTrabajadasTotales,
                ValorDeHorasExtra = Math.Round(valorHoraExtra, 2),
                TotalPagoHorasExtra = Math.Round(totalPagoHorasExtra, 2),
                TotalDevengos = Math.Round(totalBonos, 2),
                TotalDescuentos = Math.Round(totalDescuentos, 2),
                SubTotal = Math.Round(subtotal, 2),
                LiquidoTotal = Math.Round(salarioNeto, 2)
            };

            // Se guarda el detalle de la planilla en la base de datos
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

            // Descuento por tardanzas (50% del valor del minuto)
            descuentos += minutosTardias * valorMinuto * 0.5m;

            return (bonos, descuentos);
        }

        private (int DiasVacaciones, decimal PagoVacaciones) CalcularVacaciones(Empleado empleado)
        {
            // Busca la primera entrada de vacaciones del empleado.
            var vacacion = empleado.Vacacions.FirstOrDefault();
            // Si no se encuentra ninguna entrada de vacaciones, retorna cero días y cero pago.
            if (vacacion == null) return (0, 0);

            // Calcula la diferencia en días entre la fecha de fin y la fecha de inicio de las vacaciones.
            TimeSpan diferencia = vacacion.DiaFin - vacacion.DiaInicio;
            int dias = diferencia.Days;
            // Calcula el pago de las vacaciones basado en los días de vacaciones y el salario base diario del empleado.
            decimal pago = (decimal)(dias * (empleado.SalarioBase / 30m));

            // Retorna el número de días de vacaciones y el pago correspondiente.
            return (dias, pago);
        }

        private void ConfigurarViewBagsParaVista(DateTime fechaInicio, DateTime fechaFin, List<object> empleadosInfo)
        {
            // Asigna la lista de información de los empleados al ViewBag para que esté disponible en la vista.
            ViewBag.EmpleadosInfo = empleadosInfo;
            // Asigna la fecha de inicio al ViewBag.
            ViewBag.FechaInicio = fechaInicio;
            // Asigna la fecha de fin al ViewBag.
            ViewBag.FechaFin = fechaFin;

            // Calcula y asigna los totales generales al ViewBag para mostrarlos en la vista.
            // Suma todos los salarios base de los empleados en la lista.
            ViewBag.SalarioBaseGeneral = empleadosInfo.Sum(e => (decimal)e.GetType().GetProperty("SalarioBase").GetValue(e));
            // Suma todos los bonos de los empleados en la lista.
            ViewBag.TotalBonosGeneral = empleadosInfo.Sum(e => (decimal)e.GetType().GetProperty("TotalBonos").GetValue(e));
            // Suma todos los descuentos de los empleados en la lista.
            ViewBag.TotalDescuentosGeneral = empleadosInfo.Sum(e => (decimal)e.GetType().GetProperty("TotalDescuentos").GetValue(e));
            // Suma todas las horas extras de los empleados en la lista.
            ViewBag.TotalHorasExtrasGeneral = empleadosInfo.Sum(e => (decimal)e.GetType().GetProperty("HorasExtras").GetValue(e));
            // Suma el pago total de las horas extras de los empleados en la lista.
            ViewBag.TotalPagoHorasExtraGeneral = empleadosInfo.Sum(e => (decimal)e.GetType().GetProperty("TotalPagoHorasExtra").GetValue(e));
            // Suma todas las horas tardías de los empleados en la lista.
            ViewBag.TotalHorasTardiasGeneral = empleadosInfo.Sum(e => (decimal)e.GetType().GetProperty("HorasTardias").GetValue(e));
            // Suma todos los salarios netos de los empleados en la lista.
            ViewBag.TotalSalarioNetoGeneral = empleadosInfo.Sum(e => (decimal)e.GetType().GetProperty("SalarioNeto").GetValue(e));
        }
        #endregion
    }

}

