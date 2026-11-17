using System;
using System.Collections.Generic;
using System.Linq;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;

namespace TCC_Grupo12_III.Services
{
    public class NivelJogoService
    {
        private const int MaxPlataformas = 800;
        private const int MaxPerigos = 400;
        private const int MaxAnimais = 100;

        private readonly AppDbContext _context;

        public NivelJogoService(AppDbContext context)
        {
            _context = context;
        }

        public NivelJogo? ObterNivelDoDia(DateTime dia)
        {
            var data = dia.Date;

            var disponiveis = _context.NivelJogos
                .Where(n => n.Ativo)
                .ToList()
                .Where(n => NivelJogoDados.Desserializar(n.DadosJson) is { Plataformas.Count: > 0 } d
                    && d.Inicio != null && d.Objetivo != null)
                .ToList();

            var doDia = disponiveis.FirstOrDefault(n => n.Data.Date == data);
            if (doDia != null)
            {
                return doDia;
            }

            var anteriores = disponiveis
                .Where(n => n.Data.Date <= data)
                .OrderBy(n => n.Data)
                .ThenBy(n => n.Id)
                .ToList();

            if (anteriores.Count == 0)
            {
                return null;
            }

            var indice = (int)(data.ToOADate() % anteriores.Count);
            return anteriores[indice];
        }

        public Dictionary<int, JogoAnimalInfo> MontarInfoAnimais(NivelJogoDados? dados)
        {
            var info = new Dictionary<int, JogoAnimalInfo>();
            if (dados == null)
            {
                return info;
            }

            var ids = dados.Animais.Select(a => a.EspecieId).Distinct().ToList();
            var especies = _context.Especies.Where(e => ids.Contains(e.Id)).ToList();

            foreach (var especie in especies)
            {
                info[especie.Id] = new JogoAnimalInfo
                {
                    Nome = especie.Nome,
                    Emoji = EmojiPara(especie)
                };
            }

            return info;
        }

        public static NivelJogoDados NivelVazio()
        {
            var dados = new NivelJogoDados { Largura = 120, Altura = NivelJogoDados.AlturaPadrao };
            dados.Plataformas.Add(new NivelPlataforma { X = 0, Y = dados.Altura - 2, W = dados.Largura, H = 2, Tipo = "terra" });
            dados.Inicio = new NivelPonto { X = 2, Y = dados.Altura - 3 };
            dados.Objetivo = new NivelPonto { X = dados.Largura - 4, Y = dados.Altura - 3 };
            return dados;
        }

