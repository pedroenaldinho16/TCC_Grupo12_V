using System;
using System.ComponentModel.DataAnnotations;

namespace TCC_Grupo12_III.Models
{
    public class Avistamento
    {
        public int Id { get; set; }
        public bool Aprovado { get; set; }

        public int? EspecieId { get; set; }
        public Especie? Especie { get; set; }

        [Required(ErrorMessage = "Informe o animal avistado")]
        [StringLength(100)]
        [Display(Name = "Animal avistado")]
        public string NomeAnimal { get; set; } = string.Empty;

        public string? FotoPath { get; set; }

        public List<AvistamentoMidia> Midias { get; set; } = new();

        [Required(ErrorMessage = "Marque no mapa o local do avistamento")]
        [Range(-90, 90, ErrorMessage = "Latitude inválida")]
        public double Latitude { get; set; }

        [Required(ErrorMessage = "Marque no mapa o local do avistamento")]
        [Range(-180, 180, ErrorMessage = "Longitude inválida")]
        public double Longitude { get; set; }

        [Required(ErrorMessage = "A data do avistamento é obrigatória")]
        [DataType(DataType.Date)]
        [Display(Name = "Data do avistamento")]
        public DateTime DataAvistamento { get; set; } = DateTime.Today;

        [Range(1, 500, ErrorMessage = "A quantidade deve estar entre 1 e 500")]
        [Display(Name = "Quantidade de indivíduos")]
        public int Quantidade { get; set; } = 1;

        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }

        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        [Required(ErrorMessage = "O nome do observador é obrigatório")]
        [StringLength(100)]
        [Display(Name = "Observador")]
        public string NomeObservador { get; set; } = string.Empty;

        public DateTime CriadoEm { get; set; } = DateTime.Now;
    }
}
