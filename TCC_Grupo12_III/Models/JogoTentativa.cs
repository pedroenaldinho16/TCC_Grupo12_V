using System;

namespace TCC_Grupo12_III.Models
{
    public class JogoTentativa
    {
        public int Id { get; set; }

        public int NivelJogoId { get; set; }
        public NivelJogo Nivel { get; set; } = null!;

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public DateTime Dia { get; set; }

        public DateTime IniciadoEm { get; set; }

        public DateTime? ConcluidoEm { get; set; }

        public bool Concluida { get; set; }

        public int AnimaisResgatados { get; set; }

        public int TotalAnimais { get; set; }

        public int TempoSegundos { get; set; }

        public int VidasRestantes { get; set; }

        public string AnimaisResgatadosIds { get; set; } = string.Empty;
    }
}
