using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TCC_Grupo12_III.Services;
namespace TCC_Grupo12_III.Filters;
public class RequerNivelMapaAttribute:TypeFilterAttribute {
 public RequerNivelMapaAttribute():base(typeof(RequerNivelMapaFilter)){}
}
public class RequerNivelMapaFilter(JornadaService jornada):IActionFilter {
 public void OnActionExecuting(ActionExecutingContext context) {
  if(!jornada.MapaLiberado(context.HttpContext.User))
   context.Result=new RedirectToActionResult("Mapa","Jornada",null);
 }
 public void OnActionExecuted(ActionExecutedContext context){}
}
