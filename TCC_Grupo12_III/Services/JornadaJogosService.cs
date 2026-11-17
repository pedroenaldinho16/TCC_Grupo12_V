using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;

namespace TCC_Grupo12_III.Services;

public class JornadaJogosService(AppDbContext db)
{
    public const string Guia = "https://www.gov.br/icmbio/pt-br/centrais-de-conteudo/guia2-pdf";
    public const string Fauna = "https://www.gov.br/pt-br/servicos/realizar-a-entrega-de-animal-silvestre-no-centro-de-triagem-de-animais-silvestres-cetas";
    public static readonly string[] Residuos = ["garrafa", "lata", "embalagem"];
    public static readonly string[] Ferramentas = ["observar", "recolher", "sinalizar", "radio"];

    public JornadaJogo Criar(int desafio)
    {
        if (desafio == 1)
        {
            var cartas = db.Especies.OrderBy(e => e.Id).AsEnumerable()
                .Where(e => !string.IsNullOrWhiteSpace(e.ImagemUrl) && e.ImagemUrl.StartsWith("/img/", StringComparison.Ordinal) && !e.ImagemUrl.Contains(".."))
                .Take(14).Select(e => new CartaFauna(e.Id, e.Nome, e.NomeCientifico, e.Categoria,
                    e.ImagemUrl!, e.Habitat, e.Alimentacao, e.Caracteristicas, e.ImportanciaEcologica, e.FonteCientifica)).ToList();
            return new() { Tipo = "cartas", Cartas = cartas, OrdemNomes = cartas.Select(c => c.Id).OrderBy(_ => Random.Shared.Next()).ToList() };
        }
        return new()
        {
            Tipo = "missao",
            Missoes = [
                new("residuos", "Uma trilha sem resíduos", "Recolha os três resíduos e leve cada um ao ponto de descarte da entrada.",
                    "Leve os resíduos de volta. Deixá-los no ambiente prolonga os impactos da visita.", Guia),
                new("ave", "Ajuda para uma ave", "Observe a situação, sinalize à distância e use o rádio para chamar a equipe.",
                    "Uma ave presa precisa de avaliação. Nesta missão, a equipe especializada assume o atendimento; o visitante não faz a soltura.", Fauna),
                new("trilha", "O caminho também protege", "Investigue o atalho e sinalize o percurso autorizado com a equipe do jogo.",
                    "Permanecer nas trilhas evita ampliar danos à vegetação. A sinalização desta cena representa uma ação orientada pela equipe do parque.", Guia)
            ]
        };
    }

    public static int Total(JornadaJogo jogo) => jogo.Tipo == "cartas" ? jogo.Cartas.Count : 8;
    public static int Pontos(JornadaJogo jogo, EstadoJornadaJogo estado) => jogo.Tipo == "cartas" ? estado.Associadas.Count : estado.Objetivos.Count;
    public static bool MissaoConcluida(string id, EstadoJornadaJogo estado) => id switch
    {
        "residuos" => Residuos.All(r => estado.Objetivos.Contains("descarte-" + r)),
        "ave" => estado.Objetivos.Contains("ave-atendida"),
        "trilha" => estado.Objetivos.Contains("trilha-sinalizada"),
        _ => false
    };

    // Retorna null somente para uma ação desconhecida. Objetivos e pontuação vêm do servidor.
    public static string? Agir(JornadaJogo jogo, EstadoJornadaJogo e, string acao, string alvo, string ferramenta, string destino = "",bool? aberta=null)
    {
        var antes = Pontos(jogo, e);
        string? mensagem = jogo.Tipo == "cartas" ? Cartas(jogo, e, acao, alvo,aberta) : acao == "mover" ? Mover(e, alvo, destino) : Campo(e, acao, alvo, ferramenta);
        if (mensagem is null) return null;
        e.Interacoes++;
        e.Diario.Add(new(DateTime.UtcNow, acao, alvo, Pontos(jogo, e) > antes, mensagem));
        if (e.Diario.Count > 60) e.Diario.RemoveAt(0);
        e.Revisao++;
        return mensagem;
    }

