namespace TCC_Grupo12_III.Models;

public record AcessoMapaViewModel(bool Autenticado, bool Administrador, int Concluidos)
{
    public bool Liberado => Autenticado && (Administrador || Concluidos == 3);
    public string Estado => !Autenticado ? "Conheça o mapa" : Administrador ? "Acesso administrativo" : Liberado ? "Mapa desbloqueado" : $"{Concluidos} de 3 desafios concluídos";
    public string Acao => Liberado ? "Abrir mapa" : "Ver como desbloquear";
}

public record GuardiaoAvatarViewModel(bool Conquistado, bool Acessorio, bool Compacto = false);
