using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TCC_Grupo12_III.Data;
namespace TCC_Grupo12_III.Controllers;
[Authorize(Roles="Admin")]
public class AdminAvistamentosController(AppDbContext db,IWebHostEnvironment env,IConfiguration config):Controller {
 public IActionResult Index()=>View(db.Avistamentos.Include(a=>a.Especie).Include(a=>a.Midias).OrderByDescending(a=>a.CriadoEm).ToList());
 [HttpPost,ValidateAntiForgeryToken] public IActionResult Aprovar(int id,bool aprovado){var a=db.Avistamentos.Find(id);if(a==null)return NotFound();a.Aprovado=aprovado;db.SaveChanges();TempData["Sucesso"]=aprovado?"Registro aprovado.":"Registro retirado do mapa; dados preservados.";return RedirectToAction(nameof(Index));}
 public IActionResult Evidencia(int id){var m=db.AvistamentoMidias.Find(id);if(m==null||!m.Caminho.StartsWith("/evidencias/"))return NotFound();var nome=Path.GetFileName(m.Caminho);var file=Path.Combine(config["DataDirectory"] ?? Path.Combine(env.ContentRootPath,"App_Data"),"avistamentos",nome);if(!System.IO.File.Exists(file))return NotFound();return PhysicalFile(file,"application/octet-stream",nome);}
}
