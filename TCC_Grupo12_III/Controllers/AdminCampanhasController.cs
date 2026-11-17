using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;

namespace TCC_Grupo12_III.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminCampanhasController : Controller
    {
        private static readonly HashSet<string> ExtensoesPermitidas = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".webp", ".gif"
        };

        private const long TamanhoMaximoBytes = 5 * 1024 * 1024;

        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AdminCampanhasController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index(string? aba)
        {
            var totalPendentes = await _context.Campanhas.CountAsync(c => c.StatusAprovacao == "Pendente");

            if (string.IsNullOrWhiteSpace(aba))
            {
                aba = "ativas";
            }
            else
            {
                aba = aba.ToLowerInvariant();
            }

            var query = _context.Campanhas
                .Include(c => c.Usuario)
                .AsQueryable();

            if (aba == "pendentes")
            {
                query = query.Where(c => c.StatusAprovacao == "Pendente");
            }
            else if (aba == "ativas")
            {
                query = query.Where(c => c.Ativa && (c.StatusAprovacao == "Aprovado" || string.IsNullOrEmpty(c.StatusAprovacao)));
            }
            else if (aba == "encerradas")
            {
                query = query.Where(c => !c.Ativa || c.StatusAprovacao == "Reprovado");
            }

            var campanhas = await query
                .OrderByDescending(c => c.DataCriacao)
                .ToListAsync();

            ViewBag.AbaAtiva = aba;
            ViewBag.TotalPendentes = totalPendentes;
            ViewBag.TotalAtivas = await _context.Campanhas.CountAsync(c => c.Ativa && (c.StatusAprovacao == "Aprovado" || string.IsNullOrEmpty(c.StatusAprovacao)));
            ViewBag.TotalEncerradas = await _context.Campanhas.CountAsync(c => !c.Ativa || c.StatusAprovacao == "Reprovado");

            return View(campanhas);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprovar(int id, string? returnUrl)
        {
            var campanha = await _context.Campanhas.FindAsync(id);
            if (campanha == null)
            {
                return NotFound();
            }

            campanha.StatusAprovacao = "Aprovado";
            campanha.Ativa = true;
            campanha.DataResposta = DateTime.Now;
            campanha.AprovadoPorNome = User.Identity?.Name ?? "Moderação";
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"A campanha \"{campanha.Titulo}\" foi aprovada com sucesso e já está disponível para a comunidade!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index), new { aba = "pendentes" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reprovar(int id, string? motivo, string? returnUrl)
        {
            var campanha = await _context.Campanhas.FindAsync(id);
            if (campanha == null)
            {
                return NotFound();
            }

            campanha.StatusAprovacao = "Reprovado";
            campanha.DataResposta = DateTime.Now;
            campanha.MotivoReprovacao = string.IsNullOrWhiteSpace(motivo)
                ? "A campanha não atende aos critérios ecológicos ou logísticos da plataforma."
                : motivo.Trim();
            campanha.AprovadoPorNome = User.Identity?.Name ?? "Moderação";
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"A proposta da campanha \"{campanha.Titulo}\" foi reprovada.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index), new { aba = "pendentes" });
        }

        public IActionResult Create()
        {
            return View(new Campanha
            {
                Meta = 100,
                Ativa = true,
                StatusAprovacao = "Aprovado"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Campanha campanha, IFormFile? imagem)
        {
            var caminhoImagem = SalvarImagem(imagem);

            if (string.IsNullOrWhiteSpace(campanha.Periodo) && campanha.DataInicio.HasValue)
            {
                campanha.Periodo = campanha.DataInicio.Value.ToString("dd/MM/yyyy");
                ModelState.Remove("Periodo");
            }

            if (!ModelState.IsValid)
            {
                return View(campanha);
            }

            if (caminhoImagem != null)
            {
                campanha.ImagemUrl = caminhoImagem;
            }

            campanha.DataCriacao = DateTime.Now;
            campanha.DataResposta = DateTime.Now;
            campanha.AprovadoPorNome = User.Identity?.Name ?? "Moderação";
            if (string.IsNullOrEmpty(campanha.StatusAprovacao))
            {
                campanha.StatusAprovacao = "Aprovado";
            }

            _context.Campanhas.Add(campanha);
            _context.SaveChanges();

            TempData["Sucesso"] = "Campanha criada com sucesso!";
            return RedirectToAction("Index", new { aba = "ativas" });
        }

        public IActionResult Edit(int id)
        {
            var campanha = _context.Campanhas.Find(id);
            if (campanha == null)
            {
                return NotFound();
            }

            return View(campanha);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Campanha model, IFormFile? imagem, bool removerImagem = false)
        {
            var caminhoImagem = SalvarImagem(imagem);

            if (string.IsNullOrWhiteSpace(model.Periodo) && model.DataInicio.HasValue)
            {
                model.Periodo = model.DataInicio.Value.ToString("dd/MM/yyyy");
                ModelState.Remove("Periodo");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var campanha = _context.Campanhas.Find(model.Id);
            if (campanha == null)
            {
                return NotFound();
            }

            campanha.Titulo = model.Titulo;
            campanha.Descricao = model.Descricao;
            campanha.Periodo = model.Periodo;
            campanha.Participantes = model.Participantes;
            campanha.Meta = model.Meta;
            campanha.Ativa = model.Ativa;
            campanha.ObjetivoPrincipal = model.ObjetivoPrincipal;
            campanha.ObjetivosEspecificos = model.ObjetivosEspecificos;
            campanha.DataInicio = model.DataInicio;
            campanha.DataFim = model.DataFim;
            campanha.Horario = model.Horario;
            campanha.LocalEncontro = model.LocalEncontro;
            campanha.PublicoAlvo = model.PublicoAlvo;
            campanha.OQueLevar = model.OQueLevar;
            campanha.Cronograma = model.Cronograma;
            campanha.ImpactoEsperado = model.ImpactoEsperado;
            campanha.Organizador = model.Organizador;
            campanha.StatusAprovacao = model.StatusAprovacao;

            if (caminhoImagem != null)
            {
                campanha.ImagemUrl = caminhoImagem;
            }
            else if (removerImagem)
            {
                campanha.ImagemUrl = null;
            }

            _context.SaveChanges();

            TempData["Sucesso"] = "Campanha atualizada com sucesso!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var campanha = _context.Campanhas.Find(id);
            if (campanha != null)
            {
                _context.Campanhas.Remove(campanha);
                _context.SaveChanges();
            }

            TempData["Sucesso"] = "Campanha removida com sucesso!";
            return RedirectToAction("Index");
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

            var pasta = Path.Combine(_environment.WebRootPath, "img", "campanhas");
            Directory.CreateDirectory(pasta);

            var nomeArquivo = $"{Guid.NewGuid():N}{extensao.ToLowerInvariant()}";
            using (var stream = System.IO.File.Create(Path.Combine(pasta, nomeArquivo)))
            {
                imagem.CopyTo(stream);
            }

            return $"/img/campanhas/{nomeArquivo}";
        }
    }
}
