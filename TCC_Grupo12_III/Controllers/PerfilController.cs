using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TCC_Grupo12_III.Data;
namespace TCC_Grupo12_III.Controllers;
[Authorize]
public class PerfilController(AppDbContext db):Controller{
 int Uid=>int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 public IActionResult Index(){var u=db.Usuarios.Find(Uid);return u==null?NotFound():View(u);}
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> AtualizarDados(string? nome,string? email){
 var u=db.Usuarios.Find(Uid);if(u==null)return NotFound();
 nome=nome?.Trim();email=email?.Trim().ToLowerInvariant();
 if(string.IsNullOrWhiteSpace(nome)||nome.Length>100||string.IsNullOrWhiteSpace(email)||!new EmailAddressAttribute().IsValid(email)||db.Usuarios.Any(x=>x.Id!=Uid&&x.Email.ToLower()==email)){TempData["Erro"]="Confira o nome e informe um e-mail válido que ainda não esteja em uso.";return RedirectToAction(nameof(Index));}
 u.Nome=nome;u.Email=email;db.SaveChanges();
 await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(new ClaimsIdentity(new[]{new Claim(ClaimTypes.NameIdentifier,u.Id.ToString()),new Claim(ClaimTypes.Name,u.Nome),new Claim(ClaimTypes.Email,u.Email),new Claim(ClaimTypes.Role,u.IsAdmin?"Admin":"Estudante")},CookieAuthenticationDefaults.AuthenticationScheme)));
 TempData["Sucesso"]="Dados atualizados.";return RedirectToAction(nameof(Index));}
 [HttpPost,ValidateAntiForgeryToken]public IActionResult AlterarSenha(string? senhaAtual,string? novaSenha,string? confirmarSenha){
 var u=db.Usuarios.Find(Uid);if(u==null)return NotFound();
 if(string.IsNullOrWhiteSpace(senhaAtual)||!BCrypt.Net.BCrypt.Verify(senhaAtual,u.Senha)||novaSenha==null||novaSenha.Length<6||novaSenha!=confirmarSenha){TempData["Erro"]="Confira a senha atual e repita a nova senha, com pelo menos seis caracteres.";return RedirectToAction(nameof(Index));}
 u.Senha=BCrypt.Net.BCrypt.HashPassword(novaSenha);db.SaveChanges();TempData["Sucesso"]="Senha atualizada.";return RedirectToAction(nameof(Index));}
}