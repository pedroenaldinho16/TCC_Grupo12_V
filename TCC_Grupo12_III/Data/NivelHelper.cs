using System.Linq;
using System.Security.Claims;
using TCC_Grupo12_III.Models;

namespace TCC_Grupo12_III.Data
{
    public static class NivelHelper
    {
        public const int XpPorNivel = 250;

        // Limiares de Nível Justos e Balanceados
        public const int NivelBasico = 1;              // 0 a 249 XP: CantaDex, Jogo Diário, Leitura do Fórum, Respostas, FAQ, Denúncias
        public const int NivelForumTopico = 2;         // 250 XP: Criar Novos Tópicos no Fórum e Participar de Campanhas
        public const int NivelCampanhas = 2;           // 250 XP: Inscrição e Pré-inscrição em Campanhas
        public const int NivelMapa = 3;                // 500 XP: Registro de Avistamentos Comunitário
        public const int NivelEstudos = 4;             // 750 XP: Caderno de Campo & Submissão de Estudos Científicos
        public const int NivelLideranca = 5;           // 1000 XP: Propor Novas Comunidades & Campanhas

        public const int XpDescobrirEspecie = 10;
        public const int XpDominarEspecie = 35;
        public const int XpJogoDiarioBase = 15;
        public const int XpPorAnimalResgatado = 10;
        public const int XpTopicoForum = 15;
        public const int XpRespostaForum = 5;
        public const int XpAvistamento = 25;
        public const int XpDenuncia = 20;
        public const int XpCampanhaParticipacao = 30;
        public const int XpEstudoAprovado = 50;

        public static int Nivel(int xp) => (xp / XpPorNivel) + 1;

        public static int XpNoNivel(int xp) => xp % XpPorNivel;

        public static int XpNecessarioParaNivel(int nivel) => (nivel - 1) * XpPorNivel;

        public static int XpParaMapa => XpNecessarioParaNivel(NivelMapa);

        public static bool PodeCriarTopico(int xp, bool isPrivilegiado) => isPrivilegiado || Nivel(xp) >= NivelForumTopico;

        public static bool PodeParticiparCampanha(int xp, bool isPrivilegiado) => isPrivilegiado || Nivel(xp) >= NivelCampanhas;

        // Compatibilidade histórica: XP não concede acesso ao mapa.
        public static bool MapaDesbloqueado(int xp, bool admin) => admin;

        public static bool PodeSubmeterEstudo(int xp, bool isPrivilegiado) => isPrivilegiado || Nivel(xp) >= NivelEstudos;

        public static bool PodeLiderar(int xp, bool isPrivilegiado) => isPrivilegiado || Nivel(xp) >= NivelLideranca;

        public static int ObterXp(AppDbContext context, ClaimsPrincipal user)
        {
            var id = IdDoUsuario(user);
            return id <= 0 ? 0 : context.Usuarios.Where(u => u.Id == id).Select(u => u.Xp).FirstOrDefault();
        }

        public static string Conceder(AppDbContext context, ClaimsPrincipal user, int xp)
        {
            var id = IdDoUsuario(user);
            if (id <= 0 || xp <= 0)
            {
                return string.Empty;
            }

            var usuario = context.Usuarios.Find(id);
            if (usuario == null)
            {
                return string.Empty;
            }

            var nivelAntes = Nivel(usuario.Xp);
            usuario.Xp += xp;
            context.SaveChanges();

            var nivelDepois = Nivel(usuario.Xp);
            var mensagem = $"+{xp} XP";

            if (nivelDepois > nivelAntes)
            {
                mensagem += $" · Você subiu para o nível {nivelDepois}!";
            }

            return mensagem;
        }

        private static int IdDoUsuario(ClaimsPrincipal user)
        {
            return int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
        }
    }
}
