using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
namespace TCC_Grupo12_III.Filters;
public class EscopoMeta1Filter:IActionFilter {
 public void OnActionExecuting(ActionExecutingContext c){
 var controller=c.RouteData.Values["controller"]?.ToString();
 var destinos=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase){{"Jogo","/Jornada"},{"Nivel","/Jornada/Progresso"},{"Denuncia","/Home/Preservar"},{"AdminDenuncias","/Admin"},{"AdminNiveis","/AdminJornada"}};
 if(controller!=null&&destinos.TryGetValue(controller,out var destino)){
 c.Result=HttpMethods.IsGet(c.HttpContext.Request.Method)?new RedirectResult(destino):new StatusCodeResult(StatusCodes.Status410Gone);}
 }
 public void OnActionExecuted(ActionExecutedContext c){}
}