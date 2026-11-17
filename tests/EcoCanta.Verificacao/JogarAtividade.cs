using System.Net;
using TCC_Grupo12_III.Models;

internal static class JogarAtividade
{
    public static async Task Completar(Session session, Func<int, JornadaTentativa> read, int id, bool finalizar = true)
    {
        async Task Act(string action, string target = "", string tool = "observar")
        {
            var response = await session.Post("/Jornada/Agir", new() { ["id"] = id.ToString(), ["revisao"] = read(id).EstadoJogo.Revisao.ToString(), ["acao"] = action, ["alvo"] = target, ["ferramenta"] = tool }, "/Jornada");
            if (response.StatusCode != HttpStatusCode.Redirect) throw new Exception("Ação de jogo não foi registrada: " + action);
        }
        var t = read(id); var jogo = t.Jogo!;
        if (jogo.Tipo == "cartas")
        {
            foreach (var c in jogo.Cartas.Where(c => !t.EstadoJogo.Estudadas.Contains(c.Id))) await Act("estudar", c.Id.ToString());
            await Act("associar");
            if (!finalizar) { await Act("foto", jogo.Cartas[0].Id.ToString()); await Act("nome", jogo.Cartas[1].Id.ToString()); return; }
            foreach (var c in jogo.Cartas.Where(c => !t.EstadoJogo.Associadas.Contains(c.Id))) { await Act("foto", c.Id.ToString()); await Act("nome", c.Id.ToString()); }
        }
        else
        {
            foreach (var item in new[] { "garrafa", "lata", "embalagem" }) { await Act("interagir", item, "recolher"); await Act("interagir", "descarte", "recolher"); }
            await Act("interagir", "ave", "observar"); await Act("interagir", "ave", "sinalizar"); await Act("interagir", "ave", "radio");
            if (finalizar) { await Act("interagir", "atalho", "observar"); await Act("interagir", "atalho", "sinalizar"); }
        }
    }
}
