using System;

namespace TCC_Grupo12_III.Models
{
    public class ForumCurtidaTopico
    {
        public int Id { get; set; }

        public int ForumTopicoId { get; set; }

        public ForumTopico? ForumTopico { get; set; }

        public int UsuarioId { get; set; }

        public Usuario? Usuario { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.Now;
    }
}
