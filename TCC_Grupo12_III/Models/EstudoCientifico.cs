using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TCC_Grupo12_III.Models
{
    public class EstudoCientifico
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        [Required(ErrorMessage = "O título é obrigatório")]
        [StringLength(200, ErrorMessage = "O título pode ter no máximo 200 caracteres")]
        [Display(Name = "Título da pesquisa")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O subtítulo é obrigatório")]
        [StringLength(300, ErrorMessage = "O subtítulo pode ter no máximo 300 caracteres")]
        [Display(Name = "Subtítulo explicativo")]
        public string Subtitulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A categoria é obrigatória")]
        [StringLength(50)]
        [Display(Name = "Categoria biológica")]
        public string Categoria { get; set; } = "Mamíferos";

        [Required(ErrorMessage = "O resumo executivo é obrigatório")]
        [Display(Name = "Resumo executivo / Abstract")]
        public string Resumo { get; set; } = string.Empty;

        [Required(ErrorMessage = "O conteúdo completo do estudo é obrigatório")]
        [Display(Name = "Conteúdo completo da pesquisa")]
        public string ConteudoHtml { get; set; } = string.Empty;

        [StringLength(150)]
        public string Slug { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Nome da espécie em foco (opcional)")]
        public string? EspecieNome { get; set; }

        public int? EspecieId { get; set; }
        public Especie? Especie { get; set; }

        [StringLength(60)]
        [Display(Name = "Status de conservação")]
        public string? StatusConservacao { get; set; }

        [Required(ErrorMessage = "O nome do autor é obrigatório")]
        [StringLength(120)]
        [Display(Name = "Nome do autor / pesquisador")]
        public string Autor { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Instituição / Laboratório")]
        public string? Instituicao { get; set; }

        [Display(Name = "URL ou caminho da imagem ilustrativa")]
        public string? ImagemUrl { get; set; }

        [Display(Name = "Tempo estimado de leitura (minutos)")]
        public int TempoLeituraMinutos { get; set; } = 5;

        // Linhas de texto separadas por \n
        [Display(Name = "Destaques / Principais achados (um por linha)")]
        public string? DestaquesTexto { get; set; }

        [Display(Name = "Ameaças identificadas (uma por linha)")]
        public string? AmeacasTexto { get; set; }

        [Display(Name = "Ações de conservação recomendadas (uma por linha)")]
        public string? AcoesTexto { get; set; }

        [Display(Name = "Referências bibliográficas (uma por linha)")]
        public string? ReferenciasTexto { get; set; }

        // Moderação / Aprovação administrativa: "Pendente", "Aprovado", "Reprovado"
        [StringLength(30)]
        public string StatusAprovacao { get; set; } = "Pendente";

        public DateTime DataSubmissao { get; set; } = DateTime.Now;

        public DateTime? DataResposta { get; set; }

        [StringLength(1000)]
        public string? MotivoReprovacao { get; set; }

        [StringLength(1000)]
        public string? ObservacoesAdmin { get; set; }

        public ArtigoEducacao ToArtigoEducacao()
        {
            var destaques = !string.IsNullOrWhiteSpace(DestaquesTexto)
                ? DestaquesTexto.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                : new List<string>();

            var ameacas = !string.IsNullOrWhiteSpace(AmeacasTexto)
                ? AmeacasTexto.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                : new List<string>();

            var acoes = !string.IsNullOrWhiteSpace(AcoesTexto)
                ? AcoesTexto.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                : new List<string>();

            var referencias = !string.IsNullOrWhiteSpace(ReferenciasTexto)
                ? ReferenciasTexto.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                : new List<string>();

            return new ArtigoEducacao
            {
                Id = 10000 + Id,
                Slug = Slug,
                Titulo = Titulo,
                Subtitulo = Subtitulo,
                Categoria = Categoria,
                Resumo = Resumo,
                ConteudoHtml = ConteudoHtml,
                EspecieNome = EspecieNome ?? string.Empty,
                EspecieId = EspecieId,
                StatusConservacao = StatusConservacao ?? string.Empty,
                Autor = Autor,
                Instituicao = Instituicao ?? string.Empty,
                DataPublicacao = DataSubmissao,
                TempoLeituraMinutos = TempoLeituraMinutos <= 0 ? 5 : TempoLeituraMinutos,
                ImagemUrl = !string.IsNullOrWhiteSpace(ImagemUrl) ? ImagemUrl : "/img/campanhas/educacao-escolas.jpg",
                Destaques = destaques,
                AmeacasChave = ameacas,
                AcoesConservacao = acoes,
                Referencias = referencias
            };
        }
    }
}
