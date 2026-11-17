using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TCC_Grupo12_III.Models
{
    public class ForumComunidade
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da comunidade é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Nome da Comunidade")]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        public string Slug { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição é obrigatória")]
        [StringLength(500, ErrorMessage = "A descrição deve ter no máximo 500 caracteres")]
        [Display(Name = "Descrição")]
        public string Descricao { get; set; } = string.Empty;

        [StringLength(600, ErrorMessage = "As regras devem ter no máximo 600 caracteres")]
        [Display(Name = "Regras e Diretrizes da Comunidade")]
        public string? Regras { get; set; }

        [StringLength(300)]
        public string? IconePath { get; set; }

        [StringLength(300)]
        public string? BannerPath { get; set; }

        public int? CriadorId { get; set; }
        public Usuario? Criador { get; set; }

        // Status: "Pendente", "Aprovada", "Rejeitada"
        [Required]
        [StringLength(30)]
        public string StatusAprovacao { get; set; } = "Pendente";

        public DateTime? AprovadoEm { get; set; }

        public int? AprovadoPorId { get; set; }

        [StringLength(500)]
        public string? MotivoRejeicao { get; set; }

        // Indica se é a comunidade padrão "Assuntos Gerais" gerenciada pelos administradores
        public bool IsGeral { get; set; } = false;

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        public List<ForumTopico> Topicos { get; set; } = new();

        public List<Campanha> Campanhas { get; set; } = new();
    }
}
