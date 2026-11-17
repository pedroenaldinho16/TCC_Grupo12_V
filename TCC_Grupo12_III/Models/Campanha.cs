using System.ComponentModel.DataAnnotations;

namespace TCC_Grupo12_III.Models
{
    public class Campanha
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título é obrigatório")]
        [StringLength(120)]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição é obrigatória")]
        public string Descricao { get; set; } = string.Empty;

        [Required(ErrorMessage = "O período é obrigatório")]
        [Display(Name = "Período (ex: Jan – Dez 2026)")]
        public string Periodo { get; set; } = string.Empty;

        [Display(Name = "Participantes")]
        public int Participantes { get; set; } = 0;

        [Display(Name = "Meta de participantes")]
        public int Meta { get; set; } = 100;

        [Display(Name = "Campanha ativa")]
        public bool Ativa { get; set; } = true;

        [Display(Name = "Imagem")]
        public string? ImagemUrl { get; set; }

        [Display(Name = "Objetivo principal")]
        public string? ObjetivoPrincipal { get; set; }

        [Display(Name = "Objetivos específicos")]
        public string? ObjetivosEspecificos { get; set; }

        [Display(Name = "Data de início / realização")]
        [DataType(DataType.Date)]
        public DateTime? DataInicio { get; set; }

        [Display(Name = "Data de término")]
        [DataType(DataType.Date)]
        public DateTime? DataFim { get; set; }

        [Display(Name = "Horário das atividades")]
        [StringLength(100)]
        public string? Horario { get; set; }

        [Display(Name = "Ponto de encontro / Localização")]
        [StringLength(200)]
        public string? LocalEncontro { get; set; }

        [Display(Name = "Público-alvo")]
        [StringLength(150)]
        public string? PublicoAlvo { get; set; }

        [Display(Name = "O que levar / Requisitos")]
        public string? OQueLevar { get; set; }

        [Display(Name = "Cronograma de etapas")]
        public string? Cronograma { get; set; }

        [Display(Name = "Impacto ecológico esperado")]
        public string? ImpactoEsperado { get; set; }

        [Display(Name = "Organização / Responsável")]
        [StringLength(150)]
        public string? Organizador { get; set; }

        // Auditoria e moderação
        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        [Display(Name = "Comunidade do Fórum Vinculada")]
        public int? ComunidadeId { get; set; }
        public ForumComunidade? Comunidade { get; set; }

        [Display(Name = "Status de aprovação")]
        [StringLength(20)]
        public string StatusAprovacao { get; set; } = "Aprovado";

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        public DateTime? DataResposta { get; set; }

        [StringLength(100)]
        public string? AprovadoPorNome { get; set; }

        [StringLength(500)]
        public string? MotivoReprovacao { get; set; }
    }
}
