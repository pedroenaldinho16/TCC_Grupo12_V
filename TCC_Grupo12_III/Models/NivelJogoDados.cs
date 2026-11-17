using System.Collections.Generic;
using System.Text.Json;

namespace TCC_Grupo12_III.Models
{
    public class NivelJogoDados
    {
        public const int AlturaPadrao = 17;
        public const int AlturaMinima = 12;
        public const int AlturaMaxima = 120;
        public const int LarguraMinima = 40;
        public const int LarguraMaxima = 400;

        public int Largura { get; set; } = 120;
        public int Altura { get; set; } = AlturaPadrao;

        public List<NivelPlataforma> Plataformas { get; set; } = new();

        public List<NivelPerigo> Perigos { get; set; } = new();

        public List<NivelPlataformaMovel> PlataformasMoveis { get; set; } = new();

        public List<NivelPerigoMovel> PerigosMoveis { get; set; } = new();

        public List<NivelItem> Itens { get; set; } = new();

        public List<NivelAnimal> Animais { get; set; } = new();

        public List<NivelPonto> Checkpoints { get; set; } = new();

        public NivelPonto? Inicio { get; set; }

        public NivelPonto? Objetivo { get; set; }

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };

        public string Serializar() => JsonSerializer.Serialize(this, JsonOptions);

        public static NivelJogoDados? Desserializar(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonSerializer.Deserialize<NivelJogoDados>(json, JsonOptions);
            }
            catch (JsonException)
            {
                return null;
            }
        }
    }

    public interface INivelRetangulo
    {
        int X { get; set; }
        int Y { get; set; }
        int W { get; set; }
        int H { get; set; }
    }

    public class NivelPlataforma : INivelRetangulo
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; } = 1;
        public int H { get; set; } = 1;
        public string Tipo { get; set; } = "terra";
        public string Rot { get; set; } = "cima";
    }

    public class NivelPerigo : INivelRetangulo
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int W { get; set; } = 1;
        public int H { get; set; } = 1;
        public string Tipo { get; set; } = "fogo";
        public string Direcao { get; set; } = "cima";
        public string Dir { get => Direcao; set => Direcao = value; }
    }

    public class NivelPlataformaMovel
    {
        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int X2 { get; set; }
        public int Y2 { get; set; }
        public int Largura { get; set; } = 2;
        public double Velocidade { get; set; } = 1.0;
        public string Tipo { get; set; } = "plataforma";
    }

    public class NivelPerigoMovel
    {
        public int X1 { get; set; }
        public int Y1 { get; set; }
        public int X2 { get; set; }
        public int Y2 { get; set; }
        public string Tipo { get; set; } = "espinho";
        public string Direcao { get; set; } = "cima";
        public string Dir { get => Direcao; set => Direcao = value; }
        public double Velocidade { get; set; } = 1.0;
    }

    public class NivelItem
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Tipo { get; set; } = "dash";
    }

    public class NivelAnimal
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int EspecieId { get; set; }
    }

    public class NivelPonto
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
