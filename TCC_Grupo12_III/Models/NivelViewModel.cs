using TCC_Grupo12_III.Data;

namespace TCC_Grupo12_III.Models
{
    public class NivelViewModel
    {
        public int Xp { get; set; }
        public bool Admin { get; set; }
        public bool Bloqueado { get; set; }

        public int Nivel => NivelHelper.Nivel(Xp);
        public int XpNoNivel => NivelHelper.XpNoNivel(Xp);
        public int XpPorNivel => NivelHelper.XpPorNivel;
        public int XpParaProximo => XpPorNivel - XpNoNivel;

        // Desbloqueios por Nível
        public bool TopicoForumDesbloqueado => NivelHelper.PodeCriarTopico(Xp, Admin);
        public bool CampanhasDesbloqueadas => NivelHelper.PodeParticiparCampanha(Xp, Admin);
        public bool MapaDesbloqueado => NivelHelper.MapaDesbloqueado(Xp, Admin);
        public bool EstudosDesbloqueados => NivelHelper.PodeSubmeterEstudo(Xp, Admin);
        public bool LiderancaDesbloqueada => NivelHelper.PodeLiderar(Xp, Admin);

        public int XpFaltandoMapa => MapaDesbloqueado ? 0 : NivelHelper.XpParaMapa - Xp;
        public int PercentualMapa => NivelHelper.XpParaMapa == 0 ? 100 : System.Math.Min(100, Xp * 100 / NivelHelper.XpParaMapa);
    }
}
