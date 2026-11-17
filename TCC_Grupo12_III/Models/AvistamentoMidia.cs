using System;
using System.ComponentModel.DataAnnotations;

namespace TCC_Grupo12_III.Models
{
    public class AvistamentoMidia
    {
        public int Id { get; set; }

        public int AvistamentoId { get; set; }
        public Avistamento? Avistamento { get; set; }

        [Required]
        [StringLength(300)]
        public string Caminho { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Tipo { get; set; } = "Foto"; // "Foto" ou "Video"

        public DateTime CriadoEm { get; set; } = DateTime.Now;
    }
}
