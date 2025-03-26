using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PlanillaAllAccessGrupo01.AppWebMVC.Models;

namespace PlanillaAllAccessGrupo01.AppWebMVC.Controllers
{
    public class VacacionController : Controller
    {
        private readonly PlanillaDbContext _context;

        public VacacionController(PlanillaDbContext context)
        {
            _context = context;
        }

       
    }
}
