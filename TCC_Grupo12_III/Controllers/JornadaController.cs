using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;
using TCC_Grupo12_III.Services;
namespace TCC_Grupo12_III.Controllers;
public class JornadaController(AppDbContext db,JornadaService jornada,JornadaJogosService jogos):Controller {
 public IActionResult Index(bool bloqueado=false) {ViewBag.Bloqueado=bloqueado;return View(jornada.Historico(User));}
 [HttpGet("/Mapa")]
 public IActionResult Mapa() {
  if(jornada.MapaLiberado(User))return RedirectToAction("Index","Avistamentos");
  return View(jornada.Historico(User));
 }
 [Authorize] public IActionResult Progresso() => View(jornada.Historico(User));
 [Authorize,HttpPost,ValidateAntiForgeryToken]
 public IActionResult Iniciar(int desafio,bool atualizar=false) {
  if(!jornada.PodeIniciar(User,desafio)){TempData["Aviso"]="Conclua a etapa anterior para continuar na jornada.";return RedirectToAction(nameof(Index));}
  var uid=jornada.UsuarioId(User);
  var tentativa=db.JornadaTentativas.FirstOrDefault(t=>t.UsuarioId==uid&&t.Desafio==desafio&&t.ConcluidaEm==null);
  if(tentativa!=null && (tentativa.Jogo is not null || desafio==2 || !atualizar))return RedirectToAction(nameof(Jogar),new{id=tentativa.Id});
  if(desafio is 1 or 3) {
   var jogo=jogos.Criar(desafio);
   if(desafio==1 && jogo.Cartas.Count<2){TempData["Aviso"]="As cartas precisam de pelo menos duas espécies com fotografias locais. A equipe está preparando o acervo.";return RedirectToAction(nameof(Index));}
   using var transaction=db.Database.BeginTransaction();
   if(tentativa!=null){
    // O estudante confirma a transição. Respostas e questões anteriores são preservadas.
    tentativa.RespostasJson=JsonSerializer.Serialize(new{Respostas=tentativa.Respostas,EncerradaNaAtualizacao=true});
    tentativa.ConcluidaEm=DateTime.UtcNow;tentativa.Aprovada=false;db.SaveChanges();
   }
   tentativa=new JornadaTentativa{UsuarioId=uid,Desafio=desafio,Total=JornadaJogosService.Total(jogo),QuestoesJson=JsonSerializer.Serialize(jogo),RespostasJson=JsonSerializer.Serialize(new EstadoJornadaJogo())};
   db.JornadaTentativas.Add(tentativa);db.SaveChanges();transaction.Commit();
   return RedirectToAction(nameof(Jogar),new{id=tentativa.Id});
  }
  var questoes=db.JornadaPerguntas.AsNoTracking().Where(p=>p.Desafio==desafio&&p.Ativa).OrderBy(p=>p.Id).ToList();
  if(questoes.Count==0){TempData["Aviso"]="Este desafio está em revisão. Explore as fichas enquanto o conteúdo é preparado.";return RedirectToAction(nameof(Index));}
  // Snapshots preservam a avaliação mesmo se um administrador editar perguntas.
  tentativa=new JornadaTentativa{UsuarioId=uid,Desafio=desafio,Total=questoes.Count,QuestoesJson=JsonSerializer.Serialize(questoes)};
  db.JornadaTentativas.Add(tentativa);db.SaveChanges();
  return RedirectToAction(nameof(Jogar),new{id=tentativa.Id});
 }
 private JornadaTentativa? Obter(int id){var uid=jornada.UsuarioId(User);return db.JornadaTentativas.FirstOrDefault(t=>t.Id==id&&t.UsuarioId==uid);}
 [Authorize] public IActionResult Jogar(int id) {
  var t=Obter(id);if(t==null)return NotFound();
  if(t.Jogo is { } jogo){
   if(t.ConcluidaEm!=null && jogo.Tipo!="missao")return RedirectToAction(nameof(Resultado),new{id});
   return View(jogo.Tipo=="cartas"?"Cartas":"Missao",new JornadaJogoViewModel(t,jogo,t.EstadoJogo));
  }
  if(t.ConcluidaEm!=null)return RedirectToAction(nameof(Resultado),new{id});
  if(t.Desafio is 1 or 3)return View("AtualizacaoJogo",t);
  var pos=t.Respostas.Count;return View(new JornadaQuestaoViewModel(t,t.Questoes[pos],pos,false));
 }
 [Authorize,HttpPost,ValidateAntiForgeryToken]
 public IActionResult Responder(int id,int posicao,int? resposta) {
  var t=Obter(id);if(t==null)return NotFound();
  if(t.ConcluidaEm!=null)return RedirectToAction(nameof(Resultado),new{id});
  if(t.Jogo is not null || t.Desafio!=2)return RedirectToAction(nameof(Jogar),new{id});
  var respostas=t.Respostas;
  if(posicao!=respostas.Count)return RedirectToAction(nameof(Jogar),new{id});
  if(resposta is null or <1 or >4){TempData["Aviso"]="Selecione uma alternativa para continuar.";return RedirectToAction(nameof(Jogar),new{id});}
  var jsonAnterior=t.RespostasJson;
  respostas.Add(resposta.Value);
  var novoJson=JsonSerializer.Serialize(respostas);
  var pontos=respostas.Select((r,i)=>r==t.Questoes[i].Correta).Count(c=>c);
  var final=respostas.Count==t.Total;
  // Compare-and-swap impede respostas simultâneas e envio duplicado de pontuação.
  var changed=db.JornadaTentativas.Where(x=>x.Id==id&&x.UsuarioId==t.UsuarioId&&x.RespostasJson==jsonAnterior&&x.ConcluidaEm==null)
   .ExecuteUpdate(s=>s.SetProperty(x=>x.RespostasJson,novoJson).SetProperty(x=>x.Pontos,pontos)
   .SetProperty(x=>x.ConcluidaEm,final?(DateTime?)DateTime.UtcNow:null).SetProperty(x=>x.Aprovada,final&&pontos*100>=t.Total*70));
  if(changed==0)return RedirectToAction(nameof(Jogar),new{id});
  return RedirectToAction(nameof(Feedback),new{id});
 }
 [Authorize,HttpPost,ValidateAntiForgeryToken]
 public IActionResult Agir(int id,int revisao,string acao,string alvo="",string ferramenta="observar",string destino="",bool? aberta=null) {
  var t=Obter(id);if(t==null)return NotFound();
  if(t.ConcluidaEm!=null)return DestinoJogo(t,true);
  if(t.Jogo is not { } jogo)return BadRequest();
  var estado=t.EstadoJogo;
  if(estado.Revisao!=revisao)return DestinoJogo(t,false,409);
  var mensagem=JornadaJogosService.Agir(jogo,estado,acao,alvo,ferramenta,destino,aberta);
  if(mensagem is null)return BadRequest();
  var novoJson=JsonSerializer.Serialize(estado);
  var pontos=JornadaJogosService.Pontos(jogo,estado);
  var final=pontos==t.Total;
  var mudou=db.JornadaTentativas.Where(x=>x.Id==t.Id && x.UsuarioId==t.UsuarioId && x.RespostasJson==t.RespostasJson && x.ConcluidaEm==null)
   .ExecuteUpdate(s=>s.SetProperty(x=>x.RespostasJson,novoJson).SetProperty(x=>x.Pontos,pontos)
    .SetProperty(x=>x.Aprovada,final).SetProperty(x=>x.ConcluidaEm,final?(DateTime?)DateTime.UtcNow:null));
  if(mudou==0)return DestinoJogo(t,false,409);
  if(jogo.Tipo=="cartas" && acao=="estudar" && Request.Headers.Accept.ToString().Contains("application/json"))
   return Json(new{revisao=estado.Revisao,cartasEstudadas=estado.Estudadas,cartaAberta=estado.CartaAberta,mensagem});
  TempData[$"JogoFeedback-{t.Id}"]=mensagem;
  return DestinoJogo(t,final);
 }
 private IActionResult DestinoJogo(JornadaTentativa tentativa,bool final,int status=200) {
  var resultado=final && tentativa.Jogo?.Tipo!="missao";
  var url=Url.Action(resultado?nameof(Resultado):nameof(Jogar),new{id=tentativa.Id});
  if(Request.Headers.Accept.ToString().Contains("application/json"))return new JsonResult(new{url,concluida=resultado}){StatusCode=status};
  return Redirect(url!);
 }
 [Authorize] public IActionResult Feedback(int id) {var t=Obter(id);if(t==null)return NotFound();if(t.Jogo is not null || (t.ConcluidaEm==null && t.Desafio!=2))return RedirectToAction(nameof(Jogar),new{id});if(t.Respostas.Count==0)return NotFound();var pos=t.Respostas.Count-1;return View("Jogar",new JornadaQuestaoViewModel(t,t.Questoes[pos],pos,true));}
 [Authorize] public IActionResult Resultado(int id) {var t=Obter(id);if(t==null)return NotFound();if(t.ConcluidaEm==null)return RedirectToAction(nameof(Jogar),new{id});return View(t);}
}
