using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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

        public async Task<IActionResult> Index(Empleado empleado, int topRegistro = 10, int? mesInicioContrato = null, int? mesFinContrato = null)
        {
            var query = _context.Empleados.AsQueryable();
            if (!string.IsNullOrWhiteSpace(empleado.Dui))
                query = query.Where(s => s.Dui.Contains(empleado.Dui));
            if (!string.IsNullOrWhiteSpace(empleado.Nombre))
                query = query.Where(s => s.Nombre.Contains(empleado.Nombre));
            if (!string.IsNullOrWhiteSpace(empleado.Apellido))
                query = query.Where(s => s.Apellido.Contains(empleado.Apellido));

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

            if (topRegistro > 0)
                query = query.Take(topRegistro);
            query = query
                .Include(p => p.TipoDeHorario).Include(p => p.PuestoTrabajo).Include(p => p.JefeInmediato);

            var puestotrabajos = _context.PuestoTrabajos.ToList();
            puestotrabajos.Add(new PuestoTrabajo { NombrePuesto = "SELECCIONAR", Id = 0 });

            var tipodehorario = _context.TipodeHorarios.ToList();
            tipodehorario.Add(new TipodeHorario { NombreHorario = "SELECCIONAR", Id = 0 });

            var jefesInmediatos = await _context.Empleados.Where(e => e.PuestoTrabajo.NombrePuesto.Contains("Supervisor")).Select(e => new { e.Id, e.Nombre }).ToListAsync();

            jefesInmediatos.Insert(0, new { Id = 0, Nombre = "SELECCIONAR" });


            var meses = Enumerable.Range(1, 12)
                .Select(m => new { Id = m, Nombre = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(m) })
                .ToList();
            meses.Insert(0, new { Id = 0, Nombre = "SELECCIONAR" });



            ViewData["TipoDeHorarioId"] = new SelectList(tipodehorario, "Id", "NombreHorario", 0);
            ViewData["PuestoTrabajoId"] = new SelectList(puestotrabajos, "Id", "NombrePuesto", 0);
            ViewData["JefeInmediatoId"] = new SelectList(items: jefesInmediatos, dataValueField: "Id", dataTextField: "Nombre", selectedValue: 0);
            ViewData["Meses"] = new SelectList(meses, "Id", "Nombre");
            ViewData["MesInicioContratoSeleccionado"] = mesInicioContrato;
            ViewData["MesFinContratoSeleccionado"] = mesFinContrato;


            return View(await query.ToListAsync());
        }

        public IActionResult Create()
        {
            var estados = new List<SelectListItem>
            {
                new  SelectListItem{ Value="1",Text="Activo" },
                new  SelectListItem{ Value="0",Text="Inactivo" }
            };

            ViewBag.Estados = estados;

            ViewData["JefeInmediatoId"] = new SelectList(_context.Empleados.Where(e => e.PuestoTrabajo.NombrePuesto == "Supervisor").ToList(), "Id", "Nombre");
            ViewData["PuestoTrabajoId"] = new SelectList(_context.PuestoTrabajos.Where(e => e.Estado == 1).ToList(), "Id", "NombrePuesto");
            ViewData["NombreHorario"] = new SelectList(_context.TipodeHorarios, "Id", "NombreHorario");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,JefeInmediatoId,TipoDeHorarioId,Dui,Nombre,Apellido,Telefono,Correo,Estado,SalarioBase,FechaContraInicial,FechaContraFinal,Usuario,Password,ConfirmarPassword,PuestoTrabajoId")] Empleado empleado)
        {
            if (string.IsNullOrWhiteSpace(empleado.Usuario))
            {
                ModelState.AddModelError("Usuario", "El campo Usuario es obligatorio.");
            }

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

            if (!string.IsNullOrWhiteSpace(empleado.Usuario))
            {
                if (await _context.Empleados.AnyAsync(e => e.Usuario == empleado.Usuario))
                {
                    ModelState.AddModelError("Usuario", "El usuario ingresado ya está registrado.");
                }
            }

            

            var puestoTrabajo = await _context.PuestoTrabajos.FindAsync(empleado.PuestoTrabajoId);

            var rolesSinJefeInmediato = new[] { "Recursos Humanos", "Supervisor", "Administrador de Nómina" };
            if (puestoTrabajo != null && rolesSinJefeInmediato.Contains(puestoTrabajo.NombrePuesto))
            {
                if (empleado.JefeInmediatoId != null)
                {
                    ModelState.AddModelError("JefeInmediatoId", "El campo Jefe Inmediato no se puede asignar para este puesto.");
                    empleado.JefeInmediatoId = null;
                }
            }


            if (ModelState.IsValid)
            {
                empleado.Password = CalcularHashMD5(empleado.Password);
                _context.Add(empleado);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["JefeInmediatoId"] = new SelectList(_context.Empleados.Where(e => e.PuestoTrabajo.NombrePuesto == "Supervisor").ToList(), "Id", "Nombre");
            ViewData["PuestoTrabajoId"] = new SelectList(_context.PuestoTrabajos.Where(e => e.Estado == 1).ToList(), "Id", "NombrePuesto");
            ViewData["NombreHorario"] = new SelectList(_context.TipodeHorarios, "Id", "NombreHorario", empleado.TipoDeHorarioId);
            return View(empleado);
        }

        private string CalcularHashMD5(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSalarioBase(int puestoTrabajoId)
        {
            var puesto = await _context.PuestoTrabajos
                .Where(p => p.Id == puestoTrabajoId)
                .Select(p => new { SalarioBase = p.SalarioBase })
                .FirstOrDefaultAsync();

            if (puesto == null)
            {
                return NotFound();
            }

            return Json(new { salarioBase = puesto.SalarioBase });
        }

        [HttpGet]
        public async Task<IActionResult> GetPuestoNombre(int puestoTrabajoId)
        {
            var puesto = await _context.PuestoTrabajos
                .Where(p => p.Id == puestoTrabajoId)
                .Select(p => new { NombrePuesto = p.NombrePuesto })
                .FirstOrDefaultAsync();

            if (puesto == null)
            {
                return NotFound();
            }

            return Json(new { nombrePuesto = puesto.NombrePuesto });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleados
                .Include(e => e.JefeInmediato)
                .Include(e => e.PuestoTrabajo)
                .Include(e => e.TipoDeHorario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (empleado == null)
            {
                return NotFound();
            }

            return View(empleado);
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleados
                .Include(e => e.JefeInmediato)
                .Include(e => e.PuestoTrabajo)
                .Include(e => e.TipoDeHorario)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (empleado == null)
            {
                return NotFound();
            }

            return View(empleado);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado != null)
            {
                _context.Empleados.Remove(empleado);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmpleadoExists(int id)
        {
            return _context.Empleados.Any(e => e.Id == id);
        }

        public async Task<IActionResult> Edit(int? id)
        {

            var estados = new List<SelectListItem>
        {
            new  SelectListItem{ Value="1",Text="Activo" },
            new  SelectListItem{ Value="0",Text="Inactivo" }
        };

            ViewBag.Estados = estados;

            if (id == null)
            {
                return NotFound();
            }

            var empleado = await _context.Empleados.FindAsync(id);
            if (empleado == null)
            {
                return NotFound();
            }
            ViewData["JefeInmediatoId"] = new SelectList(_context.Empleados.Where(e => e.PuestoTrabajo.NombrePuesto == "Supervisor").ToList(), "Id", "Nombre");
            ViewData["PuestoTrabajoId"] = new SelectList(_context.PuestoTrabajos.Where(e => e.Estado == 1).ToList(), "Id", "NombrePuesto");
            ViewData["NombreHorario"] = new SelectList(_context.TipodeHorarios, "Id", "NombreHorario", empleado.TipoDeHorarioId);
            return View(empleado);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,JefeInmediatoId,TipoDeHorarioId,Dui,Nombre,Apellido,Telefono,Correo,Estado,SalarioBase,FechaContraInicial,FechaContraFinal,PuestoTrabajoId")] Empleado empleado)
        {
            if (id != empleado.Id)
            {
                return NotFound();
            }

            var empleadoExistente = await _context.Empleados.FindAsync(id);
            if (empleadoExistente == null)
            {
                return NotFound();
            }

            empleado.Usuario = empleadoExistente.Usuario;
            empleado.Password = empleadoExistente.Password;

            var puestoTrabajo = await _context.PuestoTrabajos.FindAsync(empleado.PuestoTrabajoId);
            var rolesSinJefeInmediato = new[] { "Recursos Humanos", "Supervisor", "Administrador de Nómina" };

            if (puestoTrabajo != null && rolesSinJefeInmediato.Contains(puestoTrabajo.NombrePuesto))
            {
                if (empleado.JefeInmediatoId != null)
                {
                    ModelState.AddModelError("JefeInmediatoId", "El campo Jefe Inmediato no se puede asignar para este puesto.");
                    empleado.JefeInmediatoId = null;
                }
            }

            if (ModelState.IsValid)
            {
                try
                {

                    _context.Entry(empleadoExistente).CurrentValues.SetValues(empleado);

                    empleadoExistente.Usuario = empleado.Usuario;
                    empleadoExistente.Password = empleado.Password;

                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmpleadoExists(empleado.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }

            ViewData["JefeInmediatoId"] = new SelectList(_context.Empleados.Where(e => e.PuestoTrabajo.NombrePuesto == "Supervisor").ToList(), "Id", "Nombre");
            ViewData["PuestoTrabajoId"] = new SelectList(_context.PuestoTrabajos.Where(e => e.Estado == 1).ToList(), "Id", "NombrePuesto");
            ViewData["NombreHorario"] = new SelectList(_context.TipodeHorarios, "Id", "NombreHorario", empleado.TipoDeHorarioId);
            return View(empleado);
        }


    }

}
