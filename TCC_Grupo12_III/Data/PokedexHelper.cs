using System;
using System.Linq;
using TCC_Grupo12_III.Models;

namespace TCC_Grupo12_III.Data
{
    public static class PokedexHelper
    {
        /// <summary>
        /// Registra a aprovação no Quiz da espécie (mínimo 70%).
        /// A cartinha só é conquistada se o estudo científico também tiver sido lido.
        /// </summary>
        public static (bool ConquistadaAgora, string Mensagem, int XpGanho) RegistrarQuizConcluido(AppDbContext context, int usuarioId, int especieId)
        {
            if (usuarioId <= 0)
            {
                return (false, string.Empty, 0);
            }

            var registro = context.PokedexRegistros
                .FirstOrDefault(r => r.UsuarioId == usuarioId && r.EspecieId == especieId);

            if (registro == null)
            {
                registro = new PokedexRegistro
                {
                    UsuarioId = usuarioId,
                    EspecieId = especieId,
                    VistaEm = DateTime.Now,
                    QuizAprovado = true,
                    EstudoLido = false,
                    Dominada = false
                };
                context.PokedexRegistros.Add(registro);
                context.SaveChanges();
                return (false, "Quiz concluído com sucesso! Agora leia ao menos 1 estudo científico da espécie para desbloquear a cartinha.", 0);
            }

            registro.QuizAprovado = true;

            if (registro.EstudoLido && !registro.Dominada)
            {
                registro.Dominada = true;
                registro.DominadaEm = DateTime.Now;
                context.SaveChanges();
                var xp = NivelHelper.XpDominarEspecie;
                return (true, $"🎉 Parabéns! Você completou o Quiz e já estudou a ficha científico. Cartinha conquistada na CantaDex! (+{xp} XP)", xp);
            }

            context.SaveChanges();
            if (registro.Dominada)
            {
                return (false, "Quiz concluído com sucesso!", 0);
            }

            return (false, "Quiz concluído com sucesso! Leia ao menos 1 estudo científico da espécie para liberar a cartinha na CantaDex.", 0);
        }

        /// <summary>
        /// Registra a leitura de um estudo científico vinculado à espécie.
        /// A cartinha só é conquistada se o quiz da espécie também tiver sido concluído.
        /// </summary>
        public static (bool ConquistadaAgora, string Mensagem, int XpGanho) RegistrarEstudoLido(AppDbContext context, int usuarioId, int especieId)
        {
            if (usuarioId <= 0)
            {
                return (false, string.Empty, 0);
            }

            var registro = context.PokedexRegistros
                .FirstOrDefault(r => r.UsuarioId == usuarioId && r.EspecieId == especieId);

            if (registro == null)
            {
                registro = new PokedexRegistro
                {
                    UsuarioId = usuarioId,
                    EspecieId = especieId,
                    VistaEm = DateTime.Now,
                    QuizAprovado = false,
                    EstudoLido = true,
                    Dominada = false
                };
                context.PokedexRegistros.Add(registro);
                context.SaveChanges();
                return (false, "Leitura de estudo científico registrada! Realize o quiz da espécie (mínimo 70%) para desbloquear a cartinha na CantaDex.", 0);
            }

            registro.EstudoLido = true;

            if (registro.QuizAprovado && !registro.Dominada)
            {
                registro.Dominada = true;
                registro.DominadaEm = DateTime.Now;
                context.SaveChanges();
                var xp = NivelHelper.XpDominarEspecie;
                return (true, $"🎉 Parabéns! Você estudou a ficha e já tinha completado o Quiz. Cartinha conquistada na CantaDex! (+{xp} XP)", xp);
            }

            context.SaveChanges();
            if (registro.Dominada)
            {
                return (false, "Ficha registrada como estudada!", 0);
            }

            return (false, "Leitura registrada! Falta apenas concluir o Quiz da espécie para liberar a cartinha na CantaDex.", 0);
        }

        public static (bool QuizAprovado, bool EstudoLido, bool Dominada) ObterStatus(AppDbContext context, int usuarioId, int especieId)
        {
            if (usuarioId <= 0) return (false, false, false);
            var r = context.PokedexRegistros.FirstOrDefault(x => x.UsuarioId == usuarioId && x.EspecieId == especieId);
            if (r == null) return (false, false, false);
            return (r.QuizAprovado, r.EstudoLido, r.Dominada);
        }

        public static PokedexViewModel Montar(AppDbContext context, int usuarioId)
        {
            var registros = context.PokedexRegistros
                .Where(r => r.UsuarioId == usuarioId)
                .ToDictionary(r => r.EspecieId);

            var entradas = context.Especies
                .OrderBy(e => e.Id)
                .ToList()
                .Select((e, i) =>
                {
                    registros.TryGetValue(e.Id, out var reg);
                    var dominada = reg?.Dominada == true;
                    return new PokedexEntrada
                    {
                        Numero = i + 1,
                        Especie = e,
                        Vista = dominada || reg?.EstudoLido == true || reg?.QuizAprovado == true,
                        Dominada = dominada,
                        QuizAprovado = reg?.QuizAprovado == true,
                        EstudoLido = reg?.EstudoLido == true
                    };
                })
                .ToList();

            return new PokedexViewModel { Entradas = entradas };
        }
    }
}
