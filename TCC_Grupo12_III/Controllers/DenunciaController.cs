using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;

namespace TCC_Grupo12_III.Controllers
{
    [Authorize]
    public class DenunciaController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DenunciaController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            return View(new Denuncia());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Denuncia denuncia, IFormFile? foto)
        {
            if (denuncia.Anonima)
            {
                denuncia.NomeContato = null;
                denuncia.EmailContato = null;
                ModelState.Remove("EmailContato");
            }

            if (!ModelState.IsValid)
            {
                return View(denuncia);
            }

            if (foto != null && foto.Length > 0)
            {
                var pastaUploads = Path.Combine(_env.WebRootPath, "uploads", "denuncias");
                Directory.CreateDirectory(pastaUploads);

                var nomeArquivo = $"{Guid.NewGuid()}{Path.GetExtension(foto.FileName)}";
                var caminhoCompleto = Path.Combine(pastaUploads, nomeArquivo);

                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await foto.CopyToAsync(stream);
                }

                denuncia.FotoPath = $"/uploads/denuncias/{nomeArquivo}";
            }

            denuncia.CriadoEm = DateTime.Now;

            _context.Denuncias.Add(denuncia);
            _context.SaveChanges();

            TempData["Xp"] = NivelHelper.Conceder(_context, User, NivelHelper.XpDenuncia);
            TempData["Sucesso"] = "Denúncia enviada com sucesso. Obrigado por proteger a Serra da Cantareira!";
            return RedirectToAction("Index");
        }
    }
}
