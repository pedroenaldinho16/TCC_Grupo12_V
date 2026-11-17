using System.ComponentModel.DataAnnotations;

namespace TCC_Grupo12_III.Models
{
    public class Especie
    {
        public int Id { get; set; }
        public string ImportanciaEcologica { get; set; } = "";
        public string AcoesPreservacao { get; set; } = "";
        public string FonteCientifica { get; set; } = "";
        public string ConservacaoGlobal { get; set; } = "";
        public string ConservacaoNacional { get; set; } = "";
        public string ConservacaoRegional { get; set; } = "";

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O nome científico é obrigatório")]
        [StringLength(120)]
        [Display(Name = "Nome científico")]
        public string NomeCientifico { get; set; } = string.Empty;

        [Required(ErrorMessage = "A categoria é obrigatória")]
        public string Categoria { get; set; } = "Mamíferos";

        [Required(ErrorMessage = "O status de conservação é obrigatório")]
        [Display(Name = "Status de conservação")]
        public string StatusConservacao { get; set; } = "Vulnerável";

        [Display(Name = "Tags (separadas por vírgula)")]
        public string Tags { get; set; } = string.Empty;

        [Required(ErrorMessage = "O habitat é obrigatório")]
        public string Habitat { get; set; } = string.Empty;

        [Required(ErrorMessage = "A alimentação é obrigatória")]
        public string Alimentacao { get; set; } = string.Empty;

        [Display(Name = "Principais ameaças (uma por linha)")]
        public string Ameacas { get; set; } = string.Empty;

        public string Caracteristicas { get; set; } = string.Empty;

        public string Comportamento { get; set; } = string.Empty;

        [Display(Name = "Texto do status de conservação")]
        public string StatusDetalhe { get; set; } = string.Empty;

        [Display(Name = "Imagem")]
        public string? ImagemUrl { get; set; }
    }
}
