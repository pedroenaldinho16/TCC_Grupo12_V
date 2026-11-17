using System.ComponentModel.DataAnnotations;

namespace TCC_Grupo12_III.Models
{
    public class ReservaAvistamento
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;

        public int EspecieId { get; set; }
        public Especie Especie { get; set; } = null!;

        [Range(0, int.MaxValue)]
        public int QuantidadeDisponivel { get; set; } = 0;
    }
}
