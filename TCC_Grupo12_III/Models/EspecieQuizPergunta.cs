using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace TCC_Grupo12_III.Models
{
    public class EspecieQuizPergunta
    {
        public int Id { get; set; }

        public int EspecieId { get; set; }

        public Especie? Especie { get; set; }

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

        [Range(1, 4)]
        [Display(Name = "Alternativa correta")]
        public int RespostaCorreta { get; set; } = 1;

        [Display(Name = "Ordem")]
        public int Ordem { get; set; }

        public string OpcaoTexto(int numero) => numero switch
        {
            1 => OpcaoA,
            2 => OpcaoB,
            3 => OpcaoC,
            4 => OpcaoD,
            _ => string.Empty
        };
    }

    public class EspecieQuizResultadoItem
    {
        public EspecieQuizPergunta Pergunta { get; set; } = new();
        public int? Selecionada { get; set; }
        public bool Correta => Selecionada.HasValue && Selecionada.Value == Pergunta.RespostaCorreta;
    }

    public class EspecieQuizResultadoViewModel
    {
        public Especie Especie { get; set; } = new();
        public List<EspecieQuizResultadoItem> Itens { get; set; } = new();
        public int Acertos => Itens.Count(i => i.Correta);
        public const int PercentualParaDominar = 70;
        public bool Dominada { get; set; }
        public bool QuizAprovado { get; set; }
        public bool EstudoLido { get; set; }
        public string StatusMensagem { get; set; } = string.Empty;
        public string XpMensagem { get; set; } = string.Empty;
        public int Total => Itens.Count;
        public int Percentual => Total == 0 ? 0 : (int)System.Math.Round(Acertos * 100.0 / Total);
    }

    public class QuizRespostaInput
    {
        public int PerguntaId { get; set; }
        public int OpcaoSelecionada { get; set; }
    }
}
