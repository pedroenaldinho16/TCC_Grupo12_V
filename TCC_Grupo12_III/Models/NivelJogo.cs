using System;
using System.ComponentModel.DataAnnotations;

namespace TCC_Grupo12_III.Models
{
    public class NivelJogo
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título é obrigatório")]
        [StringLength(120)]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição é obrigatória")]
        public string Descricao { get; set; } = string.Empty;

        [Display(Name = "Data do nível")]
        [DataType(DataType.Date)]
        public DateTime Data { get; set; } = DateTime.Today;

        [Display(Name = "Nível ativo")]
        public bool Ativo { get; set; } = true;

        public string DadosJson { get; set; } = string.Empty;

        public int TotalAnimais { get; set; }
    }
}
