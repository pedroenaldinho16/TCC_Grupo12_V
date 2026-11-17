using System.Collections.Generic;

namespace TCC_Grupo12_III.Models
{
    public class ForumModeracaoViewModel
    {
        public List<Usuario> UsuariosSuspensos { get; set; } = new();
        public List<Usuario> UsuariosBanidos { get; set; } = new();
        public List<Usuario> Moderadores { get; set; } = new();
        public List<ForumTopico> TopicosRecentes { get; set; } = new();
        public List<ForumResposta> RespostasRecentes { get; set; } = new();
        public bool IsAdmin { get; set; }
        public string? TermoBusca { get; set; }
        public List<Usuario> UsuariosEncontrados { get; set; } = new();
    }
}
