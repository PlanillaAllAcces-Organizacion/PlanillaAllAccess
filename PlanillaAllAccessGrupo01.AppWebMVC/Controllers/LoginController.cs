using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlanillaAllAccessGrupo01.AppWebMVC.Models;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Controllers
{
    public class LoginController : Controller
    {
        private readonly PlanillaDbContext _context;

        public LoginController(PlanillaDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            // Esta acción HTTP GET simplemente muestra la vista de inicio de sesión.
            // El atributo [AllowAnonymous] permite que usuarios no autenticados accedan a esta acción.
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Empleado empleado)
        {
            // Esta acción HTTP POST maneja el envío del formulario de inicio de sesión.
            // Calcula el hash MD5 de la contraseña ingresada por el usuario.
            empleado.Password = CalcularHashMD5(empleado.Password);

            // Realiza una consulta asíncrona a la base de datos para buscar un empleado con el usuario y contraseña proporcionados.
            // Incluye la relación 'PuestoTrabajo' para obtener el nombre del puesto del empleado.
            var empleadoAuth = await _context.Empleados
                .Include(e => e.PuestoTrabajo)
                .FirstOrDefaultAsync(c => c.Usuario == empleado.Usuario && c.Password == empleado.Password);

            // Verifica si se encontró un empleado con las credenciales proporcionadas.
            if (empleadoAuth != null && empleadoAuth.Id > 0 && empleadoAuth.Usuario == empleado.Usuario)
            {
                // Si se encontró un empleado, crea una lista de claims (afirmaciones) para la autenticación.
                var claims = new[] {
                new Claim(ClaimTypes.Name, empleadoAuth.Correo), 
                new Claim("Id", empleadoAuth.Id.ToString()), 
                new Claim("Usuario", empleadoAuth.Usuario),
                new Claim(ClaimTypes.Role, empleadoAuth.PuestoTrabajo.NombrePuesto) 
                };

                // Crea una identidad de claims utilizando el esquema de autenticación de cookies.
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Inicia sesión del usuario utilizando la identidad de claims y el esquema de autenticación de cookies.
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                // Redirige al usuario a la acción "Index" del controlador "Home".
                return RedirectToAction("Index", "Home");

            }else
            {
                // Si no se encontró un empleado con las credenciales proporcionadas, agrega un error al ModelState.
                ModelState.AddModelError("Usuario", "El usario o contraseña estan incorrectos");

                // Retorna la vista de inicio de sesión con los errores de validación.
                return View();
            }


        }

        private string CalcularHashMD5(string input)
        {
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

        [AllowAnonymous]
        public async Task<IActionResult> CerrarSession()
        {
            // Esta acción HTTP GET cierra la sesión del usuario.
            // El atributo [AllowAnonymous] permite que cualquier usuario acceda a esta acción.

            // Cierra la sesión del usuario utilizando el esquema de autenticación de cookies.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Login");
        }


        public async Task<IActionResult> Perfil()
        {
            // Esta acción HTTP GET muestra el perfil del usuario autenticado.

            // Obtiene el ID del usuario autenticado de los claims.
            var idStr = User.FindFirst("Id")?.Value;

            // Convierte el ID a un entero.
            int id = int.Parse(idStr);

            // Realiza una consulta asíncrona a la base de datos para obtener el empleado con el ID especificado.
            var user = await _context.Empleados.FindAsync(id);

            // Retorna la vista "Perfil" con el objeto 'user' para mostrar los detalles del perfil.
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Perfil(int id, [Bind("Id,TipoDeHorarioId,Dui,Nombre,Apellido,Telefono,Correo,Estado,SalarioBase,FechaContraInicial,FechaContraFinal,Usuario,Password,ConfirmarPassword,PuestoTrabajoId")] Empleado empleado)
        {
            // Esta acción HTTP POST maneja la actualización del perfil del usuario.

            // Verifica si el 'id' proporcionado coincide con el 'id' del empleado en el modelo.
            if (id != empleado.Id)
            {
                // Si no coinciden, retorna un resultado NotFound (código de estado 404)
                return NotFound();
            }

            // Realiza una consulta asíncrona a la base de datos para obtener el empleado existente.
            var EmpleadoUpdate = await _context.Empleados.FindAsync(id); // Usa FindAsync

            // Verifica si se encontró el empleado existente.
            if (EmpleadoUpdate == null)
            {
                // Si no se encontró, retorna un resultado NotFound (código de estado 404).
                return NotFound();
            }

            try
            {
                // Actualiza los valores del empleado existente con los valores del empleado modificado.
                EmpleadoUpdate.Dui = empleado.Dui;
                EmpleadoUpdate.Nombre = empleado.Nombre;
                EmpleadoUpdate.Apellido = empleado.Apellido;
                EmpleadoUpdate.Correo = empleado.Correo;

                // Verifica si se proporcionó una nueva contraseña.
                if (!string.IsNullOrEmpty(empleado.Password))
                {
                    // Si se proporcionó, calcula el hash MD5 de la nueva contraseña y la actualiza.
                    EmpleadoUpdate.Password = CalcularHashMD5(empleado.Password);
                }

                EmpleadoUpdate.FechaContraInicial = empleado.FechaContraInicial;
                EmpleadoUpdate.FechaContraFinal = empleado.FechaContraFinal;
                EmpleadoUpdate.Usuario = empleado.Usuario;

                // Actualiza el empleado en la base de datos.
                _context.Update(EmpleadoUpdate);
                // Guarda los cambios en la base de datos de forma asíncrona.
                await _context.SaveChangesAsync();

                // Redirige al usuario a la acción "Index" del controlador "Home".
                return RedirectToAction("Index", "Home");
            }
            catch (DbUpdateConcurrencyException)
            {
                // Maneja la excepción de concurrencia si ocurre.
                if (!EmpleadoExists(empleado.Id))
                {
                    return NotFound();
                }
                else
                {
                    // Si ocurre una excepción de concurrencia, retorna la vista "Perfil" con el objeto 'empleado' y los errores de validación.
                    return View(empleado);
                }
            }
        }

        private bool EmpleadoExists(int id)
        {
            return _context.Empleados.Any(e => e.Id == id);
        }
    }
}
