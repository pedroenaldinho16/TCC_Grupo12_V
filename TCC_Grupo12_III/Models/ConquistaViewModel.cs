using System;

namespace TCC_Grupo12_III.Models
{
    public class ConquistaViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Icone { get; set; } = "bi-award-fill";
        public string Cor { get; set; } = "success"; // "success", "primary", "warning", "info"
        public string Categoria { get; set; } = "Geral"; // "Exploração", "Comunidade", "Pesquisa", "Proteção"
        public int XpRecompensa { get; set; } = 20;

        public int ProgressoAtual { get; set; }
        public int Meta { get; set; }
        public bool Desbloqueada => ProgressoAtual >= Meta;
        public int Percentual => Meta <= 0 ? 100 : Math.Min(100, (int)Math.Round((double)ProgressoAtual / Meta * 100));
        public DateTime? DataDesbloqueio { get; set; }
    }
}
