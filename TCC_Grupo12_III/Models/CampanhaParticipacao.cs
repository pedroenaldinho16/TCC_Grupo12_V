namespace TCC_Grupo12_III.Models
{
    public class CampanhaParticipacao
    {
        public int Id { get; set; }

        public int CampanhaId { get; set; }
        public Campanha? Campanha { get; set; }

        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
    }
}
