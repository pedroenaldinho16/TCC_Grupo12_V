using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;
using TCC_Grupo12_III.Services;

namespace TCC_Grupo12_III.Controllers
{

    public class EspeciesController : Controller
    {
        private readonly AppDbContext _context;

        public EspeciesController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string? busca, string? filtro)
        {
            var especies = _context.Especies.AsQueryable();

            if (!string.IsNullOrWhiteSpace(busca))
            {
                especies = especies.Where(e => e.Nome.Contains(busca) || e.NomeCientifico.Contains(busca));
            }

            if (!string.IsNullOrWhiteSpace(filtro) && filtro != "Todos")
            {
                especies = filtro == "Terrestres" ? especies.Where(e => e.Categoria != "Aves") : especies.Where(e => e.Categoria == filtro);
            }

            ViewBag.Busca = busca;
            ViewBag.Filtro = filtro ?? "Todos";
            ViewBag.Total = especies.Count();
            ViewBag.Pokedex = PokedexHelper.Montar(_context, UsuarioId());

            return View(especies.OrderBy(e => e.Nome).ToList());
        }

        public IActionResult Detalhe(int id)
        {
            var especie = _context.Especies.FirstOrDefault(e => e.Id == id);
            if (especie == null)
            {
                return NotFound();
            }

            var uid = UsuarioId();

            // Status na CantaDex: precisa de Quiz Aprovado E Estudo Lido
            var status = PokedexHelper.ObterStatus(_context, uid, id);
            ViewBag.JaConquistada = status.Dominada;
            ViewBag.QuizAprovado = status.QuizAprovado;
            ViewBag.EstudoLido = status.EstudoLido;

            // Busca estudos científicos e pesquisas de campo relacionados a esta espécie
            var todosEstudos = EducacaoService.ObterTodos(_context);
            var estudos = todosEstudos
                .Where(a => a.EspecieId == id
                         || (!string.IsNullOrEmpty(a.EspecieNome) && a.EspecieNome.Equals(especie.Nome, StringComparison.OrdinalIgnoreCase))
                         || (!string.IsNullOrEmpty(a.Titulo) && a.Titulo.Contains(especie.Nome, StringComparison.OrdinalIgnoreCase))
                         || (!string.IsNullOrEmpty(a.Resumo) && a.Resumo.Contains(especie.Nome, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            var estudosPorCategoria = false;
            if (estudos.Count == 0 && !string.IsNullOrWhiteSpace(especie.Categoria))
            {
                // Sugestão de estudos da mesma categoria biológica
                estudos = todosEstudos
                    .Where(a => a.Categoria.Equals(especie.Categoria, StringComparison.OrdinalIgnoreCase))
                    .Take(4)
                    .ToList();
                estudosPorCategoria = true;
            }

            ViewBag.Estudos = estudos;
            ViewBag.EstudosSaoPorCategoria = estudosPorCategoria;

            ViewBag.Quiz = _context.EspecieQuizPerguntas
                .Where(q => q.EspecieId == id)
                .OrderBy(q => q.Ordem)
                .ThenBy(q => q.Id)
                .ToList();

            ViewBag.Outras = _context.Especies
                .Where(e => e.Id != id)
                .OrderBy(e => e.Nome)
                .Take(5)
                .ToList();

            return View(especie);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public IActionResult MarcarEstudoLido(int id, string? returnUrl)
        {
            var uid = UsuarioId();
            if (uid > 0)
            {
                var res = PokedexHelper.RegistrarEstudoLido(_context, uid, id);
                if (res.XpGanho > 0)
                {
                    NivelHelper.Conceder(_context, User, res.XpGanho);
                }
                if (!string.IsNullOrEmpty(res.Mensagem))
                {
                    TempData[res.ConquistadaAgora ? "Sucesso" : "Info"] = res.Mensagem;
                }
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(Detalhe), new { id });
        }

        private int UsuarioId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return id != null ? int.Parse(id) : 0;
        }
    }
}
