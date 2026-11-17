using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;
using TCC_Grupo12_III.Services;

internal static class JogosEducativos
{
    public static async Task Run(string root,bool interacoes=false,bool confiabilidade=false)
    {
        var output = Path.Combine(root, "documentacao", confiabilidade ? "correcoes-20261115" : interacoes ? "interacoes-20261114" : "jogos-20261114");
        var fixture = Path.Combine(root, "scratch", "jogos", "execucao-" + DateTime.Now.ToString("HHmmss"));
        Directory.CreateDirectory(output); Directory.CreateDirectory(fixture);
        var dbPath = Path.Combine(fixture, "teste.db");
        using (var source = new SqliteConnection($"Data Source={Path.Combine(root, "TCC_Grupo12_III", "app.db")};Mode=ReadOnly"))
        using (var target = new SqliteConnection($"Data Source={dbPath}")) { source.Open(); target.Open(); source.BackupDatabase(target); }
        TestApplication.PrepareCopy(dbPath);
        AppDbContext Db() => new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite($"Data Source={dbPath}").Options);
        JornadaTentativa Read(int id) { using var db = Db(); return db.JornadaTentativas.AsNoTracking().Single(t => t.Id == id); }
        var checks = new List<object>();
        void Check(bool pass, string name) { checks.Add(new { teste = name, passou = pass }); Console.WriteLine((pass ? "OK " : "FALHOU ") + name); if (!pass) throw new Exception(name); }
        var password = "Teste!" + Guid.NewGuid().ToString("N");
        var adminEmail = Guid.NewGuid().ToString("N") + "@teste.invalid";
        using (var db = Db())
        {
            foreach (var u in db.Usuarios) { u.Nome = "Conta demonstrativa " + u.Id; u.Email = "conta" + u.Id + "@exemplo.invalid"; }
            db.Usuarios.Add(new Usuario { Nome = "Administração de demonstração", Email = adminEmail, Senha = BCrypt.Net.BCrypt.HashPassword(password), IsAdmin = true }); db.SaveChanges();
        }
        var psi = new ProcessStartInfo("dotnet") { WorkingDirectory = Path.Combine(root, "TCC_Grupo12_III"), UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
        psi.ArgumentList.Add(TestApplication.Dll(root)); psi.ArgumentList.Add("--urls"); psi.ArgumentList.Add("http://127.0.0.1:5057");
        psi.Environment["ASPNETCORE_ENVIRONMENT"] = "Development"; psi.Environment["ConnectionStrings__DefaultConnection"] = $"Data Source={dbPath}";
        psi.Environment["DataDirectory"] = fixture; psi.Environment["Logging__LogLevel__Default"] = "Warning"; psi.Environment["Logging__LogLevel__Microsoft"] = "Warning";
        using var server = Process.Start(psi)!;
        var stdout = server.StandardOutput.ReadToEndAsync(); var stderr = server.StandardError.ReadToEndAsync();
        try
        {
            using var health = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            bool ready = false;
            for (var i = 0; i < 60; i++) { try { if ((await health.GetAsync("http://127.0.0.1:5057/")).IsSuccessStatusCode) { ready = true; break; } } catch { } await Task.Delay(200); }
            Check(ready, "Servidor inicia em cópia isolada do banco");
            using (var db = Db())
            {
                Check(!db.Database.HasPendingModelChanges() && !db.Database.GetPendingMigrations().Any(), "Jogos não exigem migração ou alteração de esquema");
                var conn = db.Database.GetDbConnection(); conn.Open(); using var cmd = conn.CreateCommand(); cmd.CommandText = "PRAGMA integrity_check"; Check((string?)cmd.ExecuteScalar() == "ok", "Integridade SQLite");
            }
            var guest = new Session(); var student = new Session(); var admin = new Session(); var other = new Session();
            foreach (var url in new[] { "/", "/Jornada", "/Mapa", "/Faq", "/Home/Sobre", "/Especies" }) Check((await guest.Client.GetAsync(url)).IsSuccessStatusCode, "Navegação pública " + url);
            var email = Guid.NewGuid().ToString("N") + "@teste.invalid";
            await student.Post("/Cadastro", new() { ["Nome"] = "Estudante de demonstração", ["Email"] = email, ["Senha"] = password, ["IsAdmin"] = "true" }, "/Cadastro");
            await student.Login(email, password); await admin.Login(adminEmail, password);
            int uid; using (var db = Db()) { var user = db.Usuarios.Single(u => u.Email == email); uid = user.Id; Check(!user.IsAdmin, "Cadastro dos jogos continua criando somente estudante"); }
            var otherEmail = Guid.NewGuid().ToString("N") + "@teste.invalid";
            await other.Post("/Cadastro", new() { ["Nome"] = "Outra conta", ["Email"] = otherEmail, ["Senha"] = password }, "/Cadastro"); await other.Login(otherEmail, password);
            async Task<int> Start(int stage)
            {
                var response = await student.Post("/Jornada/Iniciar", new() { ["desafio"] = stage.ToString() }, "/Jornada");
                Check(response.StatusCode == HttpStatusCode.Redirect, "Iniciar etapa " + stage);
                using var db = Db(); return db.JornadaTentativas.Where(t => t.UsuarioId == uid && t.Desafio == stage).OrderByDescending(t => t.Id).First().Id;
            }
            async Task<HttpResponseMessage> Act(int id, string action, string target = "", string tool = "observar", int? version = null, Session? session = null, bool json = false,string destination="")
            {
                var client = session ?? student;
                var fields = new Dictionary<string, string> { ["id"] = id.ToString(), ["revisao"] = (version ?? Read(id).EstadoJogo.Revisao).ToString(), ["acao"] = action, ["alvo"] = target, ["ferramenta"] = tool, ["Pontos"] = "999", ["Aprovada"] = "true" };
                fields["__RequestVerificationToken"] = await client.Token("/Jornada");
                fields["destino"]=destination;
                using var request = new HttpRequestMessage(HttpMethod.Post, "/Jornada/Agir") { Content = new FormUrlEncodedContent(fields) };
                if (json) request.Headers.Accept.ParseAdd("application/json");
                return await client.Client.SendAsync(request);
            }
            await student.Post("/Jornada/Iniciar", new() { ["desafio"] = "3" }, "/Jornada");
            using (var db = Db()) Check(!db.JornadaTentativas.Any(t => t.UsuarioId == uid), "Servidor impede pular para a missão");
            var first = await Start(1);
            Check(Read(first).Jogo?.Tipo == "cartas" && Read(first).Questoes.Count == 0, "Detetive inicia cartas, sem questões de quiz");
            var original = Read(first).QuestoesJson;
            using (var db = Db()) { var species = db.Especies.Find(Read(first).Jogo!.Cartas[0].Id)!; species.Habitat += " Revisão isolada de teste."; db.SaveChanges(); }
            Check(Read(first).QuestoesJson == original, "Edição de espécie não muda cartas de uma tentativa iniciada");
            Check((await other.Client.GetAsync("/Jornada/Jogar/" + first)).StatusCode == HttpStatusCode.NotFound, "Outra conta não lê as cartas da tentativa");
            Check((await Act(first, "estudar", "1", session: other)).StatusCode == HttpStatusCode.NotFound, "Outra conta não modifica as cartas da tentativa");
            Check((await guest.Client.PostAsync("/Jornada/Agir", new FormUrlEncodedContent(new Dictionary<string, string>()))).StatusCode == HttpStatusCode.Redirect, "Ações exigem autenticação");
            Check((await student.Client.PostAsync("/Jornada/Agir", new FormUrlEncodedContent(new Dictionary<string, string> { ["id"] = first.ToString() }))).StatusCode == HttpStatusCode.BadRequest, "Ações exigem antifalsificação");
            Check((await Act(first, "concluir", "999")).StatusCode == HttpStatusCode.BadRequest && Read(first).Pontos == 0, "Não existe comando de conclusão arbitrária ou pontuação enviada pelo cliente");
            await Act(first, "associar"); Check(Read(first).EstadoJogo.Etapa == "estudo", "Associação exige explorar todas as cartas");
            Check((await student.Client.GetAsync("/Avistamentos")).Headers.Location?.ToString() == "/Mapa", "Mapa permanece bloqueado durante as cartas");
            var adminPage = await admin.Client.GetStringAsync("/AdminJornada");
            Check(adminPage.Contains("Acervo da vers") && adminPage.Contains("Quiz"), "Administração distingue quiz atual e acervo anterior");

            using var chrome = Process.Start(new ProcessStartInfo(@"C:\Program Files\Google\Chrome\Application\chrome.exe") { UseShellExecute = false, CreateNoWindow = true, Arguments = "--headless=new --disable-gpu --hide-scrollbars --no-first-run --remote-debugging-port=9226 --user-data-dir=\"" + Path.Combine(fixture, "chrome") + "\" about:blank" })!;
            try
            {
                string? ws = null;
                for (int i = 0; i < 50; i++) { try { using var doc = JsonDocument.Parse(await health.GetStringAsync("http://127.0.0.1:9226/json/list")); ws = doc.RootElement.EnumerateArray().First(p => p.GetProperty("type").GetString() == "page").GetProperty("webSocketDebuggerUrl").GetString(); if (ws is not null) break; } catch { } await Task.Delay(200); }
                using var cdp = new Cdp(); await cdp.Connect(ws!); await cdp.Call("Page.enable", new { }); await cdp.Call("Network.enable", new { });
                await cdp.Call("Network.setCacheDisabled", new { cacheDisabled = true });
                await cdp.Call("Emulation.setEmulatedMedia",new{features=new[]{new{name="prefers-reduced-motion",value="no-preference"}}});
                await cdp.Call("Page.addScriptToEvaluateOnNewDocument", new { source = "window.__gameErrors=[];addEventListener('error',e=>{if(e.error)__gameErrors.push(e.message);});" });
                await cdp.Call("Page.addScriptToEvaluateOnNewDocument", new { source = "window.__motionLog=[];const originalAnimate=Element.prototype.animate;Element.prototype.animate=function(frames,options){const animation=originalAnimate.call(this,frames,options);__motionLog.push({tag:this.tagName,frames:JSON.stringify(frames),state:animation.playState});return animation;};" });
                foreach (Cookie cookie in student.Cookies.GetCookies(new Uri("http://127.0.0.1:5057"))) await cdp.Call("Network.setCookie", new { name = cookie.Name, value = cookie.Value, url = "http://127.0.0.1:5057", httpOnly = cookie.HttpOnly });
                async Task<JsonElement> Eval(string js)
                {
                    var response = await cdp.Call("Runtime.evaluate", new { expression = js, returnByValue = true, awaitPromise = true });
                    if (response.TryGetProperty("exceptionDetails", out var error)) throw new Exception(error.ToString());
                    return response.GetProperty("result").GetProperty("value");
                }
                async Task Navigate(string path, int width = 1440, string theme = "light")
                {
                    await cdp.Call("Emulation.setDeviceMetricsOverride", new { width, height = 960, deviceScaleFactor = 1, mobile = width < 650 });
                    await cdp.Call("Page.navigate", new { url = "http://127.0.0.1:5057" + path }); await Task.Delay(250);
                    for (var i = 0; i < 70; i++) { try { if ((await Eval("document.readyState==='complete' && location.pathname==='" + path + "'")).GetBoolean()) break; } catch { } await Task.Delay(100); }
                    if ((await Eval("document.documentElement.dataset.theme")).GetString() != theme) await Eval("document.querySelector('[data-theme-toggle]').click();true");
                    await Eval("document.fonts.ready.then(()=>true)");
                    await Eval("Promise.all([...document.images].map(i=>i.decode().catch(()=>{}))).then(()=>true)");
                }
                async Task Capture(string name)
                {
                    for(var i=0;i<60;i++){try{if((await Eval("document.readyState==='complete'")).GetBoolean())break;}catch{}await Task.Delay(100);}
                    await Eval("document.fonts.ready.then(()=>true)");
                    await Eval("Promise.all([...document.images].map(i=>i.decode().catch(()=>{}))).then(()=>true)");
                    Check((await Eval("document.documentElement.scrollWidth<=innerWidth+1")).GetBoolean(), "Sem transbordamento " + name);
                    Check((await Eval("window.__gameErrors.length===0")).GetBoolean(), "Sem erros JavaScript " + name);
                    Check((await Eval("[...document.images].every(i=>i.complete&&i.naturalWidth>0)")).GetBoolean(), "Imagens carregadas " + name);
                    var shot = await cdp.Call("Page.captureScreenshot", new { format = "png", captureBeyondViewport = true, fromSurface = true });
                    var dir = Path.Combine(output, "capturas"); Directory.CreateDirectory(dir); File.WriteAllBytes(Path.Combine(dir, name + ".png"), Convert.FromBase64String(shot.GetProperty("data").GetString()!));
                }
                async Task Click(string selector, bool keyboard = false)
                {
                    var version = (await Eval("document.querySelector('[name=revisao]')?.value||location.pathname")).GetString();
                    var selection=(await Eval("!!document.querySelector("+JsonSerializer.Serialize(selector)+")?.dataset.dragSource")).GetBoolean();
                    await Eval("document.querySelector(" + JsonSerializer.Serialize(selector) + ").focus();true");
                    if (keyboard)
                    {
                        await cdp.Call("Input.dispatchKeyEvent", new { type = "keyDown", key = "Enter", code = "Enter", windowsVirtualKeyCode = 13, text = "\r", unmodifiedText = "\r" });
                        await cdp.Call("Input.dispatchKeyEvent", new { type = "keyUp", key = "Enter", code = "Enter", windowsVirtualKeyCode = 13 });
                    }
                    else await Eval("document.querySelector(" + JsonSerializer.Serialize(selector) + ").click();true");
                    if(selection){Check((await Eval("document.querySelector("+JsonSerializer.Serialize(selector)+").getAttribute('aria-pressed')==='true'")).GetBoolean(),"Item selecionado por clique ou teclado");return;}
                    for (var i = 0; i < 100; i++) { await Task.Delay(100); try { if ((await Eval("!document.querySelector('#atividade[aria-busy]') && (document.querySelector('[name=revisao]')?.value||location.pathname)!==" + JsonSerializer.Serialize(version))).GetBoolean()) return; } catch { } }
                    throw new Exception("Ação do navegador não atualizou: " + selector + " " + (await Eval("document.querySelector('#jogo-feedback')?.textContent")).ToString());
                }
                async Task Drag(string sourceSelector,string targetSelector,bool touch=false,bool valid=true,bool cancel=false,double edgeMargin=0,bool waitForSave=true)
                {
                    await Eval("document.querySelector('.ec-mission-scene').scrollIntoView({block:'center'});true");
                    await Task.Delay(150);
                    var points=await Eval("JSON.stringify(["+JsonSerializer.Serialize(sourceSelector)+","+JsonSerializer.Serialize(targetSelector)+"].map(s=>{const r=document.querySelector(s).getBoundingClientRect();return {x:r.left+r.width/2,y:r.top+r.height/2,right:r.right};}))");
                    using var positions=JsonDocument.Parse(points.GetString()!);var list=positions.RootElement.EnumerateArray().ToArray();
                    var sx=list[0].GetProperty("x").GetDouble();var sy=list[0].GetProperty("y").GetDouble();var tx=list[1].GetProperty("x").GetDouble();var ty=list[1].GetProperty("y").GetDouble();
                    if(edgeMargin>0)tx=list[1].GetProperty("right").GetDouble()+edgeMargin;
                    Check(sy>0&&sy<960&&ty>0&&ty<960,"Origem e destino visíveis no arraste "+(touch?"touch":"mouse"));
                    var before=(await Eval("document.querySelector('[name=revisao]').value")).GetString();
                    if(touch){await cdp.Call("Emulation.setTouchEmulationEnabled",new{enabled=true,maxTouchPoints=1});await cdp.Call("Input.dispatchTouchEvent",new{type="touchStart",touchPoints=new[]{new{x=sx,y=sy,id=1,radiusX=7,radiusY=7,force=1}}});}
                    else await cdp.Call("Input.dispatchMouseEvent",new{type="mousePressed",x=sx,y=sy,button="left",buttons=1,clickCount=1});
                    for(var i=1;i<=10;i++){
                        var x=sx+(tx-sx)*i/10;var y=sy+(ty-sy)*i/10;
                        if(touch)await cdp.Call("Input.dispatchTouchEvent",new{type="touchMove",touchPoints=new[]{new{x,y,id=1,radiusX=7,radiusY=7,force=1}}});
                        else await cdp.Call("Input.dispatchMouseEvent",new{type="mouseMoved",x,y,button="left",buttons=1});
                        await Task.Delay(16);
                    }
                    Check((await Eval("!!document.querySelector('.ec-drag-ghost')")).GetBoolean(),"Objeto acompanha arraste "+(touch?"touch":"mouse"));
                    if(valid)Check((await Eval("!!document.querySelector('.ec-drop-hover')")).GetBoolean(),"Destino correto destacado durante arraste");
                    if(interacoes && valid && !cancel){var shot=await cdp.Call("Page.captureScreenshot",new{format="png",captureBeyondViewport=false,fromSurface=true});File.WriteAllBytes(Path.Combine(output,"capturas","arraste-"+(touch?"touch":"mouse")+"-ativo.png"),Convert.FromBase64String(shot.GetProperty("data").GetString()!));}
                    if(cancel)await cdp.Call("Input.dispatchKeyEvent",new{type="keyDown",key="Escape",code="Escape",windowsVirtualKeyCode=27});
                    if(touch)await cdp.Call("Input.dispatchTouchEvent",new{type="touchEnd",touchPoints=Array.Empty<object>()});
                    else await cdp.Call("Input.dispatchMouseEvent",new{type="mouseReleased",x=tx,y=ty,button="left",buttons=0,clickCount=1});
                    if(touch)await cdp.Call("Emulation.setTouchEmulationEnabled",new{enabled=false});
                    for(var i=0;i<90;i++){await Task.Delay(100);if((await Eval("!document.querySelector('.ec-drag-ghost')"+(waitForSave?"&&!document.querySelector('#atividade[aria-busy]')":"")+(valid&&!cancel&&waitForSave?"&&document.querySelector('[name=revisao]').value!=="+JsonSerializer.Serialize(before):""))).GetBoolean())break;}
                    Check((await Eval("!document.querySelector('.ec-drag-ghost')"+(waitForSave?"&&!document.querySelector('#atividade[aria-busy]')":""))).GetBoolean(),"Arraste finaliza sem objeto preso"+(waitForSave?" ou carregamento pendente":""));
                }
                var cartas = Read(first).Jogo!.Cartas;
                await Navigate("/Jornada/Jogar/" + first); await Capture("01-flashcards-desktop-claro");
                if(confiabilidade){
                    await cdp.Call("Network.emulateNetworkConditions",new{offline=false,latency=350,downloadThroughput=1000000,uploadThroughput=1000000});
                    await Eval("window.__originalCard=document.querySelector('.ec-flash-turn');__originalCard.click();true");
                    await Task.Delay(120);
                    Check((await Eval("(()=>{const el=document.querySelector('.ec-flash-inner')||document.querySelector('.ec-flash-turn');return getComputedStyle(el).transform!=='none'&&getComputedStyle(el).transform!=='matrix(1, 0, 0, 1, 0, 0)';})()")).GetBoolean(),"Virada visível começa antes da resposta de uma rede lenta");
                    await Capture("virada-visivel-rede-lenta");
                    for(var i=0;i<80;i++){await Task.Delay(100);if(!(await Eval("!!document.querySelector('#atividade[aria-busy]')")).GetBoolean())break;}
                    Check((await Eval("window.__originalCard===document.querySelector('.ec-flash-turn')")).GetBoolean(),"Salvar estudo preserva o elemento animado no DOM");
                    await cdp.Call("Network.emulateNetworkConditions",new{offline=false,latency=0,downloadThroughput=-1,uploadThroughput=-1});
                    await Click("[data-focus-key='carta-"+cartas[0].Id+"']");
                }
                await Click("[data-focus-key='carta-" + cartas[0].Id + "']", true);
                Check(Read(first).EstadoJogo.Estudadas.Contains(cartas[0].Id), "Flashcard vira pelo teclado e registra estudo");
                Check((await Eval("document.querySelector('.ec-flash-inner')!==null&&parseFloat(getComputedStyle(document.querySelector('.ec-flash-inner')).transitionDuration)>0")).GetBoolean(),"Frente e verso usam transição 3D no mesmo elemento");
                await cdp.Call("Emulation.setEmulatedMedia",new{features=new[]{new{name="prefers-reduced-motion",value="reduce"}}});
                await Eval("__motionLog=[];true");await Click("[data-focus-key='carta-"+cartas[0].Id+"']");
                Check((await Eval("__motionLog.length===0&&parseFloat(getComputedStyle(document.querySelector('.ec-flash-inner')).transitionDuration)===0")).GetBoolean(),"Preferência por movimento reduzido desativa animações de ações");
                if(confiabilidade){await Eval("document.querySelector('[data-game-motion-toggle]').click();true");Check((await Eval("document.documentElement.dataset.gameMotion==='on'&&parseFloat(getComputedStyle(document.querySelector('.ec-flash-inner')).transitionDuration)>0")).GetBoolean(),"Usuário pode ativar animações no jogo mesmo quando o sistema reduz movimento");}
                await cdp.Call("Emulation.setEmulatedMedia",new{features=new[]{new{name="prefers-reduced-motion",value="no-preference"}}});
                await Click("[data-focus-key='carta-"+cartas[0].Id+"']");
                await Capture("02-flashcard-verso");
                if(confiabilidade){
                    await cdp.Call("Network.emulateNetworkConditions",new{offline=false,latency=250,downloadThroughput=1000000,uploadThroughput=1000000});
                    foreach(var index in new[]{1,2,1}){await Eval("document.querySelector('[data-focus-key=carta-"+cartas[index].Id+"]').click();true");await Task.Delay(35);}
                    Check((await Eval("document.querySelector('[data-carta=\""+cartas[1].Id+"\"]').classList.contains('is-flipped')")).GetBoolean(),"Cliques rápidos mostram imediatamente a última carta escolhida");
                    for(var i=0;i<100;i++){await Task.Delay(100);if(!(await Eval("!!document.querySelector('#atividade[aria-busy]')")).GetBoolean())break;}
                    var fastState=Read(first).EstadoJogo;
                    Check(fastState.Estudadas.Contains(cartas[1].Id)&&fastState.Estudadas.Contains(cartas[2].Id)&&fastState.CartaAberta==cartas[1].Id,"Fila salva as cartas clicadas rapidamente sem descartar ações");
                    await cdp.Call("Network.emulateNetworkConditions",new{offline=false,latency=0,downloadThroughput=-1,uploadThroughput=-1});
                }
                var stale = Read(first).EstadoJogo.Revisao;
                await Act(first, "estudar", cartas[1].Id.ToString());
                Check((await Act(first, "estudar", cartas[1].Id.ToString(), version: stale, json: true)).StatusCode == HttpStatusCode.Conflict, "Reenvio concorrente não repete a ação");
                for (var i = 2; i < cartas.Count; i++) await Act(first, "estudar", cartas[i].Id.ToString());
                var newLogin = new Session(); await newLogin.Login(email, password);
                Check((await newLogin.Client.GetStringAsync("/Jornada/Jogar/" + first)).Contains("Montar minhas associa"), "Cartas exploradas são retomadas após novo login");
                await Navigate("/Jornada/Jogar/" + first, 390, "dark"); await Capture("03-flashcards-celular-escuro");
                await Click("#atividade button.ec-button[type=submit]");
                await Click("[data-focus-key='foto-" + cartas[0].Id + "']");
                Check((await Eval("document.querySelector('[aria-describedby^=pista-foto]')!==null && document.body.textContent.includes('Pista da ficha:')")).GetBoolean(),"Associação oferece pista textual e descrição acessível da fotografia");
                await Click("[data-focus-key='nome-" + cartas[1].Id + "']");
                Check(Read(first).Pontos == 0 && Read(first).EstadoJogo.Revisoes == 1, "Associação incorreta dá pista e não concede objetivo");
                await Capture("04-associacao-incorreta-celular");
                await Navigate("/Jornada/Jogar/" + first, 1440, "dark"); await Capture("05-associacao-desktop-escuro");
                foreach (var carta in cartas) { await Click("[data-focus-key='foto-" + carta.Id + "']"); await Click("[data-focus-key='nome-" + carta.Id + "']", true);if(carta==cartas[0])Check((await Eval("__motionLog.some(a=>a.frames.includes('strokeDashoffset'))")).GetBoolean(),"Par correto desenha uma conexão animada entre as cartas"); }
                Check(Read(first).Aprovada && Read(first).Pontos == cartas.Count, "Todas as associações conquistam o avatar no servidor");
                await Capture("06-resultado-cartas");

                var quizFailed = await Start(2); await CompleteQuiz(student, Read(quizFailed), 0);
                Check(!Read(quizFailed).Aprovada && Read(quizFailed).ConcluidaEm is not null, "Quiz mantém erro, reprovação e nova tentativa");
                var second = await Start(2);
                await Navigate("/Jornada/Jogar/" + second, 390, "dark"); await Capture("07-quiz-guardiao");
                var minimum = (int)Math.Ceiling(Read(second).Total * .7);
                await CompleteQuiz(student, Read(second), minimum);
                Check(Read(second).Aprovada && Read(second).Aproveitamento >= 70, "Quiz mantém mínimo de 70% e conquista do escudo");
                Check((await student.Client.GetAsync("/Avistamentos")).Headers.Location?.ToString() == "/Mapa", "Duas etapas não desbloqueiam mapa");
                var third = await Start(3);
                Check(Read(third).Jogo?.Tipo == "missao" && Read(third).Questoes.Count == 0, "Missão tem cenário próprio sem repetir perguntas do Detetive ou Guardião");
                Check((await Act(third, "interagir", "ave", "radio")).StatusCode == HttpStatusCode.Redirect && Read(third).Pontos == 0, "Chamar equipe sem investigar não pula etapas");
                await Navigate("/Jornada/Jogar/" + third); await Capture("08-missao-desktop-claro");
                Check(!(await Eval("document.querySelector('#jogo-feedback').textContent")).GetString()!.Contains("Par descoberto"),"Feedback de uma partida não aparece em outra atividade");
                await Navigate("/Jornada/Jogar/" + third, 768, "dark"); await Capture("09-missao-tablet-escuro");
                await Navigate("/Jornada/Jogar/" + third, 390, "dark"); await Capture("10-missao-celular-escuro");
                Check((await Eval("[...document.querySelectorAll('.ec-scene-object')].every(b=>b.getBoundingClientRect().width>=44&&b.getBoundingClientRect().height>=44)")).GetBoolean(), "Alvos do cenário têm pelo menos 44 pixels no celular");
                await Eval("document.querySelector('[name=ferramenta][value=recolher]').click();true");
                await Click("[data-focus-key=objeto-garrafa]", true);
                Check(Read(third).EstadoJogo.Carregando is null && Read(third).Pontos == 0, "Selecionar resíduo pelo teclado aguarda confirmação do destino");
                await Capture("11-missao-residuo-na-bolsa");
                await Click("[data-focus-key=objeto-descarte]");
                Check(Read(third).Pontos == 1 && Read(third).EstadoJogo.Carregando is null, "Descarte muda o cenário e registra objetivo");
                await Act(third, "interagir", "descarte", "recolher"); Check(Read(third).Pontos == 1, "Repetir descarte vazio não duplica pontos");
                Check((await Act(third,"mover","lata",destination:"ave")).StatusCode==HttpStatusCode.BadRequest&&Read(third).Pontos==1,"Servidor rejeita destino incompatível sem conceder pontos");
                await Navigate("/Jornada/Jogar/"+third);
                await Drag("[data-drag-source=lata]",".ec-scene-location",valid:false);
                Check(Read(third).Pontos==1,"Soltar fora da lixeira devolve o objeto sem pontuar");
                await Drag("[data-drag-source=lata]","[data-drop-target=descarte]",cancel:true);
                Check(Read(third).Pontos==1,"Escape cancela o arraste sem alterar progresso");
                if(confiabilidade){
                    await cdp.Call("Network.emulateNetworkConditions",new{offline=false,latency=350,downloadThroughput=1000000,uploadThroughput=1000000});
                    await Drag("[data-drag-source=lata]","[data-drop-target=descarte]",edgeMargin:10,waitForSave:false);
                    Check((await Eval("document.querySelector('#atividade').hasAttribute('aria-busy')")).GetBoolean(),"Arraste seguinte pode começar com salvamento anterior pendente");
                    await Drag("[data-drag-source=embalagem]","[data-drop-target=descarte]",waitForSave:false);
                    for(var i=0;i<120;i++){await Task.Delay(100);if(!(await Eval("document.querySelector('#atividade').hasAttribute('aria-busy')")).GetBoolean())break;}
                    Check(Read(third).Pontos==3&&Read(third).EstadoJogo.Objetivos.Contains("descarte-lata")&&Read(third).EstadoJogo.Objetivos.Contains("descarte-embalagem"),"Arrastes rápidos e soltura na borda são salvos sem perder ou duplicar resíduos");
                    await cdp.Call("Network.emulateNetworkConditions",new{offline=false,latency=0,downloadThroughput=-1,uploadThroughput=-1});
                    await Navigate("/Jornada/Jogar/"+third,390,"dark");
                }else{
                    await Drag("[data-drag-source=lata]","[data-drop-target=descarte]");Check(Read(third).Pontos==2,"Arraste real com mouse destina resíduo no servidor");
                    await Navigate("/Jornada/Jogar/"+third,390,"dark");
                    await Drag("[data-drag-source=embalagem]","[data-drop-target=descarte]",true);Check(Read(third).Pontos==3,"Arraste real por toque destina resíduo no servidor");
                }
                Check((await Eval("document.querySelector('[data-scene-stage]').dataset.sceneStage==='2'&&document.querySelector('[data-drag-source=binoculos]')!==null")).GetBoolean(),"Missão avança para a ave e apresenta somente o item necessário");
                await Capture("17-ave-celular-item-claro");
                await Drag("[data-drag-source=binoculos]","[data-drop-target=ave]",true);
                await Act(third, "interagir", "ave", "recolher");
                Check(Read(third).Pontos == 4 && !Read(third).EstadoJogo.Objetivos.Contains("ave-atendida"), "Recolher não captura ou solta a ave");
                await Navigate("/Jornada/Jogar/"+third,390,"dark");
                await Drag("[data-drag-source=sinalizacao]","[data-drop-target=ave]",true);
                await Navigate("/Jornada/Jogar/"+third);
                await Drag("[data-drag-source=radio]","[data-drop-target=ave]");
                Check(JornadaJogosService.MissaoConcluida("ave", Read(third).EstadoJogo), "Observação, sinalização e rádio trazem equipe para a ave");
                Check((await student.Client.GetAsync("/Avistamentos")).Headers.Location?.ToString() == "/Mapa", "Missão incompleta ainda protege mapa");
                await Navigate("/Jornada/Jogar/" + third, 1440, "dark"); await Capture("12-missao-equipe-no-local");
                await Drag("[data-drag-source=binoculos]","[data-drop-target=atalho]");
                await Navigate("/Jornada/Jogar/"+third,390,"dark");
                await Drag("[data-drag-source=placa]","[data-drop-target=atalho]",true);
                Check(Read(third).Aprovada && Read(third).Pontos == 8, "Três missões e oito objetivos concluem jogo no servidor");
                Check((await Eval("document.querySelectorAll('.ec-scene-object:not(:disabled)').length===0")).GetBoolean(), "Cenário concluído fica disponível para revisão sem novas ações");
                await Capture("13-missao-concluida");
                Check((await student.Client.GetAsync("/Avistamentos")).IsSuccessStatusCode, "Três experiências desbloqueiam mapa");
                var completedJson = Read(third).RespostasJson; await Act(third, "interagir", "ave", "radio"); Check(Read(third).RespostasJson == completedJson, "Ação depois da conclusão não altera histórico");
                await Navigate("/Jornada/Resultado/" + third); await Capture("14-resultado-missao");
                await Navigate("/Jornada/Progresso", 390, "dark"); await Capture("15-conquistas-tres-experiencias");
                Check((await student.Client.GetStringAsync("/Jornada/Progresso")).Contains("data-guardian-accessory"), "Avatar, escudo e mapa continuam integrados");
                var fresh = new Session(); await fresh.Login(email, password); Check((await fresh.Client.GetAsync("/Avistamentos")).IsSuccessStatusCode, "Novo login preserva desbloqueio final");
                var replay = await Start(3); Check(replay != third && Read(replay).Pontos == 0, "Nova partida cria cenário limpo sem revogar recompensa");
                await Navigate("/Jornada/Jogar/" + replay, 390, "light");
                await Eval("document.querySelector('[name=ferramenta][value=recolher]').click();document.querySelector('[data-focus-key=objeto-lata]').focus();true");
                await cdp.Call("Emulation.setScriptExecutionDisabled", new { value = true });
                await cdp.Call("Input.dispatchKeyEvent", new { type = "keyDown", key = "Enter", code = "Enter", windowsVirtualKeyCode = 13, text = "\r", unmodifiedText = "\r" });
                await cdp.Call("Input.dispatchKeyEvent", new { type = "keyUp", key = "Enter", code = "Enter", windowsVirtualKeyCode = 13 });
                for(var i=0;i<40 && Read(replay).EstadoJogo.Carregando!="lata";i++)await Task.Delay(100);
                Check(Read(replay).EstadoJogo.Carregando=="lata", "Formulário nativo executa ação com JavaScript desativado");
                await cdp.Call("Emulation.setScriptExecutionDisabled", new { value = false });
                await Navigate("/Jornada/Jogar/" + replay, 390, "light"); await Capture("16-missao-celular-claro");
                if(confiabilidade){
                    await cdp.Call("Network.emulateNetworkConditions",new{offline=true,latency=0,downloadThroughput=0,uploadThroughput=0});
                    await Drag("[data-drag-source=lata]","[data-drop-target=descarte]",waitForSave:false);
                    Check(Read(replay).Pontos==0&&(await Eval("document.querySelectorAll('.is-pending-move').length===0&&document.querySelector('#jogo-feedback button')?.textContent==='Recuperar atividade'")).GetBoolean(),"Falha de conexão restaura o objeto e oferece recuperação sem conceder pontos");
                    await cdp.Call("Network.emulateNetworkConditions",new{offline=false,latency=0,downloadThroughput=-1,uploadThroughput=-1});
                    await Eval("document.querySelector('#jogo-feedback button').click();true");await Task.Delay(500);
                    await Navigate("/Jornada/Jogar/"+replay,390,"light");
                    Check(Read(replay).EstadoJogo.Carregando=="lata","Recuperação conserva o resíduo que já estava salvo na bolsa");
                    await Drag("[data-drag-source=lata]","[data-drop-target=descarte]");
                    Check(Read(replay).Pontos==1&&Read(replay).EstadoJogo.Carregando is null,"Coleta volta a funcionar após recuperar a conexão");
                }
            }
            finally { if (!chrome.HasExited) chrome.Kill(true); }

            // Compatibilidade: uma tentativa iniciada no quiz antigo requer confirmação antes da transição.
            int legacy; string questions; string answers;
            using (var db = Db())
            {
                var otherId = db.Usuarios.Single(u => u.Email == otherEmail).Id;
                var q = db.JornadaPerguntas.First(q => q.Desafio == 1);
                questions = JsonSerializer.Serialize(new[] { q }); answers = JsonSerializer.Serialize(new[] { q.Correta });
                var t = new JornadaTentativa { UsuarioId = otherId, Desafio = 1, Total = 2, Pontos = 1, QuestoesJson = questions, RespostasJson = answers }; db.JornadaTentativas.Add(t); db.SaveChanges(); legacy = t.Id;
            }
            Check((await other.Client.GetStringAsync("/Jornada/Jogar/" + legacy)).Contains("Preservar tentativa"), "Tentativa antiga em andamento apresenta transição, sem repetir quiz");
            await other.Post("/Jornada/Iniciar", new() { ["desafio"] = "1" }, "/Jornada"); Check(Read(legacy).ConcluidaEm is null, "Transição não encerra tentativa sem confirmação na interface");
            await other.Post("/Jornada/Iniciar", new() { ["desafio"] = "1", ["atualizar"] = "true" }, "/Jornada");
            Check(Read(legacy).ArquivadaNaAtualizacao && Read(legacy).QuestoesJson == questions && Read(legacy).Respostas.Count == 1, "Transição preserva questões e respostas antigas no histórico");
            Check(WebUtility.HtmlDecode(await other.Client.GetStringAsync("/Jornada/Resultado/" + legacy)).Contains("histórico foi preservado"), "Resultado histórico continua acessível");
            using (var db = Db()) {var oldUser=Read(legacy).UsuarioId; var active=db.JornadaTentativas.Single(t=>t.UsuarioId==oldUser && t.ConcluidaEm==null); Check(active.Jogo?.Tipo=="cartas" && active.Pontos==0,"Transição cria uma única tentativa nova de cartas");}
            File.WriteAllText(Path.Combine(output, "resultado.json"), JsonSerializer.Serialize(new { executadoEm = DateTimeOffset.Now, total = checks.Count, checks }, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine("JOGOS VALIDADOS: " + checks.Count);
        }
        catch (Exception ex)
        {
            File.WriteAllText(Path.Combine(output, "falha.txt"), ex.ToString());
            File.WriteAllText(Path.Combine(output, "resultado.json"), JsonSerializer.Serialize(new { total = checks.Count, checks }, new JsonSerializerOptions { WriteIndented = true }));
            Console.Error.WriteLine(ex); Environment.ExitCode = 1;
        }
        finally { if (!server.HasExited) server.Kill(true); await server.WaitForExitAsync(); File.WriteAllText(Path.Combine(output, "servidor.log"), await stdout + await stderr); }
    }

    public static async Task CompleteQuiz(Session session, JornadaTentativa t, int correct)
    {
        for (var i = t.Respostas.Count; i < t.Questoes.Count; i++)
        {
            var q = t.Questoes[i];
            var response = await session.Post("/Jornada/Responder", new() { ["id"] = t.Id.ToString(), ["posicao"] = i.ToString(), ["resposta"] = (i < correct ? q.Correta : q.Correta % 4 + 1).ToString() }, "/Jornada/Jogar/" + t.Id);
            if (response.StatusCode != HttpStatusCode.Redirect) throw new Exception("Resposta de quiz não foi registrada.");
        }
    }
}
