using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TCC_Grupo12_III.Models
{
    public class ForumResposta
    {
        public int Id { get; set; }

        public int ForumTopicoId { get; set; }

        public ForumTopico? ForumTopico { get; set; }

        public int UsuarioId { get; set; }

        public Usuario? Usuario { get; set; }

        [Required(ErrorMessage = "A resposta é obrigatória")]
        public string Conteudo { get; set; } = string.Empty;

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        public List<ForumCurtidaResposta> Curtidas { get; set; } = new();
    }
}
