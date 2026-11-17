using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Linq;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;

namespace TCC_Grupo12_III.Controllers
{
    public class HomeController : Controller
    {
        private const string StatusCriticamenteAmeacado = "Criticamente Ameaçado";

        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Sobre() => View();
        public IActionResult Preservar() => View();
        public IActionResult Index()
        {
            ViewBag.EspeciesCriticas = _context.Especies
                .Where(e => e.StatusConservacao == StatusCriticamenteAmeacado)
                .ToList()
                .OrderBy(_ => Random.Shared.Next())
                .Take(5)
                .ToList();

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