        public static bool Validar(string? json, HashSet<int> especiesValidas, out NivelJogoDados? dados, out string erro)
        {
            dados = NivelJogoDados.Desserializar(json);
            erro = string.Empty;

            if (dados == null)
            {
                erro = "Os dados do nível estão inválidos. Redesenhe o nível no editor.";
                return false;
            }

            dados.Largura = Math.Clamp(dados.Largura, NivelJogoDados.LarguraMinima, NivelJogoDados.LarguraMaxima);
            dados.Altura = Math.Clamp(dados.Altura <= 0 ? NivelJogoDados.AlturaPadrao : dados.Altura, NivelJogoDados.AlturaMinima, NivelJogoDados.AlturaMaxima);

            var largura = dados.Largura;
            var altura = dados.Altura;

            dados.Plataformas = NormalizarRetangulos(dados.Plataformas, largura, altura, MaxPlataformas);
            dados.Perigos = NormalizarRetangulos(dados.Perigos, largura, altura, MaxPerigos);

            dados.Animais = dados.Animais
                .Where(a => a.X >= 0 && a.X < largura && a.Y >= 0 && a.Y < altura)
                .Where(a => especiesValidas.Contains(a.EspecieId))
                .GroupBy(a => (a.X, a.Y))
                .Select(g => g.First())
                .Take(MaxAnimais)
                .ToList();

            dados.PlataformasMoveis = dados.PlataformasMoveis
                .Where(p => p.X1 >= 0 && p.X1 < largura && p.Y1 >= 0 && p.Y1 < altura
                         && p.X2 >= 0 && p.X2 < largura && p.Y2 >= 0 && p.Y2 < altura)
                .Take(50)
                .Select(p => { p.Velocidade = Math.Clamp(p.Velocidade <= 0 ? 1.0 : p.Velocidade, 0.2, 5.0); return p; })
                .ToList();

            dados.PerigosMoveis = dados.PerigosMoveis
                .Where(p => p.X1 >= 0 && p.X1 < largura && p.Y1 >= 0 && p.Y1 < altura
                         && p.X2 >= 0 && p.X2 < largura && p.Y2 >= 0 && p.Y2 < altura)
                .Take(50)
                .Select(p => { p.Velocidade = Math.Clamp(p.Velocidade <= 0 ? 1.0 : p.Velocidade, 0.2, 5.0); return p; })
                .ToList();

            dados.Itens = dados.Itens
                .Where(i => i.X >= 0 && i.X < largura && i.Y >= 0 && i.Y < altura)
                .Take(50)
                .ToList();

            dados.Checkpoints = (dados.Checkpoints ?? new())
                .Where(c => c.X >= 0 && c.X < largura && c.Y >= 0 && c.Y < altura)
                .Take(20)
                .ToList();

            if (dados.Plataformas.Count == 0 && dados.PlataformasMoveis.Count == 0)
            {
                erro = "O nível precisa de pelo menos uma plataforma.";
                return false;
            }

            if (dados.Inicio == null)
            {
                erro = "Marque o ponto de início do jogador no editor.";
                return false;
            }

            if (dados.Objetivo == null)
            {
                erro = "Marque o objetivo (base de resgate) no editor.";
                return false;
            }

            dados.Inicio = LimitarPonto(dados.Inicio, largura, altura);
            dados.Objetivo = LimitarPonto(dados.Objetivo, largura, altura);

            // Validação da regra: A água não pode estar diretamente em cima da terra
            var terraOcupada = new HashSet<(int X, int Y)>();
            foreach (var plat in dados.Plataformas.Where(p => p.Tipo == "terra"))
            {
                for (int ty = plat.Y; ty < plat.Y + plat.H; ty++)
                {
                    for (int tx = plat.X; tx < plat.X + plat.W; tx++)
                    {
                        terraOcupada.Add((tx, ty));
                    }
                }
            }

            foreach (var perigo in dados.Perigos.Where(p => p.Tipo == "agua"))
            {
                for (int ty = perigo.Y; ty < perigo.Y + perigo.H; ty++)
                {
                    for (int tx = perigo.X; tx < perigo.X + perigo.W; tx++)
                    {
                        if (terraOcupada.Contains((tx, ty + 1)))
                        {
                            erro = $"A água não pode estar diretamente em cima da terra (posição X: {tx}, Y: {ty}). Remova os blocos de terra diretamente abaixo da água para criar um leito de lago/rio.";
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public static string EmojiPara(Especie especie)
        {
            var nome = especie.Nome.ToLowerInvariant();

            if (nome.Contains("bugio") || nome.Contains("macaco"))
            {
                return "\U0001F435";
            }

            if (nome.Contains("papagaio") || nome.Contains("tucano"))
            {
                return "\U0001F99C";
            }

            if (nome.Contains("onça") || nome.Contains("jaguatirica") || nome.Contains("gato"))
            {
                return "\U0001F406";
            }

            if (nome.Contains("cateto") || nome.Contains("porco"))
            {
                return "\U0001F417";
            }

            if (nome.Contains("saíra") || nome.Contains("saira") || nome.Contains("beija-flor"))
            {
                return "\U0001F426";
            }

            if (nome.Contains("tamanduá") || nome.Contains("tamandua"))
            {
                return "\U0001F9A1";
            }

            if (nome.Contains("cobra") || nome.Contains("jararaca"))
            {
                return "\U0001F40D";
            }

            return especie.Categoria switch
            {
                "Aves" => "\U0001F426",
                "Répteis" => "\U0001F98E",
                "Anfíbios" => "\U0001F438",
                "Peixes" => "\U0001F41F",
                _ => "\U0001F43E"
            };
        }

        private static List<T> NormalizarRetangulos<T>(List<T> itens, int largura, int altura, int maximo) where T : class
        {
            var resultado = new List<T>();

            foreach (var item in itens)
            {
                if (item is not INivelRetangulo ret)
                {
                    continue;
                }

                if (ret.W < 1 || ret.H < 1 || ret.X < 0 || ret.Y < 0 || ret.X >= largura || ret.Y >= altura)
                {
                    continue;
                }

                ret.W = Math.Min(ret.W, largura - ret.X);
                ret.H = Math.Min(ret.H, altura - ret.Y);
                resultado.Add(item);

                if (resultado.Count >= maximo)
                {
                    break;
                }
            }

            return resultado;
        }

        private static NivelPonto LimitarPonto(NivelPonto ponto, int largura, int altura)
        {
            return new NivelPonto
            {
                X = Math.Clamp(ponto.X, 0, largura - 1),
                Y = Math.Clamp(ponto.Y, 0, altura - 1)
            };
        }
    }
}
