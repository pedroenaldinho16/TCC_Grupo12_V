using System;

namespace TCC_Grupo12_III.Models
{
    public class ForumCurtidaResposta
    {
        public int Id { get; set; }

        public int ForumRespostaId { get; set; }

        public ForumResposta? ForumResposta { get; set; }

        public int UsuarioId { get; set; }

        public Usuario? Usuario { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.Now;
    }
}
