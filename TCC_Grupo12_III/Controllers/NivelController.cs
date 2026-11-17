using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;

namespace TCC_Grupo12_III.Controllers
{
    [Authorize]
    public class NivelController : Controller
    {
        private readonly AppDbContext _context;

        public NivelController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(bool bloqueado = false)
        {
            return View(new NivelViewModel
            {
                Xp = NivelHelper.ObterXp(_context, User),
                Admin = User.IsInRole("Admin"),
                Bloqueado = bloqueado
            });
        }
    }
}
