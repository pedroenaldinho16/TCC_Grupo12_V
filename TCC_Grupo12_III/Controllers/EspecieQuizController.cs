using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;

namespace TCC_Grupo12_III.Controllers
{
    [Authorize]
    public class EspecieQuizController : Controller
    {
        private readonly AppDbContext _context;

        public EspecieQuizController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int id)
        {
            return RedirectToAction("Detalhe", "Especies", new { id }, "quiz");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Enviar(int id, List<QuizRespostaInput> respostas)
        {
            var especie = _context.Especies.Find(id);
            if (especie == null)
            {
                return NotFound();
            }

            var enviadas = (respostas ?? new List<QuizRespostaInput>())
                .Where(r => r.PerguntaId > 0)
                .GroupBy(r => r.PerguntaId)
                .ToDictionary(g => g.Key, g => g.First().OpcaoSelecionada);

            var itens = ObterPerguntas(id)
                .Select(p => new EspecieQuizResultadoItem
                {
                    Pergunta = p,
                    Selecionada = enviadas.TryGetValue(p.Id, out var sel) ? sel : null
                })
                .ToList();

            var model = new EspecieQuizResultadoViewModel { Especie = especie, Itens = itens };
            model.QuizAprovado = model.Total > 0 && model.Percentual >= EspecieQuizResultadoViewModel.PercentualParaDominar;

            var usuarioId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : 0;
            if (model.QuizAprovado)
            {
                var resultado = PokedexHelper.RegistrarQuizConcluido(_context, usuarioId, id);
                var statusAtual = PokedexHelper.ObterStatus(_context, usuarioId, id);
                model.Dominada = statusAtual.Dominada;
                model.EstudoLido = statusAtual.EstudoLido;
                model.StatusMensagem = resultado.Mensagem;
                if (resultado.XpGanho > 0)
                {
                    model.XpMensagem = NivelHelper.Conceder(_context, User, resultado.XpGanho);
                }
            }
            else
            {
                var statusAtual = PokedexHelper.ObterStatus(_context, usuarioId, id);
                model.Dominada = statusAtual.Dominada;
                model.EstudoLido = statusAtual.EstudoLido;
                model.StatusMensagem = "Você precisa acertar pelo menos 70% das perguntas para validar o quiz.";
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_ResultadoConteudo", model);
            }

            return View("Resultado", model);
        }

        private List<EspecieQuizPergunta> ObterPerguntas(int especieId)
        {
            return _context.EspecieQuizPerguntas
                .Where(p => p.EspecieId == especieId)
                .OrderBy(p => p.Ordem)
                .ThenBy(p => p.Id)
                .ToList();
        }
    }
}
