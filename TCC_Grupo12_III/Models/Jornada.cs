using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
namespace TCC_Grupo12_III.Models;
public class JornadaPergunta {
 public int Id { get; set; }
 [Range(1,3)] public int Desafio { get; set; }
 [Required, StringLength(1500)] public string Enunciado { get; set; } = "";
 [Required] public string OpcaoA { get; set; } = "";
 [Required] public string OpcaoB { get; set; } = "";
 [Required] public string OpcaoC { get; set; } = "";
 [Required] public string OpcaoD { get; set; } = "";
 [Range(1,4)] public int Correta { get; set; }
 [Required] public string Explicacao { get; set; } = "";
 public string Fonte { get; set; } = "";
 public string? ImagemUrl { get; set; }
 public bool Ativa { get; set; } = true;
 public string Opcao(int n) => n switch {1=>OpcaoA,2=>OpcaoB,3=>OpcaoC,4=>OpcaoD,_=>""};
}
public class JornadaTentativa {
 public int Id { get; set; }
 public int UsuarioId { get; set; }
 public Usuario? Usuario { get; set; }
 public int Desafio { get; set; }
 public DateTime IniciadaEm { get; set; } = DateTime.UtcNow;
 public DateTime? ConcluidaEm { get; set; }
 public int Pontos { get; set; }
 public int Total { get; set; }
 public bool Aprovada { get; set; }
 public string QuestoesJson { get; set; } = "[]";
 public string RespostasJson { get; set; } = "[]";
 [NotMapped] public JornadaJogo? Jogo => QuestoesJson.TrimStart().StartsWith('{') ? JsonSerializer.Deserialize<JornadaJogo>(QuestoesJson) : null;
 [NotMapped] public EstadoJornadaJogo EstadoJogo => Jogo is not null ? JsonSerializer.Deserialize<EstadoJornadaJogo>(RespostasJson) ?? new() : new();
 [NotMapped] public List<JornadaPergunta> Questoes => Jogo is null ? JsonSerializer.Deserialize<List<JornadaPergunta>>(QuestoesJson) ?? new() : [];
 [NotMapped] public List<int> Respostas {
  get {
   if(Jogo is not null)return [];
   if(!RespostasJson.TrimStart().StartsWith('{'))return JsonSerializer.Deserialize<List<int>>(RespostasJson) ?? new();
   using var doc=JsonDocument.Parse(RespostasJson);
   return doc.RootElement.TryGetProperty("Respostas",out var respostas)?respostas.Deserialize<List<int>>() ?? new():[];
  }
 }
 [NotMapped] public bool ArquivadaNaAtualizacao => Jogo is null && RespostasJson.Contains("\"EncerradaNaAtualizacao\":true", StringComparison.Ordinal);
 [NotMapped] public decimal Aproveitamento => Total==0 ? 0 : decimal.Round(Pontos*100m/Total,1);
}
public record JornadaQuestaoViewModel(JornadaTentativa Tentativa, JornadaPergunta Pergunta, int Posicao, bool Feedback);
