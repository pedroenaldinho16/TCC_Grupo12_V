using System;
using System.Collections.Generic;
using System.Linq;

namespace TCC_Grupo12_III.Models
{
    public class PokedexRegistro
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public Usuario? Usuario { get; set; }

        public int EspecieId { get; set; }

        public Especie? Especie { get; set; }

        public DateTime VistaEm { get; set; } = DateTime.Now;

        public bool Dominada { get; set; }

        public bool QuizAprovado { get; set; }

        public bool EstudoLido { get; set; }

        public DateTime? DominadaEm { get; set; }
    }

    public class PokedexEntrada
    {
        public int Numero { get; set; }
        public Especie Especie { get; set; } = new();
        public bool Vista { get; set; }
        public bool Dominada { get; set; }
        public bool QuizAprovado { get; set; }
        public bool EstudoLido { get; set; }
    }

    public class PokedexViewModel
    {
        public List<PokedexEntrada> Entradas { get; set; } = new();
        public int Total => Entradas.Count;
        public int Descobertas => Entradas.Count(e => e.Vista);
        public int Dominadas => Entradas.Count(e => e.Dominada);
        public int Percentual => Total == 0 ? 0 : (int)Math.Round(Descobertas * 100.0 / Total);
    }
}
