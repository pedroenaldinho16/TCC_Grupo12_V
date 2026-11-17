using System;
using System.ComponentModel.DataAnnotations;

namespace TCC_Grupo12_III.Models
{
    public class FaqMensagem
    {
        public int Id { get; set; }

        public int FaqTicketId { get; set; }
        public FaqTicket? FaqTicket { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        [Required(ErrorMessage = "A mensagem não pode ser vazia")]
        [StringLength(2000)]
        public string Conteudo { get; set; } = string.Empty;

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        public bool IsEquipeModOuAdmin { get; set; } = false;

        [StringLength(50)]
        public string? CargoAutor { get; set; } // "Administrador", "Moderador", "Guardião"
    }
}
