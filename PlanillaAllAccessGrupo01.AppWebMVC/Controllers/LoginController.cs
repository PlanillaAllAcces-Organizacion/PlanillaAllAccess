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
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(Empleado empleado)
        {
            empleado.Password = CalcularHashMD5(empleado.Password);
            var empleadoAuth = await _context.Empleados
                .Include(e => e.PuestoTrabajo)
                .FirstOrDefaultAsync(c => c.Usuario == empleado.Usuario && c.Password == empleado.Password);
            if (empleadoAuth != null && empleadoAuth.Id > 0 && empleadoAuth.Usuario == empleado.Usuario)
            {
                var claims = new[] {
                new Claim(ClaimTypes.Name, empleadoAuth.Correo),
                new Claim("Id", empleadoAuth.Id.ToString()),
                new Claim("Usuario", empleadoAuth.Usuario),
                new Claim(ClaimTypes.Role, empleadoAuth.PuestoTrabajo.NombrePuesto)
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
                return RedirectToAction("Index", "Home");

            }else
            {
                ModelState.AddModelError("Usuario", "El usario o contraseña estan incorrectos");
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
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Login");
        }




    }
}
