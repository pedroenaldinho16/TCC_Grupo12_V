using System.ComponentModel.DataAnnotations;

namespace TCC_Grupo12_III.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório")]
        [StringLength(100)]
        [Display(Name = "Nome completo")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A senha é obrigatória")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres")]
        public string Senha { get; set; } = string.Empty;

        public bool IsAdmin { get; set; } = false;

        public bool IsModerador { get; set; } = false;

        public DateTime? SuspensoForumAte { get; set; }

        [StringLength(500)]
        [Display(Name = "Motivo da suspensão no fórum")]
        public string? MotivoSuspensaoForum { get; set; }

        public DateTime? DataSuspensaoForum { get; set; }

        [StringLength(100)]
        [Display(Name = "Suspenso por")]
        public string? SuspensoPorNome { get; set; }

        public bool IsBanido { get; set; } = false;

        public DateTime? DataBanimento { get; set; }

        [StringLength(500)]
        [Display(Name = "Motivo do banimento")]
        public string? MotivoBanimento { get; set; }

        [StringLength(100)]
        [Display(Name = "Banido por")]
        public string? BanidoPorNome { get; set; }

        public bool EstaSuspensoDoForum()
        {
            return SuspensoForumAte.HasValue && SuspensoForumAte.Value > DateTime.Now;
        }

        public bool IsJornalista { get; set; } = false;

        [StringLength(30)]
        [Display(Name = "Status da credencial de jornalista")]
        public string? StatusJornalista { get; set; }

        [StringLength(100)]
        [Display(Name = "Registro profissional (MTB / Imprensa)")]
        public string? RegistroJornalista { get; set; }

        [StringLength(150)]
        [Display(Name = "Veículo de comunicação / Organização")]
        public string? VeiculoJornalista { get; set; }

        [StringLength(300)]
        [Display(Name = "Portfólio / Matérias publicadas")]
        public string? LinkPortfolioJornalista { get; set; }

        [StringLength(1000)]
        [Display(Name = "Justificativa / Descrição da atuação")]
        public string? JustificativaJornalista { get; set; }

        public DateTime? DataSolicitacaoJornalista { get; set; }

        public DateTime? DataRespostaJornalista { get; set; }

        [StringLength(1000)]
        [Display(Name = "Motivo da reprovação / Observação do admin")]
        public string? MotivoReprovacaoJornalista { get; set; }

        public int Xp { get; set; }
    }
}
