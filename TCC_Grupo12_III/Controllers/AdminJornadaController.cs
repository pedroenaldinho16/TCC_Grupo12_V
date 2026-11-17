using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;
namespace TCC_Grupo12_III.Controllers;
[Authorize(Roles="Admin")]
public class AdminJornadaController(AppDbContext db):Controller {
 public IActionResult Index()=>View(db.JornadaPerguntas.OrderBy(p=>p.Desafio).ThenBy(p=>p.Id).ToList());
 public IActionResult Edit(int? id){var q=id.HasValue?db.JornadaPerguntas.Find(id):new JornadaPergunta{Desafio=2,Correta=1};if(q==null)return NotFound();if(q.Desafio!=2){TempData["Aviso"]="Questões da versão anterior são preservadas para consulta. Os jogos de cartas e exploração não utilizam essas perguntas.";return RedirectToAction(nameof(Index));}return View(q);}
 [HttpPost,ValidateAntiForgeryToken]public IActionResult Edit(JornadaPergunta p){
 if(p.Desafio!=2 || (p.Id!=0 && db.JornadaPerguntas.Any(q=>q.Id==p.Id && q.Desafio!=2)))return BadRequest();
 if(!string.IsNullOrWhiteSpace(p.ImagemUrl)&&(!p.ImagemUrl.StartsWith("/img/")||p.ImagemUrl.Contains("..")))ModelState.AddModelError("ImagemUrl","Use uma imagem local em /img/.");
 if(!ModelState.IsValid)return View(p);
 if(p.Id==0)db.JornadaPerguntas.Add(p);else{var q=db.JornadaPerguntas.Find(p.Id);if(q==null)return NotFound();db.Entry(q).CurrentValues.SetValues(p);}
 db.SaveChanges();TempData["Sucesso"]="Questão salva. Tentativas já iniciadas preservam o conteúdo original.";return RedirectToAction(nameof(Index));}
}
