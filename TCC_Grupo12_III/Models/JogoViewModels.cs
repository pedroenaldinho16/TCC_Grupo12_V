using System.Collections.Generic;

namespace TCC_Grupo12_III.Models
{
    public class JogoIndexViewModel
    {
        public NivelJogo Nivel { get; set; } = new();

        public JogoTentativa? Tentativa { get; set; }

        public int TotalAnimais { get; set; }

        public List<Especie> EspeciesDisponiveis { get; set; } = new();

        public Dictionary<int, int> ReservaPorEspecie { get; set; } = new();

        public int? EspecieSelecionadaId { get; set; }
    }

    public class JogoJogarViewModel
    {
        public NivelJogo Nivel { get; set; } = new();

        public string DadosJson { get; set; } = string.Empty;

        public Dictionary<int, JogoAnimalInfo> Animais { get; set; } = new();

        public bool ModoTeste { get; set; }

        public Especie? EspecieAlvo { get; set; }

        public string EspecieAlvoEmoji { get; set; } = "\U0001F43E";
    }

    public class JogoAnimalInfo
    {
        public string Nome { get; set; } = string.Empty;

        public string Emoji { get; set; } = "\U0001F43E";
    }

    public class JogoResultadoViewModel
    {
        public NivelJogo Nivel { get; set; } = new();

        public JogoTentativa Tentativa { get; set; } = new();

        public List<Especie> EspeciesResgatadas { get; set; } = new();

        public Especie? EspecieSalva { get; set; }

        public string EspecieSalvaEmoji { get; set; } = "\U0001F43E";

        public int ReservaTotalEspecie { get; set; }

        public bool GanhouAvistamento { get; set; }
    }
}
