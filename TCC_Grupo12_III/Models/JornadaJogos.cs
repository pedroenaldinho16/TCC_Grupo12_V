using System.ComponentModel.DataAnnotations.Schema;

namespace TCC_Grupo12_III.Models;

// Snapshots versionados nos campos JSON existentes; nenhum dado histórico é substituído.
public class JornadaJogo
{
    public int Versao { get; set; } = 1;
    public string Tipo { get; set; } = "";
    public List<CartaFauna> Cartas { get; set; } = [];
    public List<int> OrdemNomes { get; set; } = [];
    public List<MissaoAmbiental> Missoes { get; set; } = [];
}

public record CartaFauna(int Id, string Nome, string NomeCientifico, string Categoria,
    string Imagem, string Habitat, string Alimentacao, string Caracteristicas, string Importancia, string Fonte);
public record MissaoAmbiental(string Id, string Titulo, string Objetivo, string Aprendizado, string Fonte);
public record EventoJogo(DateTime Quando, string Acao, string Alvo, bool Avancou, string Mensagem);

public class EstadoJornadaJogo
{
    public int Revisao { get; set; }
    public string Etapa { get; set; } = "estudo";
    public HashSet<int> Estudadas { get; set; } = [];
    public HashSet<int> Associadas { get; set; } = [];
    public int? Selecionada { get; set; }
    public int? CartaAberta { get; set; }
    public string Ferramenta { get; set; } = "observar";
    public string? Carregando { get; set; }
    public HashSet<string> Objetivos { get; set; } = [];
    public int Interacoes { get; set; }
    public int Revisoes { get; set; }
    public List<EventoJogo> Diario { get; set; } = [];
}

public record JornadaJogoViewModel(JornadaTentativa Tentativa, JornadaJogo Jogo, EstadoJornadaJogo Estado);
public record PassoMissao(int Etapa, string Titulo, string Instrucao, string Item, string NomeItem, string Destino, string Ferramenta);
