using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlanillaAllAccessGrupo01.AppWebMVC.Models;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Controllers
{
    [Authorize]
    public class EmpleadoController : Controller
    {
        private readonly PlanillaDbContext _context;

        public EmpleadoController(PlanillaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(Empleado empleado, int topRegistro = 10, int? mesInicioContrato = null, int? mesFinContrato = null)
        {
            // 1. Filtrado inicial de empleados
            var query = _context.Empleados.AsQueryable();

            // Filtros básicos (texto)
            if (!string.IsNullOrWhiteSpace(empleado.Dui))
                query = query.Where(s => s.Dui.Contains(empleado.Dui));
            if (!string.IsNullOrWhiteSpace(empleado.Nombre))
                query = query.Where(s => s.Nombre.Contains(empleado.Nombre));
            if (!string.IsNullOrWhiteSpace(empleado.Apellido))
                query = query.Where(s => s.Apellido.Contains(empleado.Apellido));

            // Filtros por estado y relaciones
            if (empleado.Estado >= 0)
                query = query.Where(s => s.Estado.ToString().Contains(empleado.Estado.ToString()));

            if (empleado.PuestoTrabajoId > 0)
                query = query.Where(s => s.PuestoTrabajoId == empleado.PuestoTrabajoId);
            if (empleado.TipoDeHorarioId > 0)
                query = query.Where(s => s.TipoDeHorarioId == empleado.TipoDeHorarioId);
            if (empleado.JefeInmediatoId > 0)
                query = query.Where(s => s.JefeInmediatoId == empleado.JefeInmediatoId);

            if (mesInicioContrato.HasValue && mesInicioContrato > 0)
                query = query.Where(s => s.FechaContraInicial.Month == mesInicioContrato.Value);

            if (mesFinContrato.HasValue && mesFinContrato > 0)
                query = query.Where(s => s.FechaContraFinal.Month == mesFinContrato.Value);

            // Limitar resultados y cargar relaciones
            if (topRegistro > 0)
                query = query.Take(topRegistro);
            query = query.
                Include(e => e.Vacacions) // Asegúrate de incluir las vacaciones
                .Include(p => p.TipoDeHorario).Include(p => p.PuestoTrabajo).Include(p => p.JefeInmediato);

            // 2. Preparar datos para dropdowns
            // Lista de puestos con opción default
            var puestotrabajos = _context.PuestoTrabajos.ToList();
            puestotrabajos.Add(new PuestoTrabajo { NombrePuesto = "SELECCIONAR", Id = 0 });

            // Lista de horarios con opción default
            var tipodehorario = _context.TipodeHorarios.ToList();
            tipodehorario.Add(new TipodeHorario { NombreHorario = "SELECCIONAR", Id = 0 });

            // Lista de jefes (solo supervisores) con opción default
            var jefesInmediatos = await _context.Empleados.Where(e => e.PuestoTrabajo.NombrePuesto.Contains("Supervisor")).Select(e => new { e.Id, e.Nombre }).ToListAsync();
            jefesInmediatos.Insert(0, new { Id = 0, Nombre = "SELECCIONAR" });

            // Lista de meses para filtros
            var meses = Enumerable.Range(1, 12)
                .Select(m => new { Id = m, Nombre = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m) })
                .ToList();
            meses.Insert(0, new { Id = 0, Nombre = "SELECCIONAR" });

            // 3. Ordenar y configurar ViewData
            query = query.OrderByDescending(e => e.Id);

            ViewData["TipoDeHorarioId"] = new SelectList(tipodehorario, "Id", "NombreHorario", 0);
            ViewData["PuestoTrabajoId"] = new SelectList(puestotrabajos, "Id", "NombrePuesto", 0);
            ViewData["JefeInmediatoId"] = new SelectList(items: jefesInmediatos, dataValueField: "Id", dataTextField: "Nombre", selectedValue: 0);
            ViewData["Meses"] = new SelectList(meses, "Id", "Nombre");
            ViewData["MesInicioContratoSeleccionado"] = mesInicioContrato;
            ViewData["MesFinContratoSeleccionado"] = mesFinContrato;

            // 4. Retornar vista con resultados
            return View(await query.ToListAsync());
        }

        public IActionResult Create()
        {
            // Crea una lista de elementos SelectListItem para representar los estados (Activo/Inactivo).
            var estados = new List<SelectListItem>
            {
                new  SelectListItem{ Value="1",Text="Activo" },
                new  SelectListItem{ Value="0",Text="Inactivo" }
            };

            // Asigna la lista de estados al ViewBag para que esté disponible en la vista.
            ViewBag.Estados = estados;

            // Asigna una lista de empleados (jefes inmediatos) al ViewData para que esté disponible en la vista.
            // Filtra los empleados para obtener solo aquellos con el puesto de "Supervisor".
            ViewData["JefeInmediatoId"] = new SelectList(_context.Empleados.Where(e => e.PuestoTrabajo.NombrePuesto == "Supervisor").ToList(), "Id", "Nombre");

            // Asigna una lista de puestos de trabajo al ViewData para que esté disponible en la vista.
            // Filtra los puestos de trabajo para obtener solo aquellos que están activos (Estado == 1)
            ViewData["PuestoTrabajoId"] = new SelectList(_context.PuestoTrabajos.Where(e => e.Estado == 1).ToList(), "Id", "NombrePuesto");

            // Asigna una lista de tipos de horarios al ViewData para que esté disponible en la vista.
            ViewData["NombreHorario"] = new SelectList(_context.TipodeHorarios, "Id", "NombreHorario");

            // Retorna la vista "Create" para mostrar el formulario de creación de empleados.
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,JefeInmediatoId,TipoDeHorarioId,Dui,Nombre,Apellido,Telefono,Correo,Estado,SalarioBase,FechaContraInicial,FechaContraFinal,Usuario,Password,ConfirmarPassword,PuestoTrabajoId")] Empleado empleado)
        {
            // Realiza validaciones personalizadas para los campos Usuario y Password.
            if (string.IsNullOrWhiteSpace(empleado.Usuario))
            {
                ModelState.AddModelError("Usuario", "El campo Usuario es obligatorio.");
            }

            // Realiza validaciones para verificar si ya existen registros con el mismo DUI, Correo o Teléfono.
            if (string.IsNullOrWhiteSpace(empleado.Password))
            {
                ModelState.AddModelError("Password", "El campo Contraseña es obligatorio.");
            }

            if (await _context.Empleados.AnyAsync(e => e.Dui == empleado.Dui))
            {
                ModelState.AddModelError("Dui", "El DUI ingresado ya está registrado.");
            }

            if (await _context.Empleados.AnyAsync(e => e.Correo == empleado.Correo))
            {
                ModelState.AddModelError("Correo", "El Correo ingresado ya está registrado.");
            }

            if (await _context.Empleados.AnyAsync(e => e.Telefono == empleado.Telefono))
            {
                ModelState.AddModelError("Telefono", "El Telefono ingresado ya está registrado.");
            }

            // Realiza una validación adicional para el campo Usuario.
            if (!string.IsNullOrWhiteSpace(empleado.Usuario))
            {
                if (await _context.Empleados.AnyAsync(e => e.Usuario == empleado.Usuario))
                {
                    ModelState.AddModelError("Usuario", "El usuario ingresado ya está registrado.");
                }
            }

            
              // Obtiene el puesto de trabajo del empleado.
            var puestoTrabajo = await _context.PuestoTrabajos.FindAsync(empleado.PuestoTrabajoId);

            // Define un array de nombres de puestos que no requieren jefe inmediato.
            var rolesSinJefeInmediato = new[] { "Recursos Humanos", "Supervisor", "Administrador de Nómina" };

            // Si el puesto de trabajo del empleado está en la lista de roles sin jefe inmediato,
            // verifica que no se haya asignado un jefe inmediato.
            if (puestoTrabajo != null && rolesSinJefeInmediato.Contains(puestoTrabajo.NombrePuesto))
            {
                if (empleado.JefeInmediatoId != null)
                {
                    ModelState.AddModelError("JefeInmediatoId", "El campo Jefe Inmediato no se puede asignar para este puesto.");
                    empleado.JefeInmediatoId = null;
                }
            }

            // Si el modelo es válido, realiza las siguientes acciones:
            if (ModelState.IsValid)
            {

                // Calcula el hash MD5 de la contraseña del empleado.
                empleado.Password = CalcularHashMD5(empleado.Password);

                // Agrega el empleado a la base de datos.
                _context.Add(empleado);

                // Guarda los cambios en la base de datos de forma asíncrona. 
                await _context.SaveChangesAsync();
                // Redirige a la acción "Index" para mostrar la lista de empleados.
                return RedirectToAction(nameof(Index));
            }

            // Si el modelo no es válido, vuelve a cargar las listas de datos necesarios para la vista.
            ViewData["JefeInmediatoId"] = new SelectList(_context.Empleados.Where(e => e.PuestoTrabajo.NombrePuesto == "Supervisor").ToList(), "Id", "Nombre");
            ViewData["PuestoTrabajoId"] = new SelectList(_context.PuestoTrabajos.Where(e => e.Estado == 1).ToList(), "Id", "NombrePuesto");
            ViewData["NombreHorario"] = new SelectList(_context.TipodeHorarios, "Id", "NombreHorario", empleado.TipoDeHorarioId);

            // Retorna la vista "Create".
            return View(empleado);
        }

        private string CalcularHashMD5(string input)
        {
            // Verifica si la entrada (contraseña) es nula o vacía.
            if (string.IsNullOrEmpty(input))
            {
                // Si la entrada es nula o vacía, retorna null.
                return null;
            }

            // Utiliza el bloque 'using' para asegurar que el objeto MD5 se libere correctamente después de su uso.
            using (MD5 md5 = MD5.Create())
            {
                // Convierte la entrada (contraseña) a un array de bytes usando la codificación UTF-8.
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                // Calcula el hash MD5 del array de bytes de la entrada.
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                // Crea un StringBuilder para construir la representación hexadecimal del hash.
                StringBuilder sb = new StringBuilder();
                // Itera a través de cada byte del hash.
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    // Convierte cada byte a su representación hexadecimal de dos caracteres y lo agrega al StringBuilder.
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                // Retorna la representación hexadecimal del hash MD5 como una cadena.
                return sb.ToString();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSalarioBase(int puestoTrabajoId)
        {
            // Realiza una consulta asíncrona a la base de datos para obtener el salario base del puesto de trabajo especificado.
            var puesto = await _context.PuestoTrabajos
                .Where(p => p.Id == puestoTrabajoId)
                .Select(p => new { SalarioBase = p.SalarioBase })
                .FirstOrDefaultAsync();

            // Verifica si se encontró el puesto de trabajo.
            if (puesto == null)
            {
                // Si no se encontró, retorna un resultado NotFound (código de estado 404).
                return NotFound();
            }

            // Retorna el salario base como un objeto JSON.
            return Json(new { salarioBase = puesto.SalarioBase });
        }

        [HttpGet]
        public async Task<IActionResult> GetPuestoNombre(int puestoTrabajoId)
        {
            // Realiza una consulta asíncrona a la base de datos para obtener el nombre del puesto de trabajo especificado.
            var puesto = await _context.PuestoTrabajos
                .Where(p => p.Id == puestoTrabajoId)
                .Select(p => new { NombrePuesto = p.NombrePuesto })
                .FirstOrDefaultAsync();

            // Verifica si se encontró el puesto de trabajo.
            if (puesto == null)
            {
                // Si no se encontró, retorna un resultado NotFound (código de estado 404).
                return NotFound();
            }

            // Retorna el nombre del puesto de trabajo como un objeto JSON.
            return Json(new { nombrePuesto = puesto.NombrePuesto });
        }

        public async Task<IActionResult> Details(int? id)
        {
            // Verifica si el 'id' proporcionado es nulo.
            if (id == null)
            {
                // Si 'id' es nulo, retorna un resultado NotFound (código de estado 404).
                return NotFound();
            }

            // Realiza una consulta asíncrona a la base de datos para obtener el empleado con el 'id' especificado.
            // Incluye las relaciones 'JefeInmediato', 'PuestoTrabajo' y 'TipoDeHorario' para cargar los datos relacionados.
            var empleado = await _context.Empleados
                .Include(e => e.JefeInmediato)
                .Include(e => e.PuestoTrabajo)
                .Include(e => e.TipoDeHorario)
                .FirstOrDefaultAsync(m => m.Id == id);

            // Verifica si se encontró el empleado.
            if (empleado == null)
            {
                // Si no se encontró, retorna un resultado NotFound (código de estado 404).
                return NotFound();
            }

            // Retorna la vista "Details" con el objeto 'empleado' para mostrar los detalles del empleado.
            return View(empleado);
        }


        public async Task<IActionResult> Delete(int? id)
        {
            // Verifica si el 'id' proporcionado es nulo.
            if (id == null)
            {
                // Si 'id' es nulo, retorna un resultado NotFound (código de estado 404).
                return NotFound();
            }

            // Realiza una consulta asíncrona a la base de datos para obtener el empleado con el 'id' especificado.
            // Incluye las relaciones 'JefeInmediato', 'PuestoTrabajo' y 'TipoDeHorario' para cargar los datos relacionados.
            var empleado = await _context.Empleados
                .Include(e => e.JefeInmediato)
                .Include(e => e.PuestoTrabajo)
                .Include(e => e.TipoDeHorario)
                .FirstOrDefaultAsync(m => m.Id == id);
            // Verifica si se encontró el empleado.
            if (empleado == null)
            {
                // Si no se encontró, retorna un resultado NotFound (código de estado 404).
                return NotFound();
            }

            // Retorna la vista "Delete" con el objeto 'empleado' para mostrar la confirmación de eliminación.
            return View(empleado);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Realiza una consulta asíncrona a la base de datos para obtener el empleado con el 'id' especificado.
            var empleado = await _context.Empleados.FindAsync(id);
            // Verifica si se encontró el empleado.
            if (empleado != null)
            {
                // Si se encontró, elimina el empleado de la base de datos.
                _context.Empleados.Remove(empleado);
            }
            // Guarda los cambios en la base de datos de forma asíncrona.
            await _context.SaveChangesAsync();
            // Redirige a la acción "Index" para mostrar la lista de empleados actualizada.
            return RedirectToAction(nameof(Index));
        }

        private bool EmpleadoExists(int id)
        {
            // Verifica si existe algún empleado con el 'id' especificado en la base de datos.
            return _context.Empleados.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            // Crea una lista de elementos SelectListItem para representar los estados (Activo/Inactivo).
            var estados = new List<SelectListItem>
            {
            new  SelectListItem{ Value="1",Text="Activo" },
            new  SelectListItem{ Value="0",Text="Inactivo" }
            };

            // Asigna la lista de estados al ViewBag para que esté disponible en la vista.
            ViewBag.Estados = estados;

            // Verifica si el 'id' proporcionado es nulo.
            if (id == null)
            {
                // Si 'id' es nulo, retorna un resultado NotFound (código de estado 404).
                return NotFound();
            }

            // Realiza una consulta asíncrona a la base de datos para obtener el empleado con el 'id' especificado.
            var empleado = await _context.Empleados.FindAsync(id);
            // Verifica si se encontró el empleado.
            if (empleado == null)
            {
                // Si no se encontró, retorna un resultado NotFound (código de estado 404).
                return NotFound();
            }

            // Asigna listas de datos necesarios al ViewData para que estén disponibles en la vista.
            ViewData["JefeInmediatoId"] = new SelectList(_context.Empleados.Where(e => e.PuestoTrabajo.NombrePuesto == "Supervisor").ToList(), "Id", "Nombre");
            ViewData["PuestoTrabajoId"] = new SelectList(_context.PuestoTrabajos.Where(e => e.Estado == 1).ToList(), "Id", "NombrePuesto");
            ViewData["NombreHorario"] = new SelectList(_context.TipodeHorarios, "Id", "NombreHorario", empleado.TipoDeHorarioId);

            // Retorna la vista "Edit" con el objeto 'empleado' para mostrar el formulario de edición.
            return View(empleado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,JefeInmediatoId,TipoDeHorarioId,Dui,Nombre,Apellido,Telefono,Correo,Estado,SalarioBase,FechaContraInicial,FechaContraFinal,PuestoTrabajoId")] Empleado empleado)
        {
            // Verifica si el 'id' proporcionado coincide con el 'id' del empleado en el modelo.
            if (id != empleado.Id)
            {
                // Si no coinciden, retorna un resultado NotFound (código de estado 404).
                return NotFound();
            }

            // Realiza una consulta asíncrona a la base de datos para obtener el empleado existente.
            var empleadoExistente = await _context.Empleados.FindAsync(id);
            // Verifica si se encontró el empleado existente.
            if (empleadoExistente == null)
            {
                // Si no se encontró, retorna un resultado NotFound (código de estado 404).
                return NotFound();
            }

            // Conserva los valores de Usuario y Password del empleado existente.
            empleado.Usuario = empleadoExistente.Usuario;
            empleado.Password = empleadoExistente.Password;

            // Obtiene el puesto de trabajo del empleado.
            var puestoTrabajo = await _context.PuestoTrabajos.FindAsync(empleado.PuestoTrabajoId);

            // Define un array de nombres de puestos que no requieren jefe inmediato.
            var rolesSinJefeInmediato = new[] { "Recursos Humanos", "Supervisor", "Administrador de Nómina" };

            // Si el puesto de trabajo del empleado está en la lista de roles sin jefe inmediato,
            // verifica que no se haya asignado un jefe inmediato.
            if (puestoTrabajo != null && rolesSinJefeInmediato.Contains(puestoTrabajo.NombrePuesto))
            {
                if (empleado.JefeInmediatoId != null)
                {
                    ModelState.AddModelError("JefeInmediatoId", "El campo Jefe Inmediato no se puede asignar para este puesto.");
                    empleado.JefeInmediatoId = null;
                }
            }

            // Si el modelo es válido, realiza las siguientes acciones:
            if (ModelState.IsValid)
            {
                try
                {
                    // Actualiza los valores del empleado existente con los valores del empleado modificado.
                    _context.Entry(empleadoExistente).CurrentValues.SetValues(empleado);

                    // Asegura que los valores de Usuario y Password se mantengan sin cambios.
                    empleadoExistente.Usuario = empleado.Usuario;
                    empleadoExistente.Password = empleado.Password;

                    // Guarda los cambios en la base de datos de forma asíncrona.
                    await _context.SaveChangesAsync();
                    // Redirige a la acción "Index" para mostrar la lista de empleados actualizada.
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Maneja la excepción de concurrencia si ocurre.
                    if (!EmpleadoExists(empleado.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            // Si el modelo no es válido, vuelve a cargar las listas de datos necesarios para la vista.
            ViewData["JefeInmediatoId"] = new SelectList(_context.Empleados.Where(e => e.PuestoTrabajo.NombrePuesto == "Supervisor").ToList(), "Id", "Nombre");
            ViewData["PuestoTrabajoId"] = new SelectList(_context.PuestoTrabajos.Where(e => e.Estado == 1).ToList(), "Id", "NombrePuesto");
            ViewData["NombreHorario"] = new SelectList(_context.TipodeHorarios, "Id", "NombreHorario", empleado.TipoDeHorarioId);

            // Retorna la vista "Edit" con el objeto 'empleado' para mostrar los errores de validación.
            return View(empleado);
        }


    }

}