    public static PassoMissao Passo(EstadoJornadaJogo e)
    {
        if (!MissaoConcluida("residuos", e)) return new(1, "Deixe a trilha limpa", "Arraste cada resíduo até a lixeira na entrada do parque.", "", "", "descarte", "recolher");
        if (!e.Objetivos.Contains("ave-observada")) return new(2, "Uma ave precisa de ajuda", "Arraste os binóculos até a ave para observar a situação à distância.", "binoculos", "Binóculos", "ave", "observar");
        if (!e.Objetivos.Contains("ave-sinalizada")) return new(2, "Proteja o espaço da ave", "Arraste a sinalização até a área da ave. Mantenha distância e avise os visitantes.", "sinalizacao", "Sinalização", "ave", "sinalizar");
        if (!e.Objetivos.Contains("ave-atendida")) return new(2, "Chame quem pode ajudar", "Arraste o rádio até a ave para chamar a equipe de proteção do jogo.", "radio", "Rádio", "ave", "radio");
        if (!e.Objetivos.Contains("trilha-observada")) return new(3, "Descubra o caminho certo", "Arraste os binóculos até o atalho para investigar as pegadas fora da trilha.", "binoculos", "Binóculos", "atalho", "observar");
        if (!e.Objetivos.Contains("trilha-sinalizada")) return new(3, "Ajude a proteger a vegetação", "Arraste a placa até o atalho para orientar o percurso com a equipe do parque.", "placa", "Placa da trilha", "atalho", "sinalizar");
        return new(4, "Você cuidou da Cantareira!", "Trilha limpa, ave em atendimento e caminho sinalizado. Seu mapa está liberado.", "", "", "", "observar");
    }

    private static string? Mover(EstadoJornadaJogo e, string item, string destino)
    {
        if (Residuos.Contains(item) && destino == "descarte")
        {
            if(e.Objetivos.Contains("descarte-"+item))return "Este resíduo já está na lixeira. Procure outro na trilha.";
            if(e.Carregando is not null && e.Carregando != item)return "Leve primeiro o resíduo que está na sua bolsa até a lixeira.";
            e.Objetivos.Add("descarte-"+item);e.Carregando=null;e.Ferramenta="recolher";
            return "Resíduo na lixeira! Um objeto a menos na floresta. Na visita real, leve seus resíduos de volta para o descarte adequado.";
        }
        return (item,destino) switch {
            ("binoculos","ave" or "atalho") => Campo(e,"interagir",destino,"observar"),
            ("sinalizacao","ave") => Campo(e,"interagir",destino,"sinalizar"),
            ("radio","ave") => Campo(e,"interagir",destino,"radio"),
            ("placa","atalho") => Campo(e,"interagir",destino,"sinalizar"),
            _ => null
        };
    }

    private static string? Cartas(JornadaJogo jogo, EstadoJornadaJogo e, string acao, string alvo,bool? aberta=null)
    {
        if (acao == "associar")
        {
            if (e.Estudadas.Count != jogo.Cartas.Count) return "Vire todas as cartas para conhecer as espécies antes de montar as associações.";
            e.Etapa = "associacao";
            return "Agora conecte cada fotografia ao nome correspondente. Se precisar, consulte as cartas novamente.";
        }
        if (!int.TryParse(alvo, out var id) || jogo.Cartas.All(c => c.Id != id)) return null;
        var carta = jogo.Cartas.Single(c => c.Id == id);
        if (acao == "estudar")
        {
            e.Estudadas.Add(id);
            if(aberta.HasValue){
                if(aberta.Value)e.CartaAberta=id;
                else if(e.CartaAberta==id)e.CartaAberta=null;
            }else e.CartaAberta=e.CartaAberta==id?null:id;
            return $"Carta de {carta.Nome} explorada. Observe também seu habitat e sua alimentação.";
        }
        if (e.Etapa != "associacao") return "Explore as cartas e inicie a associação para continuar.";
        if (acao == "foto")
        {
            if (e.Associadas.Contains(id)) return "Essa espécie já foi associada. Procure outra fotografia.";
            e.Selecionada = id;
            return "Fotografia selecionada. Encontre a carta com o nome dessa espécie.";
        }
        if (acao != "nome") return null;
        if (e.Selecionada is null) return "Selecione primeiro uma fotografia, depois a carta com o nome.";
        if (e.Associadas.Contains(id)) return "Essa carta já tem um par. Procure uma carta disponível.";
        if (id != e.Selecionada)
        {
            e.Revisoes++;
            var selecionada = jogo.Cartas.Single(c => c.Id == e.Selecionada);
            return $"As cartas ainda não correspondem. Pista da ficha: {selecionada.Caracteristicas} Consulte o verso e tente outra associação.";
        }
        e.Associadas.Add(id); e.Selecionada = null;
        return $"Par descoberto: {carta.Nome}! Habitat: {carta.Habitat}";
    }

    private static string? Campo(EstadoJornadaJogo e, string acao, string alvo, string ferramenta)
    {
        if (acao != "interagir" || !Ferramentas.Contains(ferramenta) || !Residuos.Concat(["descarte", "ave", "atalho"]).Contains(alvo)) return null;
        e.Ferramenta = ferramenta;
        if (Residuos.Contains(alvo))
        {
            if (e.Objetivos.Contains("descarte-" + alvo)) return "Esse resíduo já chegou ao ponto de descarte.";
            if (ferramenta != "recolher") return "Uma embalagem ficou na trilha. Use Recolher e depois toque no ponto de descarte da entrada.";
            if (e.Carregando is not null) return "Você já está levando um resíduo. Entregue-o no ponto de descarte antes de pegar outro.";
            e.Carregando = alvo;
            return "Resíduo recolhido. Agora leve-o ao ponto de descarte na entrada, usando a mesma ferramenta.";
        }
        if (alvo == "descarte")
        {
            if (ferramenta != "recolher" || e.Carregando is null) return "Este é o ponto de descarte na entrada. Recolha uma embalagem e traga-a até aqui.";
            e.Objetivos.Add("descarte-" + e.Carregando); e.Carregando = null;
            return "Resíduo destinado! A trilha está mais limpa. Na visita real, leve seus resíduos de volta para o descarte adequado.";
        }
        if (alvo == "ave")
        {
            if (e.Objetivos.Contains("ave-atendida")) return "A equipe já assumiu o atendimento da ave. Você ajudou observando e pedindo apoio.";
            if (ferramenta == "observar")
            {
                e.Objetivos.Add("ave-observada");
                return "Você encontrou uma ave presa. Mantenha distância e sinalize a situação; não abra a gaiola nem confronte pessoas.";
            }
            if (!e.Objetivos.Contains("ave-observada")) return "Primeiro use Observar para entender a situação à distância.";
            if (ferramenta == "sinalizar")
            {
                e.Objetivos.Add("ave-sinalizada");
                return "Situação sinalizada à distância. Agora use o Rádio para avisar a equipe responsável nesta missão.";
            }
            if (ferramenta == "radio")
            {
                if (!e.Objetivos.Contains("ave-sinalizada")) return "Sinalize a situação à distância antes de chamar a equipe no jogo.";
                e.Objetivos.Add("ave-atendida");
                return "A equipe chegou e assumiu o atendimento! A avaliação e a destinação da ave ficam com profissionais habilitados.";
            }
            e.Revisoes++;
            return "A ferramenta de recolher serve para os resíduos da cena. Para a ave, sinalize e peça apoio à equipe.";
        }
        if (e.Objetivos.Contains("trilha-sinalizada")) return "O percurso autorizado já está sinalizado. A vegetação fora dele deve ser respeitada.";
        if (ferramenta == "observar")
        {
            e.Objetivos.Add("trilha-observada");
            return "Pegadas seguem por um atalho fora do percurso. Use Sinalizar para orientar a visita pela trilha autorizada.";
        }
        if (!e.Objetivos.Contains("trilha-observada")) return "Observe as pegadas primeiro para descobrir o problema no caminho.";
        if (ferramenta == "sinalizar")
        {
            e.Objetivos.Add("trilha-sinalizada");
            return "Percurso sinalizado com a equipe do jogo! Evitar atalhos ajuda a proteger as plantas e o solo.";
        }
        return "Este ponto precisa de orientação do percurso. Use Sinalizar depois de observar o atalho.";
    }
}
