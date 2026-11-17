using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;
namespace TCC_Grupo12_III.Controllers;
public class CampanhasController(AppDbContext db):Controller {
 int Uid=>int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier),out var id)?id:0;
 public IActionResult Index()=>View(db.Campanhas.Where(c=>c.StatusAprovacao=="Aprovado"||c.StatusAprovacao=="").OrderByDescending(c=>c.Ativa).ThenBy(c=>c.Titulo).ToList());
 public IActionResult Detalhes(int id){
 var c=db.Campanhas.Find(id);if(c==null||(c.StatusAprovacao!="Aprovado"&&c.StatusAprovacao!=""&&!User.IsInRole("Admin")))return NotFound();
 ViewBag.Participando=db.CampanhaParticipacoes.Any(p=>p.CampanhaId==id&&p.UsuarioId==Uid);
 ViewBag.Mural=db.ForumTopicos.Include(t=>t.Usuario).Include(t=>t.Respostas).ThenInclude(r=>r.Usuario).Include(t=>t.Curtidas).Where(t=>t.Categoria=="campanha:"+id).OrderByDescending(t=>t.CriadoEm).ToList();
 return View(c);
 }
 [Authorize,HttpPost,ValidateAntiForgeryToken]public IActionResult Participar(int id){
 var c=db.Campanhas.Find(id);if(c==null||!c.Ativa||c.StatusAprovacao!="Aprovado")return BadRequest();
 if(!db.CampanhaParticipacoes.Any(p=>p.CampanhaId==id&&p.UsuarioId==Uid)){db.CampanhaParticipacoes.Add(new CampanhaParticipacao{CampanhaId=id,UsuarioId=Uid});db.SaveChanges();}
 TempData["Sucesso"]="Participação registrada. Confira as orientações da campanha.";return RedirectToAction(nameof(Detalhes),new{id});
 }
}