using System;
using System.ComponentModel.DataAnnotations;

namespace TCC_Grupo12_III.Models
{
    public class Denuncia
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Selecione o tipo de denúncia")]
        [Display(Name = "Tipo de Denúncia")]
        public string Tipo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A localização é obrigatória")]
        public string Localizacao { get; set; } = string.Empty;

        [Display(Name = "Data da Ocorrência")]
        [DataType(DataType.Date)]
        public DateTime? DataOcorrencia { get; set; }

        [Required(ErrorMessage = "A descrição é obrigatória")]
        [MinLength(50, ErrorMessage = "A descrição deve ter no mínimo 50 caracteres")]
        public string Descricao { get; set; } = string.Empty;

        public string? FotoPath { get; set; }

        [Display(Name = "Nome")]
        public string? NomeContato { get; set; }

        [EmailAddress(ErrorMessage = "E-mail inválido")]
        [Display(Name = "E-mail")]
        public string? EmailContato { get; set; }

        [Display(Name = "Quero fazer denúncia anônima")]
        public bool Anonima { get; set; }

        public bool Resolvida { get; set; } = false;

        public string? ObservacaoAdmin { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.Now;
    }
}
