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
    [Authorize]
    public class JogoController : Controller
    {
        public const int VidasPorTentativa = 3;

        private readonly AppDbContext _context;
        private readonly NivelJogoService _nivelJogo;

        public JogoController(AppDbContext context, NivelJogoService nivelJogo)
        {
            _context = context;
            _nivelJogo = nivelJogo;
        }

        public IActionResult Index()
        {
            var nivel = _nivelJogo.ObterNivelDoDia(DateTime.Today);
            if (nivel == null)
            {
                return View("SemNivel");
            }

            var uid = UsuarioId();
            var especies = _context.Especies.OrderBy(e => e.Nome).ToList();
            var reservas = _context.ReservasAvistamento
                .Where(r => r.UsuarioId == uid)
                .ToDictionary(r => r.EspecieId, r => r.QuantidadeDisponivel);

            var dados = NivelJogoDados.Desserializar(nivel.DadosJson);
            var especieSugeridaId = dados?.Animais.FirstOrDefault()?.EspecieId ?? especies.FirstOrDefault()?.Id;

            var model = new JogoIndexViewModel
            {
                Nivel = nivel,
                Tentativa = ObterTentativaDoDia(nivel.Id),
                TotalAnimais = dados?.Animais.Count ?? nivel.TotalAnimais,
                EspeciesDisponiveis = especies,
                ReservaPorEspecie = reservas,
                EspecieSelecionadaId = especieSugeridaId
            };

            return View(model);
        }

        public IActionResult Jogar(int? especieId)
        {
            var nivel = _nivelJogo.ObterNivelDoDia(DateTime.Today);
            if (nivel == null)
            {
                return RedirectToAction("Index");
            }

            var dados = NivelJogoDados.Desserializar(nivel.DadosJson);
            if (dados == null)
            {
                return RedirectToAction("Index");
            }

            // Espécie alvo que o usuário escolheu para salvar no final do nível
            Especie? especieAlvo = null;
            if (especieId.HasValue && especieId.Value > 0)
            {
                especieAlvo = _context.Especies.Find(especieId.Value);
            }

            if (especieAlvo == null)
            {
                var primeiroAnimalId = dados.Animais.FirstOrDefault()?.EspecieId;
                if (primeiroAnimalId.HasValue)
                {
                    especieAlvo = _context.Especies.Find(primeiroAnimalId.Value);
                }
                especieAlvo ??= _context.Especies.OrderBy(e => e.Id).FirstOrDefault();
            }

            if (ObterTentativaDoDia(nivel.Id) == null)
            {
                _context.JogoTentativas.Add(new JogoTentativa
                {
                    NivelJogoId = nivel.Id,
                    UsuarioId = UsuarioId(),
                    Dia = DateTime.Today,
                    IniciadoEm = DateTime.UtcNow,
                    TotalAnimais = dados.Animais.Count
                });
                _context.SaveChanges();
            }

            var animaisMap = _nivelJogo.MontarInfoAnimais(dados);
            if (especieAlvo != null && !animaisMap.ContainsKey(especieAlvo.Id))
            {
                animaisMap[especieAlvo.Id] = new JogoAnimalInfo
                {
                    Nome = especieAlvo.Nome,
                    Emoji = NivelJogoService.EmojiPara(especieAlvo)
                };
            }

            return View(new JogoJogarViewModel
            {
                Nivel = nivel,
                DadosJson = dados.Serializar(),
                Animais = animaisMap,
                ModoTeste = false,
                EspecieAlvo = especieAlvo,
                EspecieAlvoEmoji = especieAlvo != null ? NivelJogoService.EmojiPara(especieAlvo) : "\U0001F43E"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Concluir(int vidasRestantes, int tempoSegundos, string? animaisIds, int? especieSalvarId)
        {
            var nivel = _nivelJogo.ObterNivelDoDia(DateTime.Today);
            if (nivel == null)
            {
                return RedirectToAction("Index");
            }

            var tentativa = ObterTentativaDoDia(nivel.Id);
            if (tentativa == null)
            {
                return RedirectToAction("Index");
            }

            var dados = NivelJogoDados.Desserializar(nivel.DadosJson);
            var totalAnimais = dados?.Animais.Count ?? 0;
            var idsNoNivel = dados?.Animais.Select(a => a.EspecieId).ToHashSet() ?? new HashSet<int>();

            var resgatados = (animaisIds ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out var id) ? id : 0)
                .Where(id => idsNoNivel.Contains(id))
                .Take(totalAnimais)
                .ToList();

            var tempoMaximo = Math.Max(0, (int)(DateTime.UtcNow - tentativa.IniciadoEm).TotalSeconds);
            var tempo = Math.Clamp(tempoSegundos, 0, tempoMaximo);
            var vidas = Math.Clamp(vidasRestantes, 0, VidasPorTentativa);

            var uid = UsuarioId();

            if (!tentativa.Concluida)
            {
                tentativa.Concluida = true;
                tentativa.ConcluidoEm = DateTime.UtcNow;
                tentativa.AnimaisResgatados = resgatados.Count;
                tentativa.TotalAnimais = totalAnimais;
                tentativa.TempoSegundos = tempo;
                tentativa.VidasRestantes = vidas;
                tentativa.AnimaisResgatadosIds = string.Join(',', resgatados.Distinct());
                _context.SaveChanges();

                TempData["Xp"] = NivelHelper.Conceder(
                    _context,
                    User,
                    NivelHelper.XpJogoDiarioBase + resgatados.Count * NivelHelper.XpPorAnimalResgatado);
            }
            else
            {
                var melhorAnimais = Math.Max(tentativa.AnimaisResgatados, resgatados.Count);
                var melhorTempo = tentativa.TempoSegundos <= 0 ? tempo : Math.Min(tentativa.TempoSegundos, tempo);
                var idsConhecidos = tentativa.AnimaisResgatadosIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => int.TryParse(s, out var id) ? id : 0)
                    .Union(resgatados)
                    .ToList();

                tentativa.AnimaisResgatados = melhorAnimais;
                tentativa.TempoSegundos = melhorTempo;
                tentativa.VidasRestantes = Math.Max(tentativa.VidasRestantes, vidas);
                tentativa.AnimaisResgatadosIds = string.Join(',', idsConhecidos);
                _context.SaveChanges();
            }

            // Reserva de avistamentos possíveis:
            // Ao salvar com sucesso a espécie escolhida no final do nível, ganha +1 avistamento dela!
            if (vidas > 0 && especieSalvarId.HasValue && especieSalvarId.Value > 0)
            {
                var especie = _context.Especies.Find(especieSalvarId.Value);
                if (especie != null)
                {
                    var reserva = _context.ReservasAvistamento
                        .FirstOrDefault(r => r.UsuarioId == uid && r.EspecieId == especie.Id);

                    if (reserva == null)
                    {
                        reserva = new ReservaAvistamento
                        {
                            UsuarioId = uid,
                            EspecieId = especie.Id,
                            QuantidadeDisponivel = 1
                        };
                        _context.ReservasAvistamento.Add(reserva);
                    }
                    else
                    {
                        reserva.QuantidadeDisponivel += 1;
                    }

                    _context.SaveChanges();

                    TempData["EspecieSalvaId"] = especie.Id;
                    TempData["GanhouAvistamento"] = true;
                    TempData["ReservaTotal"] = reserva.QuantidadeDisponivel;
                }
            }

            return RedirectToAction("Resultado");
        }

        public IActionResult Resultado()
        {
            var nivel = _nivelJogo.ObterNivelDoDia(DateTime.Today);
            if (nivel == null)
            {
                return RedirectToAction("Index");
            }

            var tentativa = ObterTentativaDoDia(nivel.Id);
            if (tentativa == null || !tentativa.Concluida)
            {
                return RedirectToAction("Index");
            }

            var ids = tentativa.AnimaisResgatadosIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s, out var id) ? id : 0)
                .Where(id => id > 0)
                .ToList();

            var especies = _context.Especies
                .Where(e => ids.Contains(e.Id))
                .OrderBy(e => e.Nome)
                .ToList();

            var uid = UsuarioId();
            Especie? especieSalva = null;
            var reservaTotal = 0;
            var ganhouAvistamento = TempData["GanhouAvistamento"] is bool g && g;

            if (TempData["EspecieSalvaId"] is int eid)
            {
                especieSalva = _context.Especies.Find(eid);
            }

            if (especieSalva == null && especies.Count > 0)
            {
                especieSalva = especies.First();
            }

            if (especieSalva != null)
            {
                reservaTotal = _context.ReservasAvistamento
                    .FirstOrDefault(r => r.UsuarioId == uid && r.EspecieId == especieSalva.Id)?.QuantidadeDisponivel ?? 0;
            }

            return View(new JogoResultadoViewModel
            {
                Nivel = nivel,
                Tentativa = tentativa,
                EspeciesResgatadas = especies,
                EspecieSalva = especieSalva,
                EspecieSalvaEmoji = especieSalva != null ? NivelJogoService.EmojiPara(especieSalva) : "\U0001F43E",
                ReservaTotalEspecie = reservaTotal,
                GanhouAvistamento = ganhouAvistamento
            });
        }

        private JogoTentativa? ObterTentativaDoDia(int nivelJogoId)
        {
            var usuarioId = UsuarioId();
            var hoje = DateTime.Today;

            return _context.JogoTentativas
                .FirstOrDefault(t => t.NivelJogoId == nivelJogoId && t.UsuarioId == usuarioId && t.Dia == hoje);
        }

        private int UsuarioId()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return id != null ? int.Parse(id) : 0;
        }
    }
}
