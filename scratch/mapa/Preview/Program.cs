using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;
var root=Path.GetFullPath(args[0]);var dir=Path.Combine(root,"scratch","mapa","execucao-"+DateTime.Now.ToString("HHmmss"));Directory.CreateDirectory(dir);
var output=Path.Combine(root,"documentacao",Environment.GetEnvironmentVariable("ECOCANTA_MAP_REPORT")??"mapa-correcao-20261115");Directory.CreateDirectory(output);
var backup=Directory.GetDirectories(Path.Combine(root,"backups"),"antes-jogos-*").OrderByDescending(p=>p).First();
var dbPath=Path.Combine(dir,"teste.db");File.Copy(Path.Combine(backup,"app.db"),dbPath);TestApplication.PrepareCopy(dbPath);
Console.WriteLine("Banco descartável pronto.");
var password=Guid.NewGuid().ToString("N");var email=Guid.NewGuid().ToString("N")+"@teste.invalid";
using(var db=new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite("Data Source="+dbPath).Options)){
 db.Usuarios.Add(new Usuario{Nome="Verificação visual",Email=email,Senha=BCrypt.Net.BCrypt.HashPassword(password),IsAdmin=true});
 foreach(var a in db.Avistamentos)a.Aprovado=false;db.SaveChanges();
}
var psi=new ProcessStartInfo("dotnet"){WorkingDirectory=Path.Combine(root,"TCC_Grupo12_III"),UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
psi.ArgumentList.Add(TestApplication.Dll(root));psi.ArgumentList.Add("--urls");psi.ArgumentList.Add("http://127.0.0.1:5057");
psi.Environment["ASPNETCORE_ENVIRONMENT"]="Development";psi.Environment["ConnectionStrings__DefaultConnection"]="Data Source="+dbPath;psi.Environment["Logging__LogLevel__Default"]="Warning";psi.Environment["Logging__LogLevel__Microsoft"]="Warning";
Console.WriteLine("Iniciando servidor isolado.");
using var server=Process.Start(psi)!;var log=server.StandardOutput.ReadToEndAsync();var errors=server.StandardError.ReadToEndAsync();
try{
 var cookies=new CookieContainer();using var http=new HttpClient(new HttpClientHandler{CookieContainer=cookies}){BaseAddress=new Uri("http://127.0.0.1:5057"),Timeout=TimeSpan.FromSeconds(3)};
 bool ready=false;for(int i=0;i<40;i++){try{if((await http.GetAsync("/")).IsSuccessStatusCode){ready=true;break;}}catch{}await Task.Delay(200);}if(!ready)throw new Exception("Servidor não iniciou.");
 Console.WriteLine("Servidor pronto. Autenticando conta descartável.");
 var login=await http.GetStringAsync("/Login");var token=WebUtility.HtmlDecode(Regex.Match(Regex.Match(login,"<input[^>]*name=\"__RequestVerificationToken\"[^>]*>").Value,"value=\"([^\"]+)\"").Groups[1].Value);
 await http.PostAsync("/Login",new FormUrlEncodedContent(new Dictionary<string,string>{{"Email",email},{"Senha",password},{"__RequestVerificationToken",token}}));
 Console.WriteLine("Abrindo Chrome.");
 using var chrome=Process.Start(new ProcessStartInfo(@"C:\Program Files\Google\Chrome\Application\chrome.exe"){UseShellExecute=false,CreateNoWindow=true,Arguments="--headless=new --disable-gpu --hide-scrollbars --remote-debugging-port=9227 --no-first-run --user-data-dir=\""+Path.Combine(dir,"chrome")+"\" about:blank"})!;
 try{
 string? ws=null;using var dev=new HttpClient();for(int i=0;i<40;i++){try{using var d=JsonDocument.Parse(await dev.GetStringAsync("http://127.0.0.1:9227/json/list"));ws=d.RootElement.EnumerateArray().First(p=>p.GetProperty("type").GetString()=="page").GetProperty("webSocketDebuggerUrl").GetString();if(ws is not null)break;}catch{}await Task.Delay(200);}
 using var cdp=new Cdp();await cdp.Connect(ws!);await cdp.Call("Page.enable",new{});await cdp.Call("Network.enable",new{});
 foreach(Cookie c in cookies.GetCookies(http.BaseAddress!))await cdp.Call("Network.setCookie",new{name=c.Name,value=c.Value,url=http.BaseAddress!.ToString(),httpOnly=c.HttpOnly});
 await cdp.Call("Page.addScriptToEvaluateOnNewDocument",new{source="window.__mapErrors=[];addEventListener('error',e=>{if(e.error)__mapErrors.push({message:e.message,stack:e.error.stack});});"});
 async Task<JsonElement> Eval(string js){var r=await cdp.Call("Runtime.evaluate",new{expression=js,returnByValue=true,awaitPromise=true});if(r.TryGetProperty("exceptionDetails",out var error))throw new Exception(error.ToString());return r.GetProperty("result").GetProperty("value");}
 var observations=new List<object>();
 foreach(var item in new[]{(Name:"mapa-desktop",Url:"/Avistamentos",Width:1440,Dark:false),(Name:"mapa-celular-escuro",Url:"/Avistamentos",Width:390,Dark:true),(Name:"registro-desktop",Url:"/Avistamentos/Registrar",Width:1440,Dark:false),(Name:"registro-celular",Url:"/Avistamentos/Registrar",Width:390,Dark:false)}){
 await cdp.Call("Emulation.setDeviceMetricsOverride",new{width=item.Width,height=960,deviceScaleFactor=1,mobile=item.Width<650});
 await cdp.Call("Page.navigate",new{url=http.BaseAddress+item.Url.TrimStart('/')});await Task.Delay(400);
 for(int i=0;i<60;i++){try{if((await Eval("document.readyState==='complete'")).GetBoolean())break;}catch{}await Task.Delay(150);}
 if((await Eval("document.documentElement.dataset.theme==='dark'")).GetBoolean()!=item.Dark)await Eval("document.querySelector('[data-theme-toggle]').click();true");
 if(item.Url=="/Avistamentos"){await Eval("document.querySelector('#especie-mapa').selectedIndex=1;document.querySelector('#especie-mapa').dispatchEvent(new Event('change'));true");}
 await Eval("Promise.all([...document.querySelectorAll('.leaflet-tile')].map(i=>i.decode().catch(()=>{}))).then(()=>true)"); await Task.Delay(350);
 var audit=(await Eval("JSON.stringify({contorno:document.querySelectorAll('.ec-cantareira-boundary').length,semTransbordamento:document.documentElement.scrollWidth<=innerWidth+1,erros:__mapErrors,tilesCarregados:document.querySelectorAll('.leaflet-tile-loaded').length,tiles:document.querySelectorAll('.leaflet-tile').length,status:document.querySelector('#mapa-status')?.textContent})")).GetString();Console.WriteLine(item.Name+": "+audit);
 if(item.Url.Contains("Registrar")){
 await Eval("document.querySelector('#mapa-registro').scrollIntoView({block:'center'});true");await Task.Delay(100);
 var center=await Eval("(()=>{const r=document.querySelector('#mapa-registro').getBoundingClientRect();return {x:r.left+r.width/2,y:r.top+r.height/2};})()");
 var x=center.GetProperty("x").GetDouble();var y=center.GetProperty("y").GetDouble();
 await cdp.Call("Input.dispatchMouseEvent",new{type="mousePressed",x,y,button="left",clickCount=1});await cdp.Call("Input.dispatchMouseEvent",new{type="mouseReleased",x,y,button="left",clickCount=1});
 var picked=(await Eval("document.querySelector('#Form_Latitude').value!==''&&document.querySelector('#Form_Longitude').value!==''&&document.querySelectorAll('.leaflet-marker-icon').length===1")).GetBoolean();
 observations.Add(new{tela=item.Name,analise=JsonSerializer.Deserialize<JsonElement>(audit!),selecaoDoPontoFunciona=picked});if(!picked)throw new Exception("Clique de registro bloqueado.");
 }else observations.Add(new{tela=item.Name,analise=JsonSerializer.Deserialize<JsonElement>(audit!)});
 await Eval("scrollTo(0,0);true");await Task.Delay(120);
 var shot=await cdp.Call("Page.captureScreenshot",new{format="png",captureBeyondViewport=true,fromSurface=true});File.WriteAllBytes(Path.Combine(output,item.Name+".png"),Convert.FromBase64String(shot.GetProperty("data").GetString()!));
 }
 File.WriteAllText(Path.Combine(output,"conferencia.json"),JsonSerializer.Serialize(observations,new JsonSerializerOptions{WriteIndented=true}));Console.WriteLine("Conferência das quatro telas concluída.");
 }finally{if(!chrome.HasExited)chrome.Kill(true);}
}finally{if(!server.HasExited)server.Kill(true);await server.WaitForExitAsync();File.WriteAllText(Path.Combine(output,"servidor.log"),await log+await errors);}
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
