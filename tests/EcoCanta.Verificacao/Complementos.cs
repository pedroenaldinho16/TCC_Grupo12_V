using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;
class Complementos {
 public static async Task Run(string root){
 var output=Path.Combine(root,"documentacao","validacao");var dir=Path.Combine(output,"complementos-"+DateTime.Now.ToString("yyyyMMdd-HHmmss"));Directory.CreateDirectory(dir);
 var web=Path.Combine(root,"TCC_Grupo12_III");var path=Path.Combine(dir,"teste.db");
 using(var source=new SqliteConnection($"Data Source={Path.Combine(web,"app.db")};Mode=ReadOnly")){source.Open();using var dest=new SqliteConnection($"Data Source={path}");dest.Open();source.BackupDatabase(dest);}
 AppDbContext Db()=>new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite($"Data Source={path}").Options);
 TestApplication.PrepareCopy(path);
 var checks=new List<object>();void Check(bool ok,string name){checks.Add(new{teste=name,passou=ok});Console.WriteLine((ok?"OK ":"FALHOU ")+name);if(!ok)throw new Exception(name);}
 var password="Teste!"+Guid.NewGuid().ToString("N");var email=Guid.NewGuid().ToString("N")+"@teste.invalid";int uid;
 using(var db=Db()){var u=new Usuario{Nome="Validação administrativa",Email=email,Senha=BCrypt.Net.BCrypt.HashPassword(password),IsAdmin=true};db.Usuarios.Add(u);db.SaveChanges();uid=u.Id;foreach(var old in db.Usuarios.Where(x=>x.Id!=uid)){old.Nome="Usuário de demonstração "+old.Id;old.Email="demo"+old.Id+"@exemplo.invalid";}db.SaveChanges();}
 var psi=new ProcessStartInfo("dotnet"){WorkingDirectory=web,UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
 psi.ArgumentList.Add(TestApplication.Dll(root));psi.ArgumentList.Add("--urls");psi.ArgumentList.Add("http://127.0.0.1:5057");
 psi.Environment["ASPNETCORE_ENVIRONMENT"]="Development";psi.Environment["ConnectionStrings__DefaultConnection"]=$"Data Source={path}";psi.Environment["Logging__LogLevel__Default"]="Warning";psi.Environment["Logging__LogLevel__Microsoft"]="Warning";
 using var server=Process.Start(psi)!;var stdout=server.StandardOutput.ReadToEndAsync();var stderr=server.StandardError.ReadToEndAsync();
 try{
 using(var db=Db()){var u=db.Usuarios.Find(uid)!;u.Xp=490;db.SaveChanges();var principal=new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[]{new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier,uid.ToString())},"teste"));var mensagem=NivelHelper.Conceder(db,principal,35);Check(!mensagem.Contains("desbloqueado",StringComparison.OrdinalIgnoreCase),"XP não apresenta mensagem de desbloqueio do mapa");}
 var admin=new Session();for(var i=0;i<50;i++){try{if((await admin.Client.GetAsync("/")).StatusCode==HttpStatusCode.OK)break;}catch{}await Task.Delay(200);}await admin.Login(email,password);
