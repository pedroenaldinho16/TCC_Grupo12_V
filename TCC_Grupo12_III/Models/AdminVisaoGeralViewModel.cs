using System.Collections.Generic;

namespace TCC_Grupo12_III.Models
{
    public class AdminVisaoGeralViewModel
    {
        public int TotalPendencias => DenunciasPendentes + CampanhasPendentes + JornalistasPendentes + EstudosPendentes + ComunidadesPendentes + FaqAbertos;

        public int DenunciasPendentes { get; set; }
        public int CampanhasPendentes { get; set; }
        public int JornalistasPendentes { get; set; }
        public int EstudosPendentes { get; set; }
        public int ComunidadesPendentes { get; set; }
        public int FaqAbertos { get; set; }

        public int UsuariosSuspensos { get; set; }
        public int UsuariosBanidos { get; set; }
        public int TotalModeradores { get; set; }
        public int TotalContas { get; set; }

        public List<Denuncia> ListaDenunciasPendentes { get; set; } = new();
        public List<Campanha> ListaCampanhasPendentes { get; set; } = new();
        public List<Usuario> ListaJornalistasPendentes { get; set; } = new();
        public List<EstudoCientifico> ListaEstudosPendentes { get; set; } = new();
        public List<ForumComunidade> ListaComunidadesPendentes { get; set; } = new();
        public List<FaqTicket> ListaFaqPendentes { get; set; } = new();
    }
}
