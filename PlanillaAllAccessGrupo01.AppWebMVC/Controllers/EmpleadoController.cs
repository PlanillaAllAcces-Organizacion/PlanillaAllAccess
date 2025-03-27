using System;
using System.Collections.Generic;
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

        public async Task<IActionResult> Index()
        {
            var planillaDbContext = _context.Empleados.Include(e => e.JefeInmediato).Include(e => e.PuestoTrabajo).Include(e => e.TipoDeHorario).Include(e => e.Vacacions);
            return View(await planillaDbContext.ToListAsync());
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

            var rolesSinJefeInmediato = new[] { "Gerente de Recursos Humanos", "Supervisor", "Administrador de Nómina" };
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
            ViewData["PuestoTrabajoId"] = new SelectList(_context.PuestoTrabajos, "Id", "NombrePuesto", empleado.PuestoTrabajoId);
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


    }

}
