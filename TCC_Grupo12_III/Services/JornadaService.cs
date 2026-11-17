using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;
namespace TCC_Grupo12_III.Services;
public class JornadaService(AppDbContext db) {
 public static readonly string[] Titulos = ["Detetive da Fauna","Guardião da Cantareira","Missão de Preservação"];
 public static readonly string[] Recompensas = ["Avatar de Guardião da Cantareira","Acessório de guardião","Mapa de avistamentos"];
 public int UsuarioId(ClaimsPrincipal user) => int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier),out var id)?id:0;
 public int Concluidos(ClaimsPrincipal user) {var id=UsuarioId(user);return db.JornadaTentativas.Where(t=>t.UsuarioId==id&&t.Aprovada).Select(t=>t.Desafio).Distinct().Count();}
 public bool MapaLiberado(ClaimsPrincipal user) => user.Identity?.IsAuthenticated==true && (user.IsInRole("Admin") || Concluidos(user)==3);
 public bool PodeIniciar(ClaimsPrincipal user,int desafio) {
  if(desafio<1||desafio>3||UsuarioId(user)==0)return false;
  var uid=UsuarioId(user);
  return desafio==1||db.JornadaTentativas.Where(t=>t.UsuarioId==uid&&t.Aprovada&&t.Desafio<desafio).Select(t=>t.Desafio).Distinct().Count()==desafio-1;
 }
 public List<JornadaTentativa> Historico(ClaimsPrincipal user) {var id=UsuarioId(user);return db.JornadaTentativas.AsNoTracking().Where(t=>t.UsuarioId==id).OrderByDescending(t=>t.IniciadaEm).ToList();}
}
