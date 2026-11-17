using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;
using TCC_Grupo12_III.Services;
namespace TCC_Grupo12_III.Controllers;
public class EducacaoController(AppDbContext db):Controller {
 int Uid=>int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier),out var id)?id:0;
 public IActionResult Index(string? busca=null){ViewBag.Meus=db.EstudosCientificos.Where(x=>x.UsuarioId==Uid&&x.StatusAprovacao!="Aprovado").OrderByDescending(x=>x.Id).ToList();return View(EducacaoService.ObterTodos(db,null,busca));}
 public IActionResult Artigo(string? slug){var a=EducacaoService.ObterTodos(db).FirstOrDefault(a=>a.Slug==slug);if(a==null)return NotFound();ViewBag.Conteudo=WebUtility.HtmlDecode(Regex.Replace(a.ConteudoHtml,"<[^>]+>"," "));return View(a);}
 [Authorize]public IActionResult Publicar()=>View(new VozForm());
 [Authorize,HttpPost,ValidateAntiForgeryToken]public IActionResult Publicar(VozForm f){
 if(!ModelState.IsValid)return View(f);
 var u=db.Usuarios.Find(Uid)!;
 db.EstudosCientificos.Add(new EstudoCientifico{UsuarioId=u.Id,Titulo=f.Titulo,Subtitulo="Voz Estudantil",Resumo=f.Resumo,ConteudoHtml=WebUtility.HtmlEncode(f.Conteudo).Replace("\n","<br>"),ReferenciasTexto=f.Referencias,Autor=u.Nome,Categoria="Educação ambiental",Slug="voz-"+Guid.NewGuid().ToString("N"),StatusAprovacao="Pendente"});
 db.SaveChanges();TempData["Sucesso"]="Texto enviado para revisão. A publicação depende de aprovação administrativa.";return RedirectToAction(nameof(Index));}
 [Authorize(Roles="Admin")]public IActionResult Revisar()=>View(db.EstudosCientificos.Include(x=>x.Usuario).OrderByDescending(x=>x.DataSubmissao).ToList());
 [Authorize(Roles="Admin"),HttpPost,ValidateAntiForgeryToken]public IActionResult Moderar(int id,bool aprovar,string? observacao){
 var e=db.EstudosCientificos.Find(id);if(e==null)return NotFound();e.StatusAprovacao=aprovar?"Aprovado":"Reprovado";e.DataResposta=DateTime.Now;e.ObservacoesAdmin=observacao?.Trim();db.SaveChanges();TempData["Sucesso"]="Revisão registrada.";return RedirectToAction(nameof(Revisar));}
}