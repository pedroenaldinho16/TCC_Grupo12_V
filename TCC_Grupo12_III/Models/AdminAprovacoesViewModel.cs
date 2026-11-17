using System.Collections.Generic;

namespace TCC_Grupo12_III.Models
{
    public class AdminAprovacoesViewModel
    {
        public List<Denuncia> Denuncias { get; set; } = new();
        public List<Usuario> Jornalistas { get; set; } = new();
        public string AbaAtiva { get; set; } = "denuncias";

        // Filtros e contagens de Denúncias
        public string TipoDenuncia { get; set; } = "Todos";
        public string StatusDenuncia { get; set; } = "todos";
        public string BuscaDenuncia { get; set; } = string.Empty;
        public int TotalDenuncias { get; set; }
        public int TotalDenunciasPendentes { get; set; }
        public int TotalDenunciasResolvidas { get; set; }
        public List<string> TiposDisponiveis { get; set; } = new();

        // Filtros e contagens de Credenciais de Jornalistas
        public string StatusJornalistaFiltro { get; set; } = "todos";
        public string BuscaJornalista { get; set; } = string.Empty;
        public int TotalJornalistas { get; set; }
        public int TotalJornalistasPendentes { get; set; }
        public int TotalJornalistasAprovados { get; set; }
        public int TotalJornalistasReprovados { get; set; }

        // Filtros e contagens de Estudos Científicos
        public List<EstudoCientifico> Estudos { get; set; } = new();
        public string StatusEstudoFiltro { get; set; } = "todos";
        public string BuscaEstudo { get; set; } = string.Empty;
        public int TotalEstudos { get; set; }
        public int TotalEstudosPendentes { get; set; }
        public int TotalEstudosAprovados { get; set; }
        public int TotalEstudosReprovados { get; set; }

        // Filtros e contagens de Propostas de Campanhas
        public List<Campanha> Campanhas { get; set; } = new();
        public string StatusCampanhaFiltro { get; set; } = "todos";
        public string BuscaCampanha { get; set; } = string.Empty;
        public int TotalCampanhas { get; set; }
        public int TotalCampanhasPendentes { get; set; }
        public int TotalCampanhasAprovadas { get; set; }
        public int TotalCampanhasReprovadas { get; set; }

        // Filtros e contagens de Propostas de Comunidades do Fórum
        public List<ForumComunidade> Comunidades { get; set; } = new();
        public string StatusComunidadeFiltro { get; set; } = "todos";
        public string BuscaComunidade { get; set; } = string.Empty;
        public int TotalComunidades { get; set; }
        public int TotalComunidadesPendentes { get; set; }
        public int TotalComunidadesAprovadas { get; set; }
        public int TotalComunidadesReprovadas { get; set; }
    }
}
