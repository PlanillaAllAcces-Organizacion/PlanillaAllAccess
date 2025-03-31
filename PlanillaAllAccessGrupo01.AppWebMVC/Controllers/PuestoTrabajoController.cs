using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlanillaAllAccessGrupo01.AppWebMVC.Models;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Controllers
{
    [Authorize]
    public class PuestoTrabajoController : Controller
    {
        private readonly PlanillaDbContext _context;

        public PuestoTrabajoController(PlanillaDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string nombrePuesto, byte estado = 2, int top = 10)
        {
            // Crea un diccionario para mapear estados (1: Activo, 0: Inactivo)
            var estados = new Dictionary<byte, string>
    {
        { 1, "Activo" },
        { 0, "Inactivo" }
    };

            // Pasa los estados a la vista mediante ViewBag
            ViewBag.Estados = estados;

            // Obtiene la consulta base de puestos de trabajo
            var query = _context.PuestoTrabajos.AsQueryable();

            // Filtra por nombre de puesto si se proporcionó
            if (!string.IsNullOrWhiteSpace(nombrePuesto))
                query = query.Where(p => p.NombrePuesto.Contains(nombrePuesto));

            // Filtra por estado (2 significa "Todos" por defecto)
            if (estado != 2) // 2 indica "Todos los estados"
                query = query.Where(p => p.Estado == estado);

            // Limita el número de resultados
            query = query.Take(top);

            // Ejecuta la consulta y devuelve la vista con los resultados
            return View(await query.ToListAsync());
        }
        public IActionResult Create()
        {
            // Prepara una lista de estados para el dropdown (Activo/Inactivo)
            var estados = new List<SelectListItem>
            {
                 new SelectListItem{ Value="1",Text="Activo" },
                 new SelectListItem{ Value="0",Text="Inactivo" }
            };

            // Pasa los estados a la vista
            ViewBag.Estados = estados;

            // Devuelve la vista de creación
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NombrePuesto,SalarioBase,ValorxHora,ValorExtra,Estado")] PuestoTrabajo puestoTrabajo)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Validación adicional para valores positivos
                    if (puestoTrabajo.SalarioBase <= 0 || puestoTrabajo.ValorxHora <= 0 || puestoTrabajo.ValorExtra <= 0)
                    {
                        ModelState.AddModelError("", "Los valores numéricos deben ser mayores a cero");
                        ViewBag.Estados = GetEstadosList();
                        return View(puestoTrabajo);
                    }

                    puestoTrabajo.FechaCreacion = DateTime.Now;
                    _context.Add(puestoTrabajo);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Puesto '{puestoTrabajo.NombrePuesto}' creado exitosamente";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException ex)
            {
                // Log del error
                Console.WriteLine($"Error al crear puesto: {ex.Message}");
                ModelState.AddModelError("", "No se pudo guardar. Verifique los datos e intente nuevamente.");
            }

            ViewBag.Estados = GetEstadosList();
            return View(puestoTrabajo);
        }

        // Método auxiliar para cargar los estados
        private List<SelectListItem> GetEstadosList()
        {
            return new List<SelectListItem>
    {
        new SelectListItem { Value = "1", Text = "Activo" },
        new SelectListItem { Value = "0", Text = "Inactivo" }
    };
        }

        public async Task<IActionResult> Edit(int? id)
        {
            // Prepara la lista de estados para el dropdown
            var estados = new List<SelectListItem>
    {
        new SelectListItem{ Value="1",Text="Activo" },
        new SelectListItem{ Value="0",Text="Inactivo" }
    };

            // Pasa los estados a la vista
            ViewBag.Estados = estados;

            // Verifica si se proporcionó un ID
            if (id == null)
            {
                return NotFound();
            }

            // Busca el puesto de trabajo por ID
            var puestoTrabajo = await _context.PuestoTrabajos.FindAsync(id);

            // Si no se encuentra, devuelve NotFound
            if (puestoTrabajo == null)
            {
                return NotFound();
            }

            // Devuelve la vista de edición con los datos del puesto
            return View(puestoTrabajo);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NombrePuesto,SalarioBase,ValorxHora,ValorExtra,Estado")] PuestoTrabajo puestoTrabajo)
        {
            // Verifica que el ID coincida con el modelo
            if (id != puestoTrabajo.Id)
            {
                return NotFound();
            }

            // Verifica si el modelo es válido
            if (ModelState.IsValid)
            {
                try
                {
                    // Actualiza el puesto de trabajo en el contexto
                    _context.Update(puestoTrabajo);

                    // Guarda los cambios en la base de datos
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Maneja errores de concurrencia
                    if (!PuestoTrabajoExists(puestoTrabajo.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Redirige al listado principal después de editar
                return RedirectToAction(nameof(Index));
            }

            // Si hay errores, muestra nuevamente el formulario con los datos
            return View(puestoTrabajo);
        }
        private bool PuestoTrabajoExists(int id)
        {
            // Verifica si existe un puesto de trabajo con el ID especificado
            return _context.PuestoTrabajos.Any(e => e.Id == id);
        }
        public async Task<IActionResult> Details(int? id)
        {
            // Verifica si se proporcionó un ID
            if (id == null)
            {
                return NotFound();
            }

            // Busca el puesto de trabajo por ID
            var puestoTrabajo = await _context.PuestoTrabajos
                .FirstOrDefaultAsync(m => m.Id == id);

            // Si no se encuentra, devuelve NotFound
            if (puestoTrabajo == null)
            {
                return NotFound();
            }

            // Devuelve la vista de detalles con los datos del puesto
            return View(puestoTrabajo);
        }


    }
}
