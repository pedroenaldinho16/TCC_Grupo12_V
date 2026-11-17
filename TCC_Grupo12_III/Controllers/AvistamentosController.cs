using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Filters;
using TCC_Grupo12_III.Models;
namespace TCC_Grupo12_III.Controllers;
[Authorize,RequerNivelMapa]
public class AvistamentosController(AppDbContext db,IWebHostEnvironment env,IConfiguration config):Controller {
 int Uid=>int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 public IActionResult Index(){
  var itens=db.Avistamentos.AsNoTracking().Include(a=>a.Especie).Where(a=>a.Aprovado).OrderByDescending(a=>a.DataAvistamento).ToList();
  return View(new AvistamentoMapaViewModel{PodeRegistrar=true,EstaAutenticado=true,Especies=db.Especies.OrderBy(e=>e.Nome).ToList(),
   Avistamentos=itens.Select(a=>new AvistamentoMapaItem{Id=a.Id,NomeAnimal=a.NomeAnimal,NomeCientifico=a.Especie?.NomeCientifico,EspecieId=a.EspecieId,
   Latitude=User.IsInRole("Admin")?a.Latitude:Math.Round(a.Latitude/0.02)*0.02,Longitude=User.IsInRole("Admin")?a.Longitude:Math.Round(a.Longitude/0.02)*0.02,
   DataAvistamento=a.DataAvistamento,Quantidade=a.Quantidade,NomeObservador="Estudante EcoCanta"}).ToList()});
 }
 public IActionResult Registrar(int? especieId=null)=>View(Montar(new NovoAvistamentoForm{EspecieId=especieId,NomeObservador=User.Identity?.Name??"",DataAvistamento=DateTime.Today}));
 public IActionResult Criar(int? especieId=null)=>RedirectToAction(nameof(Registrar),new{especieId});
 AvistamentoRegistroViewModel Montar(NovoAvistamentoForm f)=>new(){Form=f,EspeciesPokedex=db.Especies.OrderBy(e=>e.Nome).ToList()};
 [HttpPost,ValidateAntiForgeryToken,RequestSizeLimit(80000000)]
 public async Task<IActionResult> Registrar(NovoAvistamentoForm form){
  var e=await db.Especies.FindAsync(form.EspecieId??0);
  if(e==null)ModelState.AddModelError("Form.EspecieId","Selecione uma espécie válida.");
  double lat=0,lon=0;
  if(!double.TryParse(form.Latitude?.Replace(',','.'),NumberStyles.Float,CultureInfo.InvariantCulture,out lat)||!double.TryParse(form.Longitude?.Replace(',','.'),NumberStyles.Float,CultureInfo.InvariantCulture,out lon)||!LimitesSerraCantareira.Contem(lat,lon))ModelState.AddModelError("Form.Latitude","Marque um ponto dentro dos limites da Serra da Cantareira.");
  if(form.DataAvistamento.Date>DateTime.Today)ModelState.AddModelError("Form.DataAvistamento","A data não pode estar no futuro.");
  var fotos=(form.Fotos??new()).Where(f=>f.Length>0).ToList();if(form.Foto?.Length>0)fotos.Add(form.Foto);
  var videos=(form.Videos??new()).Where(f=>f.Length>0).ToList();
  if(fotos.Count+videos.Count==0)ModelState.AddModelError("Form.Fotos","Envie ao menos uma fotografia ou vídeo.");
  if(fotos.Count+videos.Count>5)ModelState.AddModelError("Form.Fotos","Envie até cinco arquivos.");
  foreach(var f in fotos)if(f.Length>10*1024*1024||!new[]{".jpg",".jpeg",".png",".webp"}.Contains(Path.GetExtension(f.FileName).ToLowerInvariant()))ModelState.AddModelError("Form.Fotos","Use imagens JPG, PNG ou WebP de até 10 MB.");
  foreach(var f in videos)if(f.Length>60*1024*1024||!new[]{".mp4",".webm",".ogv"}.Contains(Path.GetExtension(f.FileName).ToLowerInvariant()))ModelState.AddModelError("Form.Videos","Use vídeos MP4, WebM ou OGV de até 60 MB.");
  if(!ModelState.IsValid)return View(Montar(form));
  var registro=new Avistamento{EspecieId=e!.Id,NomeAnimal=e.Nome,Latitude=lat,Longitude=lon,UsuarioId=Uid,NomeObservador=User.Identity!.Name!,DataAvistamento=form.DataAvistamento.Date,Quantidade=form.Quantidade,Observacoes=form.Observacoes,Aprovado=false};
  // Evidências ficam fora de wwwroot e disponíveis somente para a administração.
  var pasta=Path.Combine(config["DataDirectory"] ?? Path.Combine(env.ContentRootPath,"App_Data"),"avistamentos");Directory.CreateDirectory(pasta);
  foreach(var f in fotos.Concat(videos)){
   var nome=Guid.NewGuid().ToString("N")+Path.GetExtension(f.FileName).ToLowerInvariant();
   await using(var output=System.IO.File.Create(Path.Combine(pasta,nome)))await f.CopyToAsync(output);
   registro.Midias.Add(new AvistamentoMidia{Caminho="/evidencias/"+nome,Tipo=fotos.Contains(f)?"Foto":"Video"});
  }
  registro.FotoPath=registro.Midias.FirstOrDefault(m=>m.Tipo=="Foto")?.Caminho;
  db.Avistamentos.Add(registro);await db.SaveChangesAsync();TempData["Sucesso"]="Registro recebido para análise. Após aprovação, o mapa mostrará apenas a região aproximada.";return RedirectToAction(nameof(Index));
 }
}