var visitante=new Session();
 Check((await visitante.Client.GetAsync("/uploads/avistamentos/c796a908-8746-4c3a-a59f-dc940dbed36c.png")).StatusCode==HttpStatusCode.Forbidden,"Evidência legada bloqueada para visitante");
 Check((await admin.Client.GetAsync("/uploads/avistamentos/c796a908-8746-4c3a-a59f-dc940dbed36c.png")).StatusCode==HttpStatusCode.OK,"Evidência legada disponível à administração");
 var emailEstudante=Guid.NewGuid().ToString("N")+"@teste.invalid";
 await visitante.Post("/Cadastro",new(){["Nome"]="Estudante de validação",["Email"]=emailEstudante,["Senha"]=password},"/Cadastro");await visitante.Login(emailEstudante,password);
 Check((await visitante.Client.GetAsync("/uploads/avistamentos/c796a908-8746-4c3a-a59f-dc940dbed36c.png")).StatusCode==HttpStatusCode.Forbidden,"Evidência legada bloqueada para Estudante");
 
 Dictionary<string,string> Form(object o)=>o.GetType().GetProperties().Where(p=>p.PropertyType==typeof(string)||p.PropertyType==typeof(int)||p.PropertyType==typeof(bool)).ToDictionary(p=>p.Name,p=>p.GetValue(o)?.ToString()??"");
 Especie original;using(var db=Db())original=db.Especies.AsNoTracking().First();
 var species=Form(original);species["Id"]="0";species["Nome"]="Registro isolado de validação";species["ImagemUrl"]="";
 var created=await admin.Post("/AdminEspecies/Create",species,"/AdminEspecies/Create");Check(created.StatusCode==HttpStatusCode.Redirect,"Admin cadastra espécie com campos científicos opcionais vazios");
 int createdId;using(var db=Db())createdId=db.Especies.Single(e=>e.Nome=="Registro isolado de validação").Id;
 species["Id"]=createdId.ToString();species["ImportanciaEcologica"]="Campo preparado para validação científica.";species["ConservacaoGlobal"]="Pendente de validação";species["ConservacaoNacional"]="Pendente de validação";species["ConservacaoRegional"]="Pendente de validação";species["FonteCientifica"]="Referência em revisão.";
 var edited=await admin.Post("/AdminEspecies/Edit",species,"/AdminEspecies/Edit/"+createdId);
 Check(edited.StatusCode==HttpStatusCode.Redirect,"Admin edita ficha com avaliações por abrangência");
 using(var db=Db()){var e=db.Especies.Find(createdId)!;Check(e.FonteCientifica=="Referência em revisão."&&e.ConservacaoNacional=="Pendente de validação","Campos científicos persistidos");db.Especies.Remove(e);db.SaveChanges();}
 var novaConta=Guid.NewGuid().ToString("N")+"@teste.invalid";
 var createUser=await admin.Post("/Admin/Create",new(){["Nome"]="Conta isolada",["Email"]=novaConta,["Senha"]=password,["IsAdmin"]="false"},"/Admin/Create");
 Check(createUser.StatusCode==HttpStatusCode.Redirect,"Criação administrativa de Estudante");
 int novoId;using(var db=Db())novoId=db.Usuarios.Single(u=>u.Email==novaConta).Id;
 var editUser=await admin.Post("/Admin/Edit",new(){["Id"]=novoId.ToString(),["Nome"]="Conta isolada",["Email"]=novaConta,["IsAdmin"]="true",["IsBanido"]="false",["Xp"]="0"},"/Admin/Edit/"+novoId);
 Check(editUser.StatusCode==HttpStatusCode.Redirect,"Administrador concede permissão autorizada");
 using(var db=Db())Check(db.Usuarios.Find(novoId)!.IsAdmin,"Perfil administrativo persistido");
 var campaign=await admin.Post("/AdminCampanhas/Create",new(){["Titulo"]="Campanha de validação isolada",["Descricao"]="Registro de teste em cópia do banco.",["Periodo"]="Demonstração",["Meta"]="10",["Ativa"]="true",["StatusAprovacao"]="Aprovado"},"/AdminCampanhas/Create");
 Check(campaign.StatusCode==HttpStatusCode.Redirect,"Criação administrativa de campanha");
 using(var db=Db()){var c=db.Campanhas.Single(c=>c.Titulo=="Campanha de validação isolada");Check(c.StatusAprovacao=="Aprovado","Campanha administrativa publicada sem proposta estudantil");db.Campanhas.Remove(c);db.SaveChanges();}
 var question=await admin.Post("/AdminJornada/Edit",new(){["Id"]="0",["Desafio"]="2",["Enunciado"]="Questão isolada de validação",["OpcaoA"]="A",["OpcaoB"]="B",["OpcaoC"]="C",["OpcaoD"]="D",["Correta"]="1",["Explicacao"]="Explicação de validação.",["Fonte"]="Referência de teste.",["Ativa"]="false"},"/AdminJornada/Edit");
 Check(question.StatusCode==HttpStatusCode.Redirect,"Editor salva questão com explicação e fonte");
 using(var db=Db()){var q=db.JornadaPerguntas.Single(q=>q.Enunciado=="Questão isolada de validação");Check(!q.Ativa,"Questão em revisão não é usada em novas tentativas");db.JornadaPerguntas.Remove(q);db.SaveChanges();}
 await admin.Post("/Faq/NovaPergunta",new(){["assunto"]="Dúvida de validação",["categoria"]="Geral",["mensagem"]="Dúvida isolada."},"/Faq");
 int ticket;using(var db=Db())ticket=db.FaqTickets.Where(t=>t.UsuarioId==uid).OrderByDescending(t=>t.Id).First().Id;
 var response=await admin.Post("/AdminFaq/Responder",new(){["ticketId"]=ticket.ToString(),["conteudo"]="Resposta de validação.",["novoStatus"]="Respondido"},"/AdminFaq/Atender/"+ticket);
 Check(response.StatusCode==HttpStatusCode.Redirect,"Administração responde chamado");
 using(var db=Db())Check(db.FaqMensagens.Any(m=>m.FaqTicketId==ticket&&m.IsEquipeModOuAdmin),"Resposta de suporte persistida");
 using(var db=Db()){db.JornadaTentativas.Add(new JornadaTentativa{UsuarioId=uid,Desafio=1,Aprovada=true,ConcluidaEm=DateTime.UtcNow,Pontos=1,Total=1});db.SaveChanges();}
 var wrong=await admin.Post("/Jornada/Iniciar",new(){["desafio"]="2"},"/Jornada");Check(wrong.StatusCode==HttpStatusCode.Redirect,"Quiz do Guardião disponível para teste por teclado");
 int attempt;using(var db=Db())attempt=db.JornadaTentativas.Where(t=>t.UsuarioId==uid).OrderByDescending(t=>t.Id).First().Id;
 using var chrome=Process.Start(new ProcessStartInfo(@"C:\Program Files\Google\Chrome\Application\chrome.exe"){UseShellExecute=false,CreateNoWindow=true,Arguments="--headless=new --disable-gpu --hide-scrollbars --no-first-run --remote-debugging-port=9224 --user-data-dir=\""+Path.Combine(dir,"chrome")+"\" about:blank"})!;
 try{
  using var http=new HttpClient();string? ws=null;for(var i=0;i<50;i++){try{using var json=JsonDocument.Parse(await http.GetStringAsync("http://127.0.0.1:9224/json/list"));ws=json.RootElement.EnumerateArray().First(x=>x.GetProperty("type").GetString()=="page").GetProperty("webSocketDebuggerUrl").GetString();if(ws!=null)break;}catch{}await Task.Delay(200);}
  using var cdp=new Cdp();await cdp.Connect(ws!);await cdp.Call("Page.enable",new{});await cdp.Call("Network.enable",new{});
  foreach(Cookie c in admin.Cookies.GetCookies(new Uri("http://127.0.0.1:5057")))await cdp.Call("Network.setCookie",new{name=c.Name,value=c.Value,url="http://127.0.0.1:5057"});
  async Task<JsonElement> Eval(string js){var r=await cdp.Call("Runtime.evaluate",new{expression=js,returnByValue=true,awaitPromise=true});return r.GetProperty("result").GetProperty("value");}
  async Task Capture(string name,string url,int width){await cdp.Call("Emulation.setDeviceMetricsOverride",new{width,height=960,deviceScaleFactor=1,mobile=width<650});await cdp.Call("Page.navigate",new{url="http://127.0.0.1:5057"+url});for(var i=0;i<50;i++){await Task.Delay(100);try{if((await Eval("document.readyState==='complete'")).GetBoolean())break;}catch{}}await Eval("document.fonts.ready.then(()=>true)");await Eval("Promise.all([...document.images].map(i=>{i.loading='eager';return i.decode().catch(()=>{});})).then(()=>true)");Check((await Eval("document.documentElement.scrollWidth<=innerWidth+1")).GetBoolean(),"Sem transbordamento "+name);var shot=await cdp.Call("Page.captureScreenshot",new{format="png",captureBeyondViewport=true,fromSurface=true});File.WriteAllBytes(Path.Combine(root,"documentacao","capturas",name+".png"),Convert.FromBase64String(shot.GetProperty("data").GetString()!));}
  await Capture("admin-especies-celular","/AdminEspecies",390);
  Check((await Eval("getComputedStyle(document.querySelector('.ec-table-stacked td')).display==='grid'")).GetBoolean(),"Tabela administrativa adaptada como registros legíveis no celular");
  await Capture("formulario-especie-celular","/AdminEspecies/Edit/"+original.Id,390);
  await Capture("desafio-teclado","/Jornada/Jogar/"+attempt,390);
  await Eval("document.querySelector('input[type=radio]').focus();true");
  await cdp.Call("Input.dispatchKeyEvent",new{type="keyDown",key=" ",code="Space",windowsVirtualKeyCode=32});await cdp.Call("Input.dispatchKeyEvent",new{type="keyUp",key=" ",code="Space",windowsVirtualKeyCode=32});
  Check((await Eval("document.querySelector('input[type=radio]').checked")).GetBoolean(),"Alternativa selecionada pelo teclado");
  await Eval("document.querySelector('form[data-submit-once] button[type=submit]').focus();true");
  await cdp.Call("Input.dispatchKeyEvent",new{type="keyDown",key="Enter",code="Enter",windowsVirtualKeyCode=13,text="\r",unmodifiedText="\r"});await cdp.Call("Input.dispatchKeyEvent",new{type="keyUp",key="Enter",code="Enter",windowsVirtualKeyCode=13,text="\r",unmodifiedText="\r"});for(var i=0;i<30;i++){await Task.Delay(200);try{if((await Eval("location.pathname.toLowerCase().includes(\"feedback\")")).GetBoolean())break;}catch{}}
  Check((await Eval("location.pathname.toLowerCase().includes('feedback')")).GetBoolean(),"Resposta enviada pelo teclado e feedback exibido");
  using(var db=Db())Check(db.JornadaTentativas.Find(attempt)!.Respostas.Count==1,"Resposta pelo teclado persistida");
 }finally{if(!chrome.HasExited)chrome.Kill(true);}
 File.WriteAllText(Path.Combine(output,"resultado-complementar.json"),JsonSerializer.Serialize(new{total=checks.Count,checks},new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine("COMPLEMENTOS CONCLUÍDOS: "+checks.Count);
 }catch(Exception e){File.WriteAllText(Path.Combine(output,"falha-complementar.txt"),e.ToString());Console.Error.WriteLine(e);Environment.ExitCode=1;}
 finally{if(!server.HasExited)server.Kill(true);await server.WaitForExitAsync();File.WriteAllText(Path.Combine(dir,"servidor.log"),await stdout+await stderr);}
 }
}
