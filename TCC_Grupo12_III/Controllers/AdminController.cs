using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;
namespace TCC_Grupo12_III.Controllers;
[Authorize(Roles="Admin")]
public class AdminController(AppDbContext db):Controller {
 public IActionResult Index(){ViewBag.Contas=db.Usuarios.Count();ViewBag.Especies=db.Especies.Count();ViewBag.Aves=db.Especies.Count(e=>e.Categoria=="Aves");ViewBag.Terrestres=db.Especies.Count(e=>e.Categoria!="Aves");ViewBag.Pendentes=db.EstudosCientificos.Count(e=>e.StatusAprovacao=="Pendente");ViewBag.Avistamentos=db.Avistamentos.Count(a=>!a.Aprovado);return View();}
 public IActionResult Contas()=>View(db.Usuarios.OrderBy(u=>u.Nome).ToList());
 public IActionResult Create()=>View(new Usuario());
 [HttpPost,ValidateAntiForgeryToken] public IActionResult Create([Bind("Nome,Email,Senha,IsAdmin")]Usuario usuario){
 usuario.Email=(usuario.Email ?? "").Trim().ToLowerInvariant();
 if(db.Usuarios.Any(u=>u.Email.ToLower()==usuario.Email))ModelState.AddModelError("Email","Já existe uma conta com esse e-mail.");
 if(!ModelState.IsValid)return View(usuario);usuario.Senha=BCrypt.Net.BCrypt.HashPassword(usuario.Senha);db.Usuarios.Add(usuario);db.SaveChanges();TempData["Sucesso"]="Conta criada.";return RedirectToAction(nameof(Contas));}
 public IActionResult Edit(int id){var u=db.Usuarios.Find(id);return u==null?NotFound():View(new AdminUsuarioViewModel{Id=u.Id,Nome=u.Nome,Email=u.Email,IsAdmin=u.IsAdmin,IsBanido=u.IsBanido,Xp=u.Xp});}
 [HttpPost,ValidateAntiForgeryToken] public IActionResult Edit([Bind("Id,Nome,Email,IsAdmin,IsBanido,Xp,NovaSenha")]AdminUsuarioViewModel model){
 var u=db.Usuarios.Find(model.Id);if(u==null)return NotFound();
 var email=(model.Email ?? "").Trim().ToLowerInvariant();
 if(db.Usuarios.Any(x=>x.Email.ToLower()==email&&x.Id!=u.Id))ModelState.AddModelError("Email","E-mail já utilizado.");
 if(u.Id.ToString()==User.FindFirstValue(ClaimTypes.NameIdentifier)&&(!model.IsAdmin||model.IsBanido))ModelState.AddModelError("","Você não pode bloquear ou retirar a permissão da própria conta em uso.");
 if(!string.IsNullOrWhiteSpace(model.NovaSenha)&&model.NovaSenha.Length<6)ModelState.AddModelError("NovaSenha","Use pelo menos seis caracteres.");
 if(!ModelState.IsValid)return View(model);
 u.Nome=model.Nome.Trim();u.Email=email;u.IsAdmin=model.IsAdmin;u.IsBanido=model.IsBanido;u.Xp=Math.Max(0,model.Xp);
 if(!string.IsNullOrWhiteSpace(model.NovaSenha))u.Senha=BCrypt.Net.BCrypt.HashPassword(model.NovaSenha);
 // Os marcadores antigos são preservados como histórico e não concedem permissões.
 db.SaveChanges();TempData["Sucesso"]="Conta atualizada.";return RedirectToAction(nameof(Contas));}
}
