using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TCC_Grupo12_III.Models
{
    public class AdminEspecieQuizViewModel
    {
        public int EspecieId { get; set; }
        public string EspecieNome { get; set; } = string.Empty;
        public string? EspecieNomeCientifico { get; set; }
        public List<AdminEspecieQuizPerguntaViewModel> Perguntas { get; set; } = new();
    }

    public class AdminEspecieQuizPerguntaViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O enunciado é obrigatório")]
        public string Enunciado { get; set; } = string.Empty;

        [Required(ErrorMessage = "A alternativa A é obrigatória")]
        [Display(Name = "Alternativa A")]
        public string OpcaoA { get; set; } = string.Empty;

        [Required(ErrorMessage = "A alternativa B é obrigatória")]
        [Display(Name = "Alternativa B")]
        public string OpcaoB { get; set; } = string.Empty;

        [Required(ErrorMessage = "A alternativa C é obrigatória")]
        [Display(Name = "Alternativa C")]
        public string OpcaoC { get; set; } = string.Empty;

        [Required(ErrorMessage = "A alternativa D é obrigatória")]
        [Display(Name = "Alternativa D")]
        public string OpcaoD { get; set; } = string.Empty;

        [Range(1, 4, ErrorMessage = "Escolha a alternativa correta")]
        [Display(Name = "Alternativa correta")]
        public int RespostaCorreta { get; set; } = 1;
    }
}
