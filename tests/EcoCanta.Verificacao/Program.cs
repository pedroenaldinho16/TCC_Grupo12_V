using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;

var root=Path.GetFullPath(args.Length>0?args[0]:Path.Combine(AppContext.BaseDirectory,"../../../../../"));
if(args.Contains("--backup-jogos")){BackupJogos.Run(root);return;}
if(args.Contains("--preservacao-jogos")){BackupJogos.Verificar(root);return;}
if(args.Contains("--preservacao-interacoes")){BackupJogos.Verificar(root,"interacoes-20261114");return;}
if(args.Contains("--preservacao-correcoes")){BackupJogos.Verificar(root,"correcoes-20261115");return;}
if(args.Contains("--jogos")){await JogosEducativos.Run(root);return;}
if(args.Contains("--interacoes")){await JogosEducativos.Run(root,true);return;}
if(args.Contains("--confiabilidade")){await JogosEducativos.Run(root,true,true);return;}
if(args.Contains("--revisao-interface")){await RevisaoInterface.Run(root);return;}
if(args.Contains("--diagnostico")){Diagnostico.Run(root,args.Contains("--restaurar-historico"));return;}
if(args.Contains("--complementos")){await Complementos.Run(root);return;}
var web=Path.Combine(root,"TCC_Grupo12_III");
var output=Path.Combine(root,"documentacao","validacao");Directory.CreateDirectory(output);
var fixture=Path.Combine(output,"execucao-"+DateTime.Now.ToString("yyyyMMdd-HHmmss"));Directory.CreateDirectory(fixture);
var dbPath=Path.Combine(fixture,"teste.db");
using(var source=new SqliteConnection($"Data Source={Path.Combine(web,"app.db")};Mode=ReadOnly")){
 source.Open();using var target=new SqliteConnection($"Data Source={dbPath}");target.Open();source.BackupDatabase(target);
}
AppDbContext Db()=>new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite($"Data Source={dbPath}").Options);
TestApplication.PrepareCopy(dbPath);
JornadaTentativa Read(int id){using var db=Db();return db.JornadaTentativas.AsNoTracking().Single(t=>t.Id==id);}
var checks=new List<object>();
void Check(bool pass,string name){checks.Add(new{teste=name,passou=pass});Console.WriteLine((pass?"OK ":"FALHOU ")+name);if(!pass)throw new Exception(name);}
var password="Teste!"+Guid.NewGuid().ToString("N");
var adminEmail=Guid.NewGuid().ToString("N")+"@teste.invalid";
var legacyEmail=Guid.NewGuid().ToString("N")+"@teste.invalid";
using(var db=Db()){
 db.Usuarios.Add(new Usuario{Nome="Admin de validação",Email=adminEmail,Senha=BCrypt.Net.BCrypt.HashPassword(password),IsAdmin=true});
 db.Usuarios.Add(new Usuario{Nome="Perfil histórico",Email=legacyEmail,Senha=BCrypt.Net.BCrypt.HashPassword(password),IsModerador=true,IsJornalista=true,StatusJornalista="Aprovado"});
 db.SaveChanges();
}
var psi=new ProcessStartInfo("dotnet"){WorkingDirectory=web,UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
psi.ArgumentList.Add(TestApplication.Dll(root));psi.ArgumentList.Add("--urls");psi.ArgumentList.Add("http://127.0.0.1:5057");
psi.Environment["ASPNETCORE_ENVIRONMENT"]="Development";psi.Environment["ConnectionStrings__DefaultConnection"]=$"Data Source={dbPath}";
psi.Environment["DataDirectory"]=fixture;psi.Environment["Logging__LogLevel__Default"]="Warning";psi.Environment["Logging__LogLevel__Microsoft"]="Warning";
using var server=Process.Start(psi)!;var stdout=server.StandardOutput.ReadToEndAsync();var stderr=server.StandardError.ReadToEndAsync();
var guest=new Session();var student=new Session();var admin=new Session();var other=new Session();var legacy=new Session();
try{
 for(int i=0;i<60;i++){try{if((await guest.Client.GetAsync("/")).StatusCode==HttpStatusCode.OK)break;}catch{}await Task.Delay(300);}
 using(var db=Db()){Check(!db.Database.HasPendingModelChanges(),"Modelo EF Core coincide com o snapshot");Check(!db.Database.GetPendingMigrations().Any(),"Todas as migrações aplicadas");var c=db.Database.GetDbConnection();c.Open();using var q=c.CreateCommand();q.CommandText="PRAGMA integrity_check";Check((string?)q.ExecuteScalar()=="ok","Integridade SQLite");q.CommandText="PRAGMA foreign_key_check";using var reader=q.ExecuteReader();Check(!reader.Read(),"Integridade das chaves estrangeiras");}
 int especieId;int campanhaId;
 using(var db=Db()){especieId=db.Especies.First().Id;campanhaId=db.Campanhas.First(c=>c.Ativa&&c.StatusAprovacao=="Aprovado").Id;}
 foreach(var path in new[]{"/","/Login","/Cadastro","/Home/Sobre","/Home/Preservar","/Especies","/Especies?filtro=Aves","/Especies?filtro=Terrestres","/Especies/Detalhe/"+especieId,"/Campanhas","/Campanhas/Detalhes/"+campanhaId,"/Educacao","/Faq","/Jornada"})
  Check((await guest.Client.GetAsync(path)).StatusCode==HttpStatusCode.OK,"Página pública "+path);
 Check((await guest.Client.GetAsync("/Admin")).StatusCode==HttpStatusCode.Redirect,"Visitante sem acesso administrativo");
 var invalidRegistration=await guest.Post("/Cadastro",new(){["Nome"]="",["Email"]="",["Senha"]=""},"/Cadastro");Check(invalidRegistration.StatusCode==HttpStatusCode.OK,"Cadastro vazio recebe validação sem erro de execução");
 var email=Guid.NewGuid().ToString("N")+"@teste.invalid";
 var reg=await student.Post("/Cadastro",new(){["Nome"]="Estudante de validação",["Email"]=email,["Senha"]=password,["IsAdmin"]="true",["IsModerador"]="true",["IsJornalista"]="true",["Xp"]="999999"},"/Cadastro");
 Check(reg.StatusCode==HttpStatusCode.Redirect,"Cadastro público de estudante");
 int uid;using(var db=Db()){var u=db.Usuarios.Single(u=>u.Email==email);uid=u.Id;Check(!u.IsAdmin&&!u.IsModerador&&!u.IsJornalista&&u.Xp==0,"Cadastro ignora privilégios e XP enviados pelo cliente");Check(u.Senha!=password&&BCrypt.Net.BCrypt.Verify(password,u.Senha),"Senha persistida com hash BCrypt");}
 var dup=await guest.Post("/Cadastro",new(){["Nome"]="Outro",["Email"]=email.ToUpperInvariant(),["Senha"]=password},"/Cadastro");
 Check(dup.StatusCode==HttpStatusCode.OK&&(await dup.Content.ReadAsStringAsync()).Contains("cadastro com esse"),"E-mail duplicado sem distinção de maiúsculas");
 var invalid=await student.Post("/Login",new(){["Email"]=email,["Senha"]="incorreta"},"/Login");Check(invalid.StatusCode==HttpStatusCode.OK,"Login incorreto tratado");
 await student.Login(email,password);Check((await student.Client.GetAsync("/Perfil")).StatusCode==HttpStatusCode.OK,"Login válido e perfil");
 Check((await student.Client.GetAsync("/Admin")).StatusCode==HttpStatusCode.Redirect,"Estudante sem permissão administrativa");
 var logoutGet=await student.Client.GetAsync("/Login/Logout");Check((logoutGet.StatusCode==HttpStatusCode.MethodNotAllowed||logoutGet.StatusCode==HttpStatusCode.NotFound)&&(await student.Client.GetAsync("/Perfil")).StatusCode==HttpStatusCode.OK,"Logout exige POST e GET preserva a sessão");
 Check((await student.Client.PostAsync("/Jornada/Iniciar",new FormUrlEncodedContent(new Dictionary<string,string>{{"desafio","1"}}))).StatusCode==HttpStatusCode.BadRequest,"Proteção antifalsificação de requisições");
 await admin.Login(adminEmail,password);var invalidAdmin=await admin.Post("/Admin/Create",new(){["Nome"]="",["Email"]="",["Senha"]=""},"/Admin/Create");Check(invalidAdmin.StatusCode==HttpStatusCode.OK,"Cadastro administrativo vazio recebe validação");Check((await admin.Client.GetAsync("/Admin")).StatusCode==HttpStatusCode.OK,"Administrador previamente autorizado");
 await legacy.Login(legacyEmail,password);Check((await legacy.Client.GetAsync("/Admin")).StatusCode==HttpStatusCode.Redirect,"Marcadores históricos não concedem administração");
 Check((await legacy.Client.GetAsync("/Forum/Moderacao")).StatusCode==HttpStatusCode.Redirect,"Antigo Moderador tratado como Estudante");
 foreach(var path in new[]{"/Avistamentos","/Avistamentos/Registrar","/Avistamentos/Criar"}){var blocked=await student.Client.GetAsync(path);Check(blocked.StatusCode==HttpStatusCode.Redirect&&blocked.Headers.Location!.ToString()=="/Mapa","Bloqueio de acesso direto "+path);}
 await student.Post("/Jornada/Iniciar",new(){["desafio"]="3"},"/Jornada");
 using(var db=Db()){Check(!db.JornadaTentativas.Any(t=>t.UsuarioId==uid),"Não é possível pular desafios");db.Usuarios.Find(uid)!.Xp=999999;db.SaveChanges();}
 Check((await student.Client.GetAsync("/Avistamentos")).StatusCode==HttpStatusCode.Redirect,"XP alto não desbloqueia mapa");
 var outroEmail=Guid.NewGuid().ToString("N")+"@teste.invalid";
 await other.Post("/Cadastro",new(){["Nome"]="Outro estudante",["Email"]=outroEmail,["Senha"]=password},"/Cadastro");await other.Login(outroEmail,password);
 async Task<int> Start(int nivel){var response=await student.Post("/Jornada/Iniciar",new(){["desafio"]=nivel.ToString()},"/Jornada");Check(response.StatusCode==HttpStatusCode.Redirect,"Iniciar desafio "+nivel);using var db=Db();return db.JornadaTentativas.Where(t=>t.UsuarioId==uid&&t.Desafio==nivel).OrderByDescending(t=>t.Id).First().Id;}
 async Task Complete(int id,int? corretas=null){
 if(Read(id).Jogo is not null){await JogarAtividade.Completar(student,Read,id);return;}
 using var db=Db();var t=db.JornadaTentativas.AsNoTracking().Single(t=>t.Id==id);var qs=t.Questoes;var max=corretas??qs.Count;
 for(var i=t.Respostas.Count;i<qs.Count;i++){var answer=i<max?qs[i].Correta:qs[i].Correta%4+1;
 var response=await student.Post("/Jornada/Responder",new(){["id"]=id.ToString(),["posicao"]=i.ToString(),["resposta"]=answer.ToString()},"/Jornada/Jogar/"+id);
 Check(response.StatusCode==HttpStatusCode.Redirect,"Persistir resposta "+t.Desafio+"."+i);
 var feedback=await student.Client.GetStringAsync("/Jornada/Feedback/"+id);Check(feedback.Contains("Resposta esperada")&&feedback.Contains("Refer"),"Feedback explicativo "+t.Desafio+"."+i);
 }
 }
 var first=await Start(1);
 Check((await other.Client.GetAsync("/Jornada/Jogar/"+first)).StatusCode==HttpStatusCode.NotFound,"Tentativa não pode ser lida por outro estudante");
 Check((await other.Post("/Jornada/Responder",new(){["id"]=first.ToString(),["posicao"]="0",["resposta"]="1"},"/Jornada")).StatusCode==HttpStatusCode.NotFound,"Tentativa não pode ser respondida por outro estudante");
 await student.Post("/Jornada/Responder",new(){["id"]=first.ToString(),["posicao"]="0"},"/Jornada/Jogar/"+first);
 using(var db=Db()){Check(db.JornadaTentativas.Find(first)!.Pontos==0,"POST do quiz não avança jogo de cartas");var t=db.JornadaTentativas.Find(first)!;var carta=t.Jogo!.Cartas[0];var especie=db.Especies.Find(carta.Id)!;especie.Habitat+=" Revisão administrativa";db.SaveChanges();Check(db.JornadaTentativas.Find(first)!.Jogo!.Cartas[0].Habitat!=especie.Habitat,"Edição administrativa preserva cartas de tentativas iniciadas");}
 int total;using(var db=Db())total=db.JornadaTentativas.Find(first)!.Total;
 await Complete(first,total-1);
 using(var db=Db())Check(db.JornadaTentativas.Find(first)!.Aprovada,"Associações do primeiro desafio concluídas");
 await student.Post("/Jornada/Responder",new(){["id"]=first.ToString(),["posicao"]="0",["resposta"]="1"},"/Jornada");
 using(var db=Db())Check(db.JornadaTentativas.Find(first)!.EstadoJogo.Associadas.Count==total,"Reenvio do quiz não altera associações concluídas");
 var failed=await Start(2);await Complete(failed,0);
 using(var db=Db())Check(!db.JornadaTentativas.Find(failed)!.Aprovada,"Tentativa insuficiente fica registrada e permite repetição");
 var second=await Start(2);await Complete(second,7);
 using(var db=Db()){var t=db.JornadaTentativas.Find(second)!;Check(t.Aprovada&&t.Aproveitamento==70,"Limite de 70% aplicado no servidor");}
 Check((await student.Client.GetAsync("/Avistamentos")).StatusCode==HttpStatusCode.Redirect,"Dois desafios ainda não liberam mapa");
 var third=await Start(3);await Complete(third);
 Check((await student.Client.GetAsync("/Avistamentos")).StatusCode==HttpStatusCode.OK,"Três desafios desbloqueiam mapa");
 Check((await student.Client.GetAsync("/Avistamentos/Registrar")).StatusCode==HttpStatusCode.OK,"Registro liberado após jornada");
 Check((await student.Client.GetStringAsync("/Jornada/Progresso")).Contains("Acess"),"Avatar e acessório exibidos no progresso");
 var attemptCount=0;using(var db=Db())attemptCount=db.JornadaTentativas.Count(t=>t.UsuarioId==uid);
 await Start(1);await Start(1);
 using(var db=Db())Check(db.JornadaTentativas.Count(t=>t.UsuarioId==uid)==attemptCount+1,"Continuar desafio reutiliza tentativa em andamento");
 var fake=await student.Post("/Avistamentos/Registrar",new(){["Form.EspecieId"]=especieId.ToString(),["Form.NomeObservador"]="Teste",["Form.Latitude"]="0",["Form.Longitude"]="0",["Form.DataAvistamento"]=DateTime.Today.ToString("yyyy-MM-dd"),["Form.Quantidade"]="1"},"/Avistamentos/Registrar");
 Check(fake.StatusCode==HttpStatusCode.OK&&(await fake.Content.ReadAsStringAsync()).Contains("Serra da Cantareira"),"Registro inválido recebe orientação");

 var lat=-23.411123;var lon=-46.616321;
 Check(LimitesSerraCantareira.Contem(lat,lon),"Coordenada de teste dentro da Cantareira");
 var token=await student.Token("/Avistamentos/Registrar");using(var multipart=new MultipartFormDataContent()){
 foreach(var pair in new Dictionary<string,string>{{"__RequestVerificationToken",token},{"Form.EspecieId",especieId.ToString()},{"Form.NomeObservador","Estudante"},{"Form.Latitude",lat.ToString(System.Globalization.CultureInfo.InvariantCulture)},{"Form.Longitude",lon.ToString(System.Globalization.CultureInfo.InvariantCulture)},{"Form.DataAvistamento",DateTime.Today.ToString("yyyy-MM-dd")},{"Form.Quantidade","1"},{"Form.Observacoes","Registro em banco isolado de validação."}})multipart.Add(new StringContent(pair.Value),pair.Key);
 var image=new ByteArrayContent(File.ReadAllBytes(Path.Combine(web,"wwwroot","img","especies","onca-parda.jpg")));image.Headers.ContentType=new MediaTypeHeaderValue("image/jpeg");multipart.Add(image,"Form.Fotos","observacao.jpg");
 var r=await student.Client.PostAsync("/Avistamentos/Registrar",multipart);Check(r.StatusCode==HttpStatusCode.Redirect,"Envio de avistamento com evidência real");
 }
 int registroId,midiaId;string evidence;
 using(var db=Db()){var a=db.Avistamentos.Include(a=>a.Midias).Where(a=>a.UsuarioId==uid).OrderByDescending(a=>a.Id).First();registroId=a.Id;midiaId=a.Midias.First().Id;evidence=a.Midias.First().Caminho;Check(!a.Aprovado,"Novo avistamento aguarda aprovação");}
 Check((await guest.Client.GetAsync(evidence)).StatusCode==HttpStatusCode.NotFound,"Evidência nova não é servida como arquivo público");
 await admin.Post("/AdminAvistamentos/Aprovar",new(){["id"]=registroId.ToString(),["aprovado"]="true"},"/AdminAvistamentos");
 var mapa=await student.Client.GetStringAsync("/Avistamentos");Check(!mapa.Contains(lat.ToString(System.Globalization.CultureInfo.InvariantCulture))&&!mapa.Contains(lon.ToString(System.Globalization.CultureInfo.InvariantCulture)),"Mapa do estudante não revela coordenadas precisas");
 Check((await admin.Client.GetAsync("/AdminAvistamentos/Evidencia/"+midiaId)).StatusCode==HttpStatusCode.OK,"Evidência disponível à administração");
 Check((await student.Client.GetAsync("/AdminAvistamentos/Evidencia/"+midiaId)).StatusCode==HttpStatusCode.Redirect,"Evidência restrita à administração");
 
var quizFields=new Dictionary<string,string>{{"id",especieId.ToString()}};
 using(var db=Db()){var qs=db.EspecieQuizPerguntas.Where(p=>p.EspecieId==especieId).OrderBy(p=>p.Ordem).ThenBy(p=>p.Id).ToList();for(var i=0;i<qs.Count;i++){quizFields[$"respostas[{i}].PerguntaId"]=qs[i].Id.ToString();quizFields[$"respostas[{i}].OpcaoSelecionada"]=qs[i].RespostaCorreta.ToString();}}
 var quizResponse=await student.Post("/EspecieQuiz/Enviar",quizFields,"/Especies/Detalhe/"+especieId);Check(quizResponse.StatusCode==HttpStatusCode.OK&&(await quizResponse.Content.ReadAsStringAsync()).Contains("Resposta esperada"),"Quiz da espécie apresenta correção educativa");
 await student.Post("/Especies/MarcarEstudoLido",new(){["id"]=especieId.ToString()},"/Especies/Detalhe/"+especieId);
 using(var db=Db())Check(db.PokedexRegistros.Any(r=>r.UsuarioId==uid&&r.EspecieId==especieId&&r.Dominada),"Leitura e quiz conquistam espécie na CantaDex");
 
 var vozTitle="Reflexão de validação "+Guid.NewGuid().ToString("N");
 var publish=await student.Post("/Educacao/Publicar",new(){["Titulo"]=vozTitle,["Resumo"]="Aprendizado sobre preservação.",["Conteudo"]="<script>alert(1)</script> Respeitar a fauna.",["Referencias"]="Material original do Grupo 12."},"/Educacao/Publicar");
 Check(publish.StatusCode==HttpStatusCode.Redirect,"Estudante envia texto para revisão");
 int vozId;string slug;using(var db=Db()){var v=db.EstudosCientificos.Single(e=>e.Titulo==vozTitle);vozId=v.Id;slug=v.Slug;Check(v.StatusAprovacao=="Pendente","Texto não é publicado automaticamente");}
 Check((await guest.Client.GetAsync("/Educacao/Artigo?slug="+slug)).StatusCode==HttpStatusCode.NotFound,"Texto pendente não fica público");
 await admin.Post("/Educacao/Moderar",new(){["id"]=vozId.ToString(),["aprovar"]="true"},"/Educacao/Revisar");
 var article=await guest.Client.GetStringAsync("/Educacao/Artigo?slug="+slug);Check(article.Contains("Reflex")&&!article.Contains("<script>alert(1)</script>"),"Texto aprovado disponível com HTML do estudante escapado");
 var post=await student.Post("/Forum/Criar",new(){["campanhaId"]=campanhaId.ToString(),["titulo"]="Aprender a preservar",["conteudo"]="Reflexão relacionada à campanha."},"/Campanhas/Detalhes/"+campanhaId);Check(post.StatusCode==HttpStatusCode.Redirect,"Mural pertence a uma campanha");
 int topic;using(var db=Db())topic=db.ForumTopicos.Where(t=>t.UsuarioId==uid).OrderByDescending(t=>t.Id).First().Id;
 await student.Post("/Forum/Responder",new(){["id"]=topic.ToString(),["conteudo"]="Comentário educativo."},"/Campanhas/Detalhes/"+campanhaId);
 await student.Post("/Forum/Curtir",new(){["id"]=topic.ToString()},"/Campanhas/Detalhes/"+campanhaId);
 using(var db=Db())Check(db.ForumRespostas.Any(r=>r.ForumTopicoId==topic)&&db.ForumCurtidasTopico.Any(l=>l.ForumTopicoId==topic),"Comentários e apoios persistidos");
 await admin.Post("/Forum/Arquivar",new(){["id"]=topic.ToString()},"/Forum/Moderacao");
 using(var db=Db())Check(db.ForumTopicos.Find(topic)!.Categoria.StartsWith("arquivada:"),"Administração arquiva publicação sem apagar registro");
 await student.Post("/Campanhas/Participar",new(){["id"]=campanhaId.ToString()},"/Campanhas/Detalhes/"+campanhaId);
 using(var db=Db())Check(db.CampanhaParticipacoes.Any(p=>p.CampanhaId==campanhaId&&p.UsuarioId==uid),"Participação na campanha persistida");
 foreach(var path in new[]{"/Admin","/Admin/Contas","/Admin/Create","/AdminEspecies","/AdminEspecies/Create","/AdminEspecies/Edit/"+especieId,"/AdminJornada","/AdminJornada/Edit","/AdminEspecieQuiz","/AdminCampanhas","/AdminCampanhas/Create","/AdminAvistamentos","/Educacao/Revisar","/Forum/Moderacao","/AdminFaq"})
  Check((await admin.Client.GetAsync(path)).StatusCode==HttpStatusCode.OK,"Página administrativa "+path);
 await student.Post("/Faq/NovaPergunta",new(){["assunto"]="Dúvida de validação",["categoria"]="Geral",["mensagem"]="Como revisar os conteúdos?"},"/Faq");
 using(var db=Db())Check(db.FaqTickets.Any(t=>t.UsuarioId==uid),"Dúvida de suporte persistida");
 var responseLogout=await other.Post("/Login/Logout",new(),"/Perfil");Check(responseLogout.StatusCode==HttpStatusCode.Redirect&&(await other.Client.GetAsync("/Perfil")).StatusCode==HttpStatusCode.Redirect,"Logout invalida acesso autenticado");
 using(var db=Db()){foreach(var u in db.Usuarios.Where(u=>!u.Email.EndsWith("@teste.invalid"))){u.Nome="Usuário de demonstração "+u.Id;u.Email="demo"+u.Id+"@exemplo.invalid";}db.SaveChanges();}
 await Browser.Run(root,output,guest,student,admin,especieId,campanhaId,first,second,third,Check);
 using(var db=Db()){db.Usuarios.Single(u=>u.Email==adminEmail).IsAdmin=false;db.SaveChanges();}
 Check((await admin.Client.GetAsync("/Admin")).StatusCode==HttpStatusCode.Redirect,"Revogação administrativa invalida cookie anterior");
 var fresh=new Session();await fresh.Login(email,password);Check((await fresh.Client.GetAsync("/Avistamentos")).StatusCode==HttpStatusCode.OK,"Progresso e desbloqueio persistem em novo login");
 File.WriteAllText(Path.Combine(output,"resultado.json"),JsonSerializer.Serialize(new{executadoEm=DateTimeOffset.Now,total=checks.Count,checks,fixture},new JsonSerializerOptions{WriteIndented=true}));
 Console.WriteLine("VALIDAÇÃO CONCLUÍDA: "+checks.Count+" verificações.");
}catch(Exception ex){File.WriteAllText(Path.Combine(output,"falha.txt"),ex.ToString());File.WriteAllText(Path.Combine(output,"resultado.json"),JsonSerializer.Serialize(checks,new JsonSerializerOptions{WriteIndented=true}));Console.Error.WriteLine(ex);Environment.ExitCode=1;}
finally{if(!server.HasExited)server.Kill(true);await server.WaitForExitAsync();File.WriteAllText(Path.Combine(fixture,"servidor.log"),await stdout+await stderr);}

class Session{
 public readonly CookieContainer Cookies=new();
 public HttpClient Client{get;}
 public Session(){Client=new HttpClient(new HttpClientHandler{CookieContainer=Cookies,AllowAutoRedirect=false}){BaseAddress=new Uri("http://127.0.0.1:5057"),Timeout=TimeSpan.FromSeconds(20)};}
 public async Task<string> Token(string path){var html=await Client.GetStringAsync(path);var input=Regex.Match(html,"<input[^>]*name=\"__RequestVerificationToken\"[^>]*>").Value;var token=Regex.Match(input,"value=\"([^\"]+)\"").Groups[1].Value;if(token=="")throw new Exception("Token não encontrado em "+path);return WebUtility.HtmlDecode(token);}
 public async Task<HttpResponseMessage> Post(string path,Dictionary<string,string> fields,string tokenPath){fields["__RequestVerificationToken"]=await Token(tokenPath);return await Client.PostAsync(path,new FormUrlEncodedContent(fields));}
 public async Task Login(string email,string password){var r=await Post("/Login",new(){["Email"]=email,["Senha"]=password},"/Login");if(r.StatusCode!=HttpStatusCode.Redirect)throw new Exception("Falha de autenticação no teste");}
}
class Browser{
 public static async Task Run(string root,string output,Session guest,Session student,Session admin,int especie,int campanha,int first,int second,int third,Action<bool,string> check){
  var chrome=@"C:\Program Files\Google\Chrome\Application\chrome.exe";
  var psi=new ProcessStartInfo(chrome){UseShellExecute=false,CreateNoWindow=true};
  foreach(var arg in new[]{"--headless=new","--disable-gpu","--hide-scrollbars","--no-first-run","--remote-debugging-port=9223","--user-data-dir="+Path.Combine(output,"chrome-profile"),"about:blank"})psi.ArgumentList.Add(arg);
  using var proc=Process.Start(psi)!;
  try{
   using var http=new HttpClient();string? ws=null;
   for(var i=0;i<50;i++){try{var json=await http.GetStringAsync("http://127.0.0.1:9223/json/list");using var d=JsonDocument.Parse(json);ws=d.RootElement.EnumerateArray().First(x=>x.GetProperty("type").GetString()=="page").GetProperty("webSocketDebuggerUrl").GetString();if(ws!=null)break;}catch{}await Task.Delay(200);}
   if(ws==null)throw new Exception("Chrome CDP indisponível");
   using var cdp=new Cdp();await cdp.Connect(ws);await cdp.Call("Page.enable",new{});await cdp.Call("Network.enable",new{});
   var screenshots=Path.Combine(root,"documentacao","capturas");Directory.CreateDirectory(screenshots);
   async Task Cookies(Session s){await cdp.Call("Network.clearBrowserCookies",new{});foreach(Cookie c in s.Cookies.GetCookies(new Uri("http://127.0.0.1:5057"))){await cdp.Call("Network.setCookie",new{name=c.Name,value=c.Value,url="http://127.0.0.1:5057",httpOnly=c.HttpOnly});}}
   async Task<JsonElement> Eval(string expression){var r=await cdp.Call("Runtime.evaluate",new{expression,returnByValue=true,awaitPromise=true});return r.GetProperty("result").GetProperty("value");}
   async Task Capture(string name,string path,int width=1440){
    await cdp.Call("Emulation.setDeviceMetricsOverride",new{width,height=960,deviceScaleFactor=1,mobile=width<650});
    await cdp.Call("Page.navigate",new{url="http://127.0.0.1:5057"+path});
    for(var i=0;i<50;i++){await Task.Delay(100);try{if((await Eval("document.readyState==='complete'")).GetBoolean())break;}catch{}}
    await Eval("document.fonts.ready.then(()=>true)");await Eval("Promise.all([...document.images].map(i=>{i.loading=\'eager\';return i.decode().catch(()=>{});})).then(()=>true)");await Task.Delay(250);
    var result=await Eval("JSON.stringify({width:innerWidth,scroll:document.documentElement.scrollWidth,broken:[...document.images].filter(i=>i.offsetParent!==null&&(!i.complete||i.naturalWidth===0)).length,h1:document.querySelectorAll('h1').length,title:document.title,fonts:document.fonts.check('16px Inter')&&document.fonts.check('16px Sora')})");
    using var audit=JsonDocument.Parse(result.GetString()!);var a=audit.RootElement;
    check(a.GetProperty("scroll").GetInt32()<=a.GetProperty("width").GetInt32()+1,"Sem transbordamento "+name);
    check(a.GetProperty("broken").GetInt32()==0,"Imagens carregadas "+name);
    check(a.GetProperty("fonts").GetBoolean(),"Sora e Inter carregadas "+name);
    var shot=await cdp.Call("Page.captureScreenshot",new{format="png",captureBeyondViewport=true,fromSurface=true});
    File.WriteAllBytes(Path.Combine(screenshots,name+".png"),Convert.FromBase64String(shot.GetProperty("data").GetString()!));
   }
   await Cookies(guest);
   await Capture("01-login","/Login");await Capture("02-cadastro","/Cadastro");await Capture("03-inicio","/");await Capture("sobre-projeto","/Home/Sobre");await Capture("mapa-bloqueado","/Jornada?bloqueado=true");
   await Capture("inicio-tablet","/",768);await Capture("inicio-celular","/",390);
   await Eval("document.querySelector('.ec-menu').click();true");check((await Eval("document.querySelector('.ec-menu').getAttribute('aria-expanded')==='true'&&getComputedStyle(document.querySelector('.ec-nav')).display!=='none'")).GetBoolean(),"Menu móvel abre");
   await cdp.Call("Input.dispatchKeyEvent",new{type="keyDown",key="Escape",code="Escape",windowsVirtualKeyCode=27});
   check((await Eval("document.querySelector('.ec-menu').getAttribute('aria-expanded')==='false'")).GetBoolean(),"Menu móvel fecha com Escape");
   await Cookies(student);
   await Capture("04-inicio-estudante","/");await Capture("05-catalogo","/Especies");await Capture("06-ficha","/Especies/Detalhe/"+especie);
   await Capture("07-jornada","/Jornada");await Capture("08-detetive","/Jornada/Feedback/"+first);
   await Capture("09-guardiao","/Jornada/Feedback/"+second);await Capture("10-missao","/Jornada/Feedback/"+third);
   await Capture("11-progresso","/Jornada/Progresso");await Capture("12-mapa","/Avistamentos");
   await Capture("13-campanhas","/Campanhas");await Capture("14-campanha-mural","/Campanhas/Detalhes/"+campanha);
   await Capture("15-voz-estudantil","/Educacao");await Capture("registro-avistamento","/Avistamentos/Registrar");await Capture("perfil-estudante","/Perfil");await Capture("ajuda-estudante","/Faq");await Capture("voz-publicar","/Educacao/Publicar");
   await Capture("catalogo-celular","/Especies",390);await Capture("ficha-celular","/Especies/Detalhe/"+especie,390);
   await Capture("jornada-celular","/Jornada",390);await Capture("mapa-celular","/Avistamentos",390);
   await Cookies(admin);
   await Capture("16-admin","/Admin");await Capture("17-admin-especies","/AdminEspecies");
   await Capture("18-admin-desafios","/AdminJornada");await Capture("19-admin-voz","/Educacao/Revisar");
   await Capture("20-admin-campanhas","/AdminCampanhas");await Capture("21-admin-avistamentos","/AdminAvistamentos");await Capture("22-admin-usuarios","/Admin/Contas");await Capture("admin-cadastro-usuario","/Admin/Create");await Capture("admin-formulario-especie","/AdminEspecies/Edit/"+especie);await Capture("admin-editor-desafio","/AdminJornada/Edit");await Capture("admin-editor-quiz","/AdminEspecieQuiz/Gerenciar/"+especie);await Capture("admin-cadastro-campanha","/AdminCampanhas/Create");await Capture("admin-suporte","/AdminFaq");await Capture("admin-mural","/Forum/Moderacao");
   await Capture("admin-celular","/Admin",390);await Capture("admin-especies-celular","/AdminEspecies",390);
   check((await Eval("!!document.querySelector('.skip-link')")).GetBoolean(),"Atalho para conteúdo disponível");
  }finally{if(!proc.HasExited)proc.Kill(true);}
 }
}
class Cdp:IDisposable{
 readonly ClientWebSocket ws=new();int serial;
 public Task Connect(string url)=>ws.ConnectAsync(new Uri(url),CancellationToken.None);
 public async Task<JsonElement> Call(string method,object p){
  var id=++serial;var bytes=JsonSerializer.SerializeToUtf8Bytes(new{id,method,@params=p});await ws.SendAsync(bytes,WebSocketMessageType.Text,true,CancellationToken.None);
  while(true){using var ms=new MemoryStream();var buffer=new byte[65536];WebSocketReceiveResult r;do{r=await ws.ReceiveAsync(buffer,CancellationToken.None);ms.Write(buffer,0,r.Count);}while(!r.EndOfMessage);
   using var json=JsonDocument.Parse(ms.ToArray());var e=json.RootElement;if(e.TryGetProperty("id",out var reply)&&reply.GetInt32()==id){if(e.TryGetProperty("error",out var err))throw new Exception(method+": "+err);return e.GetProperty("result").Clone();}
  }
 }
 public void Dispose()=>ws.Dispose();
}
