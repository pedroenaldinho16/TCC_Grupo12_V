using System;
using System.Collections.Generic;
using System.Linq;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;

namespace TCC_Grupo12_III.Services
{
    public static class ConquistasService
    {
        public static List<ConquistaViewModel> ObterConquistas(AppDbContext context, int usuarioId)
        {
            var usuario = context.Usuarios.Find(usuarioId);
            if (usuario == null)
            {
                return new List<ConquistaViewModel>();
            }

            // Estatísticas reais do usuário no sistema
            var totalEspecies = context.PokedexRegistros.Count(r => r.UsuarioId == usuarioId);
            var totalAvistamentos = context.Avistamentos.Count(a => a.UsuarioId == usuarioId);
            var totalTopicos = context.ForumTopicos.Count(t => t.UsuarioId == usuarioId);
            var totalRespostas = context.ForumRespostas.Count(r => r.UsuarioId == usuarioId);
            var totalDenuncias = context.Denuncias.Count(d => d.EmailContato == usuario.Email || (!string.IsNullOrEmpty(d.NomeContato) && d.NomeContato == usuario.Nome));
            var totalCampanhas = context.CampanhaParticipacoes.Count(p => p.UsuarioId == usuarioId);
            var totalEstudos = context.EstudosCientificos.Count(e => e.UsuarioId == usuarioId && e.StatusAprovacao == "Aprovado");
            var totalJogosConcluidos = context.JogoTentativas.Count(t => t.UsuarioId == usuarioId && t.Concluida);
            var totalComunidadesCriadas = context.ForumComunidades.Count(c => c.CriadorId == usuarioId && c.StatusAprovacao == "Aprovada");
            var isJornalista = usuario.IsJornalista && usuario.StatusJornalista == "Aprovado";
            var nivel = NivelHelper.Nivel(usuario.Xp);

            return new List<ConquistaViewModel>
            {
                new ConquistaViewModel
                {
                    Id = "primeiros_passos",
                    Titulo = "Primeiros Passos",
                    Descricao = "Cadastre-se e complete seu perfil no EcoCanta Cantareira.",
                    Icone = "bi-person-check-fill",
                    Cor = "success",
                    Categoria = "Geral",
                    XpRecompensa = 20,
                    ProgressoAtual = 1,
                    Meta = 1
                },
                new ConquistaViewModel
                {
                    Id = "explorador_iniciante",
                    Titulo = "Olhos da Cantareira",
                    Descricao = "Registre seu primeiro avistamento de fauna silvestre no mapa.",
                    Icone = "bi-binoculars-fill",
                    Cor = "success",
                    Categoria = "Exploração",
                    XpRecompensa = 30,
                    ProgressoAtual = Math.Min(1, totalAvistamentos),
                    Meta = 1
                },
                new ConquistaViewModel
                {
                    Id = "guardiao_fauna",
                    Titulo = "Guardião da Fauna",
                    Descricao = "Registre 5 ou mais avistamentos comprovados no mapa interativo.",
                    Icone = "bi-camera-fill",
                    Cor = "success",
                    Categoria = "Exploração",
                    XpRecompensa = 50,
                    ProgressoAtual = Math.Min(5, totalAvistamentos),
                    Meta = 5
                },
                new ConquistaViewModel
                {
                    Id = "mestre_cantadex",
                    Titulo = "Curioso da CantaDex",
                    Descricao = "Descubra e registre pelo menos 5 espécies diferentes na sua CantaDex.",
                    Icone = "bi-journal-bookmark-fill",
                    Cor = "info",
                    Categoria = "Exploração",
                    XpRecompensa = 40,
                    ProgressoAtual = Math.Min(5, totalEspecies),
                    Meta = 5
                },
                new ConquistaViewModel
                {
                    Id = "socorrista_diario",
                    Titulo = "Socorrista da Mata",
                    Descricao = "Conclua o desafio ecológico no Jogo Diário por 3 dias.",
                    Icone = "bi-controller",
                    Cor = "warning",
                    Categoria = "Exploração",
                    XpRecompensa = 35,
                    ProgressoAtual = Math.Min(3, totalJogosConcluidos),
                    Meta = 3
                },
                new ConquistaViewModel
                {
                    Id = "voz_comunidade",
                    Titulo = "Voz da Comunidade",
                    Descricao = "Inicie seu primeiro tópico de debate ou notícia no Fórum da Cantareira.",
                    Icone = "bi-chat-left-dots-fill",
                    Cor = "primary",
                    Categoria = "Comunidade",
                    XpRecompensa = 25,
                    ProgressoAtual = Math.Min(1, totalTopicos),
                    Meta = 1
                },
                new ConquistaViewModel
                {
                    Id = "debatedor_ativo",
                    Titulo = "Debatedor Consciente",
                    Descricao = "Participe com pelo menos 5 respostas e comentários construtivos no fórum.",
                    Icone = "bi-chat-square-quote-fill",
                    Cor = "primary",
                    Categoria = "Comunidade",
                    XpRecompensa = 35,
                    ProgressoAtual = Math.Min(5, totalRespostas),
                    Meta = 5
                },
                new ConquistaViewModel
                {
                    Id = "lider_comunidade",
                    Titulo = "Fundador de Comunidade",
                    Descricao = "Tenha uma comunidade temática aprovada pela moderação no fórum.",
                    Icone = "bi-people-fill",
                    Cor = "primary",
                    Categoria = "Comunidade",
                    XpRecompensa = 60,
                    ProgressoAtual = Math.Min(1, totalComunidadesCriadas),
                    Meta = 1
                },
                new ConquistaViewModel
                {
                    Id = "engajamento_ecologico",
                    Titulo = "Mão na Massa",
                    Descricao = "Inscreva-se ou participe ativamente de uma campanha ecológica de campo.",
                    Icone = "bi-flag-fill",
                    Cor = "success",
                    Categoria = "Comunidade",
                    XpRecompensa = 40,
                    ProgressoAtual = Math.Min(1, totalCampanhas),
                    Meta = 1
                },
                new ConquistaViewModel
                {
                    Id = "olho_clinico",
                    Titulo = "Sentinela Ambiental",
                    Descricao = "Envie uma denúncia ambiental responsável para proteção da reserva.",
                    Icone = "bi-shield-fill-exclamation",
                    Cor = "danger",
                    Categoria = "Proteção",
                    XpRecompensa = 45,
                    ProgressoAtual = Math.Min(1, totalDenuncias),
                    Meta = 1
                },
                new ConquistaViewModel
                {
                    Id = "cientista_cidadao",
                    Titulo = "Cientista Cidadão",
                    Descricao = "Publique seu primeiro artigo ou estudo científico aprovado na plataforma.",
                    Icone = "bi-mortarboard-fill",
                    Cor = "info",
                    Categoria = "Pesquisa",
                    XpRecompensa = 50,
                    ProgressoAtual = Math.Min(1, totalEstudos),
                    Meta = 1
                },
                new ConquistaViewModel
                {
                    Id = "pesquisador_avancado",
                    Titulo = "Pesquisador Sênior",
                    Descricao = "Tenha 10 ou mais estudos científicos aprovados no acervo do EcoCanta.",
                    Icone = "bi-award-fill",
                    Cor = "info",
                    Categoria = "Pesquisa",
                    XpRecompensa = 100,
                    ProgressoAtual = Math.Min(10, totalEstudos),
                    Meta = 10
                },
                new ConquistaViewModel
                {
                    Id = "imprensa_verde",
                    Titulo = "Imprensa Verde",
                    Descricao = "Obtenha a credencial oficial de Jornalista / Imprensa ambiental aprovada.",
                    Icone = "bi-newspaper",
                    Cor = "secondary",
                    Categoria = "Pesquisa",
                    XpRecompensa = 80,
                    ProgressoAtual = isJornalista ? 1 : 0,
                    Meta = 1
                },
                new ConquistaViewModel
                {
                    Id = "lider_ecocanta",
                    Titulo = "Líder da Cantareira",
                    Descricao = "Alcance o Nível 5 (400 XP) acumulando conhecimento e ações práticas.",
                    Icone = "bi-gem",
                    Cor = "warning",
                    Categoria = "Geral",
                    XpRecompensa = 100,
                    ProgressoAtual = Math.Min(5, nivel),
                    Meta = 5
                }
            };
        }
    }
}
