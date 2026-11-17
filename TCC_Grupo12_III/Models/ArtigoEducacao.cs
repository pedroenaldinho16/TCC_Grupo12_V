using System;
using System.Collections.Generic;

namespace TCC_Grupo12_III.Models
{
    public class ArtigoEducacao
    {
        public int Id { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Subtitulo { get; set; } = string.Empty;
        public string Categoria { get; set; } = "Mamíferos";
        public string Resumo { get; set; } = string.Empty;
        public string ConteudoHtml { get; set; } = string.Empty;
        public string EspecieNome { get; set; } = string.Empty;
        public int? EspecieId { get; set; }
        public string StatusConservacao { get; set; } = string.Empty;
        public string Autor { get; set; } = string.Empty;
        public string Instituicao { get; set; } = string.Empty;
        public DateTime DataPublicacao { get; set; }
        public int TempoLeituraMinutos { get; set; } = 5;
        public string ImagemUrl { get; set; } = string.Empty;
        public List<string> Destaques { get; set; } = new();
        public List<string> AmeacasChave { get; set; } = new();
        public List<string> AcoesConservacao { get; set; } = new();
        public List<string> Referencias { get; set; } = new();
    }
}
