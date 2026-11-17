using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;
using TCC_Grupo12_III.Services;

namespace TCC_Grupo12_III.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminNiveisController : Controller
    {
        private readonly AppDbContext _context;
        private readonly NivelJogoService _nivelJogo;

        public AdminNiveisController(AppDbContext context, NivelJogoService nivelJogo)
        {
            _context = context;
            _nivelJogo = nivelJogo;
        }

        public IActionResult Index()
        {
            var niveis = _context.NivelJogos
                .OrderByDescending(n => n.Data)
                .ThenByDescending(n => n.Id)
                .ToList();

            ViewBag.NivelDoDiaId = _nivelJogo.ObterNivelDoDia(DateTime.Today)?.Id;

            return View(niveis);
        }

        public IActionResult Create()
        {
            return View("Editor", new AdminNivelViewModel
            {
                DadosJson = NivelJogoService.NivelVazio().Serializar(),
                Especies = CarregarEspecies()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AdminNivelViewModel model)
        {
            if (!ValidarNivel(model))
            {
                model.Especies = CarregarEspecies();
                return View("Editor", model);
            }

            var dados = NivelJogoDados.Desserializar(model.DadosJson)!;

            _context.NivelJogos.Add(new NivelJogo
            {
                Titulo = model.Titulo,
                Descricao = model.Descricao,
                Data = model.Data.Date,
                Ativo = model.Ativo,
                DadosJson = dados.Serializar(),
                TotalAnimais = dados.Animais.Count
            });
            _context.SaveChanges();

            TempData["Sucesso"] = "Nível criado com sucesso.";
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var nivel = _context.NivelJogos.Find(id);
            if (nivel == null)
            {
                return RedirectToAction("Index");
            }

            return View("Editor", new AdminNivelViewModel
            {
                Id = nivel.Id,
                Titulo = nivel.Titulo,
                Descricao = nivel.Descricao,
                Data = nivel.Data,
                Ativo = nivel.Ativo,
                DadosJson = nivel.DadosJson,
                Especies = CarregarEspecies()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, AdminNivelViewModel model)
        {
            var nivel = _context.NivelJogos.Find(id);
            if (nivel == null)
            {
                return RedirectToAction("Index");
            }

            if (!ValidarNivel(model))
            {
                model.Especies = CarregarEspecies();
                return View("Editor", model);
            }

            var dados = NivelJogoDados.Desserializar(model.DadosJson)!;

            nivel.Titulo = model.Titulo;
            nivel.Descricao = model.Descricao;
            nivel.Data = model.Data.Date;
            nivel.Ativo = model.Ativo;
            nivel.DadosJson = dados.Serializar();
            nivel.TotalAnimais = dados.Animais.Count;
            _context.SaveChanges();

            TempData["Sucesso"] = "Nível atualizado com sucesso.";
            return RedirectToAction("Index");
        }

        public IActionResult Testar(int id)
        {
            var nivel = _context.NivelJogos.Find(id);
            var dados = NivelJogoDados.Desserializar(nivel?.DadosJson);
            if (nivel == null || dados == null)
            {
                return RedirectToAction("Index");
            }

            return View("~/Views/Jogo/Jogar.cshtml", new JogoJogarViewModel
            {
                Nivel = nivel,
                DadosJson = dados.Serializar(),
                Animais = _nivelJogo.MontarInfoAnimais(dados),
                ModoTeste = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var nivel = _context.NivelJogos.Find(id);
            if (nivel != null)
            {
                _context.NivelJogos.Remove(nivel);
                _context.SaveChanges();
                TempData["Sucesso"] = "Nível removido.";
            }

            return RedirectToAction("Index");
        }

        private bool ValidarNivel(AdminNivelViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return false;
            }

            var especiesValidas = _context.Especies.Select(e => e.Id).ToHashSet();
            if (!NivelJogoService.Validar(model.DadosJson, especiesValidas, out var dados, out var erro))
            {
                ModelState.AddModelError(nameof(model.DadosJson), erro);
                return false;
            }

            model.DadosJson = dados!.Serializar();
            return true;
        }

        private List<Especie> CarregarEspecies()
        {
            return _context.Especies.OrderBy(e => e.Nome).ToList();
        }
    }
}
