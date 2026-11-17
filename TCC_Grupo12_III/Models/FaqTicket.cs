using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TCC_Grupo12_III.Models
{
    public class FaqTicket
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        [Required(ErrorMessage = "O assunto da pergunta é obrigatório")]
        [StringLength(150)]
        [Display(Name = "Assunto")]
        public string Assunto { get; set; } = string.Empty;

        [Required]
        [StringLength(40)]
        [Display(Name = "Categoria")]
        public string Categoria { get; set; } = "Geral"; // "Diretrizes", "Fórum", "Avistamentos", "Campanhas", "Denúncias", "Geral"

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Aberto"; // "Aberto", "Respondido", "Fechado"

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        public DateTime AtualizadoEm { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? UltimaRespostaPor { get; set; }

        public List<FaqMensagem> Mensagens { get; set; } = new();
    }
}
