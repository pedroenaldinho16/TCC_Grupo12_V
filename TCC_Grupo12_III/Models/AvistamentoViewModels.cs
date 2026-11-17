using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TCC_Grupo12_III.Models
{
    public class AvistamentoMapaViewModel
    {
        public List<AvistamentoMapaItem> Avistamentos { get; set; } = new();
        public List<Especie> Especies { get; set; } = new();
        public List<Especie> EspeciesPokedex { get; set; } = new();
        public Dictionary<int, int> ReservaPorEspecie { get; set; } = new();
        public NovoAvistamentoForm Form { get; set; } = new();

        // Elegibilidade para registrar avistamentos
        public bool PodeRegistrar { get; set; }
        public string? MotivoBloqueio { get; set; } // "login_necessario", "nivel_insuficiente", "sem_cartinha", "sem_reserva"
        public bool EstaAutenticado { get; set; }
        public int NivelUsuario { get; set; }
        public int XpUsuario { get; set; }
        public int NivelNecessario { get; set; } = 3;
        public int XpNecessario { get; set; } = 200;
        public int EspeciesPokedexCount { get; set; }
        public int TotalReservaDisponivel { get; set; }
    }

    public class AvistamentoRegistroViewModel
    {
        public List<Especie> EspeciesPokedex { get; set; } = new();
        public Dictionary<int, int> ReservaPorEspecie { get; set; } = new();
        public NovoAvistamentoForm Form { get; set; } = new();
        public int NivelUsuario { get; set; }
        public int XpUsuario { get; set; }
        public bool IsPrivilegiado { get; set; }
    }

    public class AvistamentoMidiaItemViewModel
    {
        public int Id { get; set; }
        public string Caminho { get; set; } = string.Empty;
        public string Tipo { get; set; } = "Foto"; // "Foto" ou "Video"
    }

    public class AvistamentoMapaItem
    {
        public int Id { get; set; }
        public string NomeAnimal { get; set; } = string.Empty;
        public string? NomeCientifico { get; set; }
        public int? EspecieId { get; set; }
        public string? FotoPath { get; set; }
        public List<AvistamentoMidiaItemViewModel> Midias { get; set; } = new();
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime DataAvistamento { get; set; }
        public int Quantidade { get; set; }
        public string? Observacoes { get; set; }
        public string NomeObservador { get; set; } = string.Empty;
        public int? UsuarioId { get; set; }
        public bool PodeRemover { get; set; }
    }

    public class NovoAvistamentoForm
    {
        [Display(Name = "Espécie avistada")]
        public int? EspecieId { get; set; }

        // Strings para preservar o ponto decimal enviado pelo JS; o binding de double usaria a cultura do servidor
        // (pt-BR), onde o ponto é separador de milhar e corromperia a coordenada.
        [Required(ErrorMessage = "Marque no mapa o local do avistamento")]
        public string? Latitude { get; set; }

        [Required(ErrorMessage = "Marque no mapa o local do avistamento")]
        public string? Longitude { get; set; }

        [Required(ErrorMessage = "A data do avistamento é obrigatória")]
        [DataType(DataType.Date)]
        [Display(Name = "Data do avistamento")]
        public DateTime DataAvistamento { get; set; } = DateTime.Today;

        [Range(1, 500, ErrorMessage = "A quantidade deve estar entre 1 e 500")]
        [Display(Name = "Quantidade de indivíduos")]
        public int Quantidade { get; set; } = 1;

        [StringLength(500, ErrorMessage = "As observações devem ter no máximo 500 caracteres")]
        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }

        [Required(ErrorMessage = "O nome do observador é obrigatório")]
        [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
        [Display(Name = "Seu nome")]
        public string NomeObservador { get; set; } = string.Empty;

        // Suporte a múltiplas fotos e vídeos
        [Display(Name = "Fotos do animal")]
        public List<IFormFile> Fotos { get; set; } = new();

        [Display(Name = "Vídeos do avistamento")]
        public List<IFormFile> Videos { get; set; } = new();

        // Foto única opcional mantida para compatibilidade
        [Display(Name = "Foto do animal")]
        public IFormFile? Foto { get; set; }
    }
}
