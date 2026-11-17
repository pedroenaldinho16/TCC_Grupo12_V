using System;
using System.ComponentModel.DataAnnotations;

namespace TCC_Grupo12_III.Models
{
    public class NotaEstudo
    {
        public int Id { get; set; }

        public int? UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        [Required(ErrorMessage = "O título da anotação é obrigatório")]
        [StringLength(160, ErrorMessage = "O título deve ter no máximo 160 caracteres")]
        public string Titulo { get; set; } = "Minha Anotação de Estudo";

        public string ConteudoHtml { get; set; } = string.Empty;

        [StringLength(300)]
        public string? ResumoTexto { get; set; }

        [StringLength(100)]
        public string? ArtigoSlug { get; set; }

        [StringLength(200)]
        public string? ArtigoTitulo { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;
    }

    public class SalvarNotaEstudoDto
    {
        public int? Id { get; set; }
        public string? Titulo { get; set; }
        public string? ConteudoHtml { get; set; }
        public string? ArtigoSlug { get; set; }
        public string? ArtigoTitulo { get; set; }
    }
}
