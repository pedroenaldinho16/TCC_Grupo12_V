using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;
namespace TCC_Grupo12_III.Controllers;
public class LoginController(AppDbContext db):Controller {
 public IActionResult Index(string? returnUrl=null){ViewBag.ReturnUrl=Url.IsLocalUrl(returnUrl)?returnUrl:"";return View(new LoginViewModel());}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Index(LoginViewModel model,string? returnUrl=null){
  ViewBag.ReturnUrl=Url.IsLocalUrl(returnUrl)?returnUrl:"";
  if(!ModelState.IsValid)return View(model);
  var email=model.Email.Trim().ToLowerInvariant();
  var u=db.Usuarios.FirstOrDefault(u=>u.Email.ToLower()==email);
  var valida=false;try{valida=u!=null&&BCrypt.Net.BCrypt.Verify(model.Senha,u.Senha);}catch(BCrypt.Net.SaltParseException){}
  if(!valida||u!.IsBanido){ModelState.AddModelError("","Não foi possível entrar. Confira as credenciais ou procure a equipe responsável.");return View(model);}
  var claims=new[]{new Claim(ClaimTypes.NameIdentifier,u.Id.ToString()),new Claim(ClaimTypes.Name,u.Nome),new Claim(ClaimTypes.Email,u.Email),new Claim(ClaimTypes.Role,u.IsAdmin?"Admin":"Estudante")};
  await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,new ClaimsPrincipal(new ClaimsIdentity(claims,CookieAuthenticationDefaults.AuthenticationScheme)));
  if(Url.IsLocalUrl(returnUrl))return LocalRedirect(returnUrl!);
  return RedirectToAction("Index",u.IsAdmin?"Admin":"Home");
 }
 [Authorize,HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Logout(){await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);return RedirectToAction("Index","Home");}
}
