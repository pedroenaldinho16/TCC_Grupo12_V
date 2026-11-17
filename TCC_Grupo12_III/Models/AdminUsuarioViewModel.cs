using System;
using System.ComponentModel.DataAnnotations;

namespace TCC_Grupo12_III.Models
{
    public class AdminUsuarioViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100)]
        [Display(Name = "Nome completo")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public string Email { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Nova senha (deixe em branco para manter a atual)")]
        public string? NovaSenha { get; set; }

        [Display(Name = "Pontos de XP acumulados")]
        public int Xp { get; set; }

        public int Nivel { get; set; } = 1;

        // Cargos e Papéis
        [Display(Name = "Administrador Geral")]
        public bool IsAdmin { get; set; }

        [Display(Name = "Moderador da Plataforma (Fórum, Avistamentos e Denúncias)")]
        public bool IsModerador { get; set; }

        [Display(Name = "Jornalista Oficial")]
        public bool IsJornalista { get; set; }

        // Moderação: Banimento
        [Display(Name = "Conta banida")]
        public bool IsBanido { get; set; }

        [Display(Name = "Motivo do banimento")]
        public string? MotivoBanimento { get; set; }

        public DateTime? DataBanimento { get; set; }

        public string? BanidoPorNome { get; set; }

        // Moderação: Suspensão no Fórum
        [Display(Name = "Suspenso do fórum até")]
        public DateTime? SuspensoForumAte { get; set; }

        [Display(Name = "Motivo da suspensão no fórum")]
        public string? MotivoSuspensaoForum { get; set; }

        public DateTime? DataSuspensaoForum { get; set; }

        public string? SuspensoPorNome { get; set; }

        // Credenciamento de Jornalista
        [Display(Name = "Status da credencial de jornalista")]
        public string? StatusJornalista { get; set; }

        [Display(Name = "Registro profissional (MTB / Imprensa)")]
        public string? RegistroJornalista { get; set; }

        [Display(Name = "Veículo de comunicação / Organização")]
        public string? VeiculoJornalista { get; set; }

        [Display(Name = "Portfólio / Matérias publicadas")]
        public string? LinkPortfolioJornalista { get; set; }

        [Display(Name = "Justificativa / Descrição da atuação")]
        public string? JustificativaJornalista { get; set; }

        public DateTime? DataSolicitacaoJornalista { get; set; }

        public DateTime? DataRespostaJornalista { get; set; }

        [Display(Name = "Motivo da reprovação da credencial")]
        public string? MotivoReprovacaoJornalista { get; set; }

        // Estatísticas e Atividade na Plataforma
        public int TotalTopicosForum { get; set; }
        public int TotalRespostasForum { get; set; }
        public int TotalAvistamentos { get; set; }
        public int TotalEspeciesPokedex { get; set; }
        public int TotalCampanhas { get; set; }
        public int TotalEstudosCientificos { get; set; }
        public int TotalJogosTentativas { get; set; }
    }
}
