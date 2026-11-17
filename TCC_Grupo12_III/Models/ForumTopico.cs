using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TCC_Grupo12_III.Models
{
    public class ForumTopico
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O título é obrigatório")]
        [StringLength(150)]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A mensagem é obrigatória")]
        public string Conteudo { get; set; } = string.Empty;

        public int UsuarioId { get; set; }

        public Usuario? Usuario { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        [Display(Name = "Hashtags (separadas por vírgula)")]
        public string Hashtags { get; set; } = string.Empty;

        [Display(Name = "Categoria")]
        public string Categoria { get; set; } = "comunidade"; // "noticia" ou "comunidade"

        public int? ComunidadeId { get; set; }

        public ForumComunidade? Comunidade { get; set; }

        public List<ForumResposta> Respostas { get; set; } = new();

        public List<ForumCurtidaTopico> Curtidas { get; set; } = new();
    }
}
