using Microsoft.AspNetCore.Mvc;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;
namespace TCC_Grupo12_III.Controllers;
public class CadastroController(AppDbContext db):Controller {
 public IActionResult Index()=>View(new Usuario());
 [HttpPost,ValidateAntiForgeryToken] public IActionResult Index([Bind("Nome,Email,Senha")] Usuario usuario){
  usuario.Nome=(usuario.Nome ?? "").Trim();usuario.Email=(usuario.Email ?? "").Trim().ToLowerInvariant();
  if(db.Usuarios.Any(u=>u.Email.ToLower()==usuario.Email))ModelState.AddModelError("Email","Já existe um cadastro com esse e-mail.");
  if(!ModelState.IsValid)return View(usuario);
  usuario.Senha=BCrypt.Net.BCrypt.HashPassword(usuario.Senha);
  usuario.IsAdmin=false;usuario.IsModerador=false;usuario.IsJornalista=false;usuario.Xp=0;
  db.Usuarios.Add(usuario);db.SaveChanges();
  TempData["Sucesso"]="Conta de estudante criada! Entre para começar sua jornada.";return RedirectToAction("Index","Login");
 }
}
