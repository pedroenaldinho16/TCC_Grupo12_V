using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;
namespace TCC_Grupo12_III.Controllers;
[Authorize]
public class ForumController(AppDbContext db):Controller {
 int Uid=>int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 bool PodePostar()=>db.Usuarios.Find(Uid) is {IsBanido:false} u&&!u.EstaSuspensoDoForum();
 Campanha? CampanhaDoTopico(ForumTopico? t)=>t!=null&&t.Categoria.StartsWith("campanha:")&&int.TryParse(t.Categoria[9..],out var id)?db.Campanhas.Find(id):null;
 bool Publica(Campanha? c)=>c!=null&&c.Ativa&&c.StatusAprovacao=="Aprovado";
 IActionResult Voltar(int id)=>Redirect(Url.Action("Detalhes","Campanhas",new{id})+"#mural");
 public IActionResult Index()=>RedirectToAction("Index","Campanhas");
 [HttpPost,ValidateAntiForgeryToken]public IActionResult Criar(int campanhaId,string? titulo,string? conteudo){
 var c=db.Campanhas.Find(campanhaId);if(!Publica(c))return NotFound();
 if(!PodePostar())return Forbid();
 if(string.IsNullOrWhiteSpace(titulo)||titulo.Length>150||string.IsNullOrWhiteSpace(conteudo)||conteudo.Length>2000){TempData["Erro"]="Preencha título e reflexão dentro dos limites indicados.";return Voltar(campanhaId);}
 db.ForumTopicos.Add(new ForumTopico{UsuarioId=Uid,Titulo=titulo.Trim(),Conteudo=conteudo.Trim(),Categoria="campanha:"+campanhaId});db.SaveChanges();return Voltar(campanhaId);
 }
 [HttpPost,ValidateAntiForgeryToken]public IActionResult Responder(int id,string? conteudo){
 var t=db.ForumTopicos.Find(id);var c=CampanhaDoTopico(t);if(!Publica(c))return NotFound();if(!PodePostar())return Forbid();
 if(!string.IsNullOrWhiteSpace(conteudo)&&conteudo.Length<=2000){db.ForumRespostas.Add(new ForumResposta{ForumTopicoId=id,UsuarioId=Uid,Conteudo=conteudo.Trim()});db.SaveChanges();}return Voltar(c!.Id);
 }
 [HttpPost,ValidateAntiForgeryToken]public IActionResult Curtir(int id){var t=db.ForumTopicos.Find(id);var c=CampanhaDoTopico(t);if(!Publica(c))return NotFound();if(!PodePostar())return Forbid();var like=db.ForumCurtidasTopico.FirstOrDefault(x=>x.ForumTopicoId==id&&x.UsuarioId==Uid);if(like==null)db.ForumCurtidasTopico.Add(new ForumCurtidaTopico{ForumTopicoId=id,UsuarioId=Uid});else db.ForumCurtidasTopico.Remove(like);db.SaveChanges();return Voltar(c!.Id);}
 [Authorize(Roles="Admin")]public IActionResult Moderacao()=>View(db.ForumTopicos.Include(t=>t.Usuario).Include(t=>t.Respostas).Where(t=>t.Categoria.StartsWith("campanha:")||t.Categoria.StartsWith("arquivada:")).OrderByDescending(t=>t.CriadoEm).ToList());
 [Authorize(Roles="Admin"),HttpPost,ValidateAntiForgeryToken]public IActionResult Arquivar(int id){var t=db.ForumTopicos.Find(id);if(t==null)return NotFound();t.Categoria=t.Categoria.StartsWith("campanha:")?t.Categoria.Replace("campanha:","arquivada:"):t.Categoria.Replace("arquivada:","campanha:");db.SaveChanges();return RedirectToAction(nameof(Moderacao));}
 [Authorize(Roles="Admin"),HttpPost,ValidateAntiForgeryToken]public IActionResult OcultarResposta(int id){var r=db.ForumRespostas.Find(id);if(r==null)return NotFound();r.Conteudo="[Comentário retirado pela administração.]";db.SaveChanges();return RedirectToAction(nameof(Moderacao));}
}