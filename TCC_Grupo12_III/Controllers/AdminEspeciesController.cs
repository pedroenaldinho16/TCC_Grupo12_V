using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;

namespace TCC_Grupo12_III.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminEspeciesController : Controller
    {
        private static readonly HashSet<string> ExtensoesPermitidas = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp", ".gif"
        };

        private const long TamanhoMaximoBytes = 5 * 1024 * 1024;

        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminEspeciesController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IActionResult Index()
        {
            var especies = _context.Especies.OrderBy(e => e.Nome).ToList();
            return View(especies);
        }

        public IActionResult Create()
        {
            return View(new Especie());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Especie especie, IFormFile? imagem)
        {
            NormalizarCamposOpcionais(especie);
            var caminhoImagem = SalvarImagem(imagem);

            if (!ModelState.IsValid)
            {
                return View(especie);
            }

            if (caminhoImagem != null)
            {
                especie.ImagemUrl = caminhoImagem;
            }

            _context.Especies.Add(especie);
            _context.SaveChanges();

            TempData["Sucesso"] = "Espécie cadastrada com sucesso!";
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var especie = _context.Especies.Find(id);
            if (especie == null)
            {
                return NotFound();
            }

            return View(especie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Especie especie, IFormFile? imagem, bool removerImagem = false)
        {
            NormalizarCamposOpcionais(especie);
            var caminhoImagem = SalvarImagem(imagem);

            if (!ModelState.IsValid)
            {
                return View(especie);
            }

            if (caminhoImagem != null)
            {
                especie.ImagemUrl = caminhoImagem;
            }
            else if (removerImagem)
            {
                especie.ImagemUrl = null;
            }

            _context.Especies.Update(especie);
            _context.SaveChanges();

            TempData["Sucesso"] = "Espécie atualizada com sucesso!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var especie = _context.Especies.Find(id);
            if (especie != null)
            {
                _context.Especies.Remove(especie);
                _context.SaveChanges();
            }

            TempData["Sucesso"] = "Espécie removida com sucesso!";
            return RedirectToAction("Index");
        }

        private void NormalizarCamposOpcionais(Especie especie)
        {
            foreach (var p in typeof(Especie).GetProperties().Where(p => p.PropertyType == typeof(string)
                && p.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute), true).Length == 0
                && p.GetValue(especie) == null))
            {
                p.SetValue(especie, string.Empty);
                ModelState.Remove(p.Name);
                ModelState.Remove("especie." + p.Name);
            }
        }
        private string? SalvarImagem(IFormFile? imagem)
        {
            if (imagem == null || imagem.Length == 0)
            {
                return null;
            }

            var extensao = Path.GetExtension(imagem.FileName);
            if (!ExtensoesPermitidas.Contains(extensao))
            {
                ModelState.AddModelError("imagem", "Formato inválido. Use JPG, PNG, WEBP ou GIF.");
                return null;
            }

            if (imagem.Length > TamanhoMaximoBytes)
            {
                ModelState.AddModelError("imagem", "A imagem deve ter no máximo 5 MB.");
                return null;
            }

            var pasta = Path.Combine(_environment.WebRootPath, "img", "especies");
            Directory.CreateDirectory(pasta);

            var nomeArquivo = $"{Guid.NewGuid():N}{extensao.ToLowerInvariant()}";
            using (var stream = System.IO.File.Create(Path.Combine(pasta, nomeArquivo)))
            {
                imagem.CopyTo(stream);
            }

            return $"/img/especies/{nomeArquivo}";
        }
    }
}
