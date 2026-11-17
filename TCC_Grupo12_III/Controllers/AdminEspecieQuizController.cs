using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;

namespace TCC_Grupo12_III.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminEspecieQuizController : Controller
    {
        private readonly AppDbContext _context;

        public AdminEspecieQuizController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.Totais = _context.EspecieQuizPerguntas
                .GroupBy(p => p.EspecieId)
                .Select(g => new { g.Key, Total = g.Count() })
                .ToDictionary(x => x.Key, x => x.Total);

            return View(_context.Especies.OrderBy(e => e.Nome).ToList());
        }

        public IActionResult Gerenciar(int id)
        {
            var especie = _context.Especies.Find(id);
            if (especie == null)
            {
                return NotFound();
            }

            var perguntas = _context.EspecieQuizPerguntas
                .Where(p => p.EspecieId == id)
                .OrderBy(p => p.Ordem)
                .ThenBy(p => p.Id)
                .ToList();

            return View(MontarViewModel(especie, perguntas));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Gerenciar(AdminEspecieQuizViewModel model)
        {
            var especie = _context.Especies.Find(model.EspecieId);
            if (especie == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                model.EspecieNome = especie.Nome;
                model.EspecieNomeCientifico = especie.NomeCientifico;
                return View(model);
            }

            var existentes = _context.EspecieQuizPerguntas
                .Where(p => p.EspecieId == model.EspecieId)
                .ToList();

            var idsMantidos = model.Perguntas.Where(p => p.Id != 0).Select(p => p.Id).ToHashSet();
            var removidas = existentes.Where(p => !idsMantidos.Contains(p.Id)).ToList();
            _context.EspecieQuizPerguntas.RemoveRange(removidas);

            var ordem = 0;
            foreach (var entrada in model.Perguntas)
            {
                var pergunta = entrada.Id != 0
                    ? existentes.FirstOrDefault(p => p.Id == entrada.Id)
                    : null;

                if (pergunta == null)
                {
                    pergunta = new EspecieQuizPergunta { EspecieId = model.EspecieId };
                    _context.EspecieQuizPerguntas.Add(pergunta);
                }

                pergunta.Enunciado = entrada.Enunciado;
                pergunta.OpcaoA = entrada.OpcaoA;
                pergunta.OpcaoB = entrada.OpcaoB;
                pergunta.OpcaoC = entrada.OpcaoC;
                pergunta.OpcaoD = entrada.OpcaoD;
                pergunta.RespostaCorreta = entrada.RespostaCorreta;
                pergunta.Ordem = ordem;

                ordem++;
            }

            _context.SaveChanges();

            TempData["Sucesso"] = "Quiz atualizado com sucesso!";
            return RedirectToAction("Gerenciar", new { id = model.EspecieId });
        }

        private static AdminEspecieQuizViewModel MontarViewModel(Especie especie, List<EspecieQuizPergunta> perguntas)
        {
            return new AdminEspecieQuizViewModel
            {
                EspecieId = especie.Id,
                EspecieNome = especie.Nome,
                EspecieNomeCientifico = especie.NomeCientifico,
                Perguntas = perguntas.Select(p => new AdminEspecieQuizPerguntaViewModel
                {
                    Id = p.Id,
                    Enunciado = p.Enunciado,
                    OpcaoA = p.OpcaoA,
                    OpcaoB = p.OpcaoB,
                    OpcaoC = p.OpcaoC,
                    OpcaoD = p.OpcaoD,
                    RespostaCorreta = p.RespostaCorreta
                }).ToList()
            };
        }
    }
}
