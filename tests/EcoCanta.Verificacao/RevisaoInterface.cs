using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;

internal static class RevisaoInterface
{
    public static async Task Run(string root)
    {
        var output = Path.Combine(root, "documentacao", "revisao-20261114");
        var fixture = Path.Combine(root, "scratch", "revisao-20261114", "execucao-" + DateTime.Now.ToString("HHmmss"));
        Directory.CreateDirectory(output); Directory.CreateDirectory(fixture);
        var dbPath = Path.Combine(fixture, "teste.db");
        var web = Path.Combine(root, "TCC_Grupo12_III");
        using (var source = new SqliteConnection($"Data Source={Path.Combine(web, "app.db")};Mode=ReadOnly"))
        {
            source.Open();
            using var target = new SqliteConnection($"Data Source={dbPath}");
            target.Open(); source.BackupDatabase(target);
        }
        TestApplication.PrepareCopy(dbPath);
        AppDbContext Db() => new(new DbContextOptionsBuilder<AppDbContext>().UseSqlite($"Data Source={dbPath}").Options);
        var checks = new List<object>();
        void Check(bool pass, string name)
        {
            checks.Add(new { teste = name, passou = pass });
            Console.WriteLine((pass ? "OK " : "FALHOU ") + name);
            if (!pass) throw new Exception(name);
        }
        var password = "Teste!" + Guid.NewGuid().ToString("N");
        var adminEmail = Guid.NewGuid().ToString("N") + "@teste.invalid";
        using (var db = Db())
        {
            foreach (var u in db.Usuarios) { u.Nome = "Conta demonstrativa " + u.Id; u.Email = "conta" + u.Id + "@exemplo.invalid"; }
            db.Usuarios.Add(new Usuario { Nome = "Administrador de demonstração", Email = adminEmail, Senha = BCrypt.Net.BCrypt.HashPassword(password), IsAdmin = true });
            db.SaveChanges();
        }
        var psi = new ProcessStartInfo("dotnet") { WorkingDirectory = web, UseShellExecute = false, CreateNoWindow = true, RedirectStandardOutput = true, RedirectStandardError = true };
        psi.ArgumentList.Add(TestApplication.Dll(root)); psi.ArgumentList.Add("--urls"); psi.ArgumentList.Add("http://127.0.0.1:5057");
        psi.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        psi.Environment["ConnectionStrings__DefaultConnection"] = $"Data Source={dbPath}";
        psi.Environment["DataDirectory"] = fixture;
        psi.Environment["Logging__LogLevel__Default"] = "Warning"; psi.Environment["Logging__LogLevel__Microsoft"] = "Warning";
        using var server = Process.Start(psi)!;
        File.WriteAllText(Path.Combine(fixture, "servidor.pid"), server.Id.ToString());
        var stdout = server.StandardOutput.ReadToEndAsync(); var stderr = server.StandardError.ReadToEndAsync();
        try
        {
            var guest = new Session(); var student = new Session(); var admin = new Session();
            var ready = false;
            using var health = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:5057"), Timeout = TimeSpan.FromSeconds(2) };
            var warmup = Stopwatch.StartNew();
            while (warmup.Elapsed < TimeSpan.FromSeconds(25)) { try { if ((await health.GetAsync("/")).StatusCode == HttpStatusCode.OK) { ready = true; break; } } catch { } if (server.HasExited) break; await Task.Delay(200); }
            if (!ready && server.HasExited) Console.Error.WriteLine(await stderr);
            Check(ready, "Aplicação inicia na cópia isolada do banco");
            using (var db = Db()) Check(!db.Database.HasPendingModelChanges(), "Melhorias de interface não exigem alteração do modelo do banco");
            var landing = await guest.Client.GetStringAsync("/Mapa");
            Check(landing.Contains("Como desbloquear o mapa") && !landing.Contains("const items="), "Visitante encontra apresentação do mapa sem receber avistamentos");
            var home = await guest.Client.GetStringAsync("/");
            Check(home.Contains("ec-nav-map") && home.Contains("ec-map-access"), "Mapa visível no menu principal e na página inicial");
            var email = Guid.NewGuid().ToString("N") + "@teste.invalid";
            await student.Post("/Cadastro", new() { ["Nome"] = "Estudante de demonstração", ["Email"] = email, ["Senha"] = password }, "/Cadastro");
            await student.Login(email, password); await admin.Login(adminEmail, password);
            int uid; using (var db = Db()) uid = db.Usuarios.Single(u => u.Email == email).Id;
            Check((await admin.Client.GetAsync("/Mapa")).Headers.Location?.ToString().Contains("Avistamentos") == true, "Administrador abre o mapa diretamente pelo novo atalho");
            foreach (var path in new[] { "/Avistamentos", "/Avistamentos/Registrar", "/Avistamentos/Criar" })
            {
                var blocked = await student.Client.GetAsync(path);
                Check(blocked.StatusCode == HttpStatusCode.Redirect && blocked.Headers.Location?.ToString() == "/Mapa", "Bloqueio no servidor orienta o estudante: " + path);
            }

            async Task<int> Attempt(int level, bool correct)
            {
                await student.Post("/Jornada/Iniciar", new() { ["desafio"] = level.ToString() }, "/Jornada");
                JornadaTentativa t; using (var db = Db()) t = db.JornadaTentativas.AsNoTracking().Where(t => t.UsuarioId == uid && t.Desafio == level).OrderByDescending(t => t.Id).First();
                if(t.Jogo is not null){
                    JornadaTentativa Read(int id){using var db=Db();return db.JornadaTentativas.AsNoTracking().Single(t=>t.Id==id);}
                    await JogarAtividade.Completar(student,Read,t.Id,correct);return t.Id;
                }
                for (var i = t.Respostas.Count; i < t.Questoes.Count; i++)
                {
                    var q = t.Questoes[i];
                    var result = await student.Post("/Jornada/Responder", new() { ["id"] = t.Id.ToString(), ["posicao"] = i.ToString(), ["resposta"] = (correct ? q.Correta : q.Correta % 4 + 1).ToString() }, "/Jornada/Jogar/" + t.Id);
                    if (result.StatusCode != HttpStatusCode.Redirect) throw new Exception("Falha ao registrar resposta.");
                }
                return t.Id;
            }
            async Task RewardState(int stage, bool expected)
            {
                var html = await student.Client.GetStringAsync("/Jornada/Progresso");
                Check(html.Contains($"data-reward-stage=\"{stage}\" data-earned=\"{expected.ToString().ToLowerInvariant()}\""), "Estado persistido da recompensa " + stage + ": " + expected);
            }
            var failed = await Attempt(1, false);
            Check(!(await student.Client.GetStringAsync("/Jornada/Resultado/" + failed)).Contains("class=\"ec-earned-reward\""), "Tentativa insuficiente não apresenta recompensa conquistada");
            await RewardState(1, false);
            var first = await Attempt(1, true); await RewardState(1, true); await RewardState(2, false);
            var firstResult = await student.Client.GetStringAsync("/Jornada/Resultado/" + first);
            Check(firstResult.Contains("ec-earned-reward") && firstResult.Contains("ec-guardian"), "Primeiro desafio mostra o avatar como recompensa no resultado");
            var mapAfterOne = await student.Client.GetStringAsync("/Mapa");
            Check(mapAfterOne.Contains("1 de 3") && mapAfterOne.Contains("#desafio-2"), "Mapa bloqueado informa progresso e direciona ao segundo desafio");
            await Attempt(1, false); await RewardState(1, true);
            var second = await Attempt(2, true); await RewardState(2, true);
            Check((await student.Client.GetStringAsync("/Jornada/Progresso")).Contains("data-guardian-accessory"), "Segundo desafio equipa o escudo no avatar");
            Check((await student.Client.GetStringAsync("/Mapa")).Contains("#desafio-3"), "Apresentação do mapa aponta o último desafio pendente");

            using var chrome = Process.Start(new ProcessStartInfo(@"C:\Program Files\Google\Chrome\Application\chrome.exe")
            {
                UseShellExecute = false, CreateNoWindow = true,
                Arguments = "--headless=new --disable-gpu --hide-scrollbars --no-first-run --remote-debugging-port=9225 --user-data-dir=\"" + Path.Combine(fixture, "chrome") + "\" about:blank"
            })!;
            try
            {
                using var http = new HttpClient(); string? ws = null;
                for (var i = 0; i < 60; i++) { try { using var d = JsonDocument.Parse(await http.GetStringAsync("http://127.0.0.1:9225/json/list")); ws = d.RootElement.EnumerateArray().First(x => x.GetProperty("type").GetString() == "page").GetProperty("webSocketDebuggerUrl").GetString(); if (ws != null) break; } catch { } await Task.Delay(200); }
                using var cdp = new Cdp(); await cdp.Connect(ws!);
                await cdp.Call("Page.enable", new { }); await cdp.Call("Network.enable", new { });
                await cdp.Call("Network.setCacheDisabled", new { cacheDisabled = true });
                await cdp.Call("Page.addScriptToEvaluateOnNewDocument", new { source = "window.__interfaceErrors=[];addEventListener('error',e=>{if(e.error)window.__interfaceErrors.push(e.message);});" });
                async Task<JsonElement> Eval(string js)
                {
                    var r = await cdp.Call("Runtime.evaluate", new { expression = js, returnByValue = true, awaitPromise = true });
                    if (r.TryGetProperty("exceptionDetails", out var error)) throw new Exception(error.ToString());
                    return r.GetProperty("result").GetProperty("value");
                }
                async Task Cookies(Session session)
                {
                    await cdp.Call("Network.clearBrowserCookies", new { });
                    foreach (Cookie c in session.Cookies.GetCookies(new Uri("http://127.0.0.1:5057")))
                        await cdp.Call("Network.setCookie", new { name = c.Name, value = c.Value, url = "http://127.0.0.1:5057", httpOnly = c.HttpOnly });
                }
                async Task Navigate(string url, int width = 1440)
                {
                    await cdp.Call("Emulation.setDeviceMetricsOverride", new { width, height = 960, deviceScaleFactor = 1, mobile = width < 650 });
                    await cdp.Call("Page.navigate", new { url = "http://127.0.0.1:5057" + url });
                    for (var i = 0; i < 60; i++) { await Task.Delay(100); try { if ((await Eval("document.readyState==='complete'&&location.host==='127.0.0.1:5057'")).GetBoolean()) break; } catch { } }
                    await Eval("document.fonts.ready.then(()=>true)");
                    await Eval("Promise.all([...document.images].map(i=>{i.loading='eager';return i.decode().catch(()=>{});})).then(()=>true)");
                }
                async Task Theme(string theme)
                {
                    if ((await Eval("document.documentElement.dataset.theme")).GetString() != theme) await Eval("document.querySelector('[data-theme-toggle]').click();true");
                    Check((await Eval("document.documentElement.dataset.theme")).GetString() == theme, "Alternância para modo " + theme);
                }
                var captureDir = Path.Combine(output, "capturas"); Directory.CreateDirectory(captureDir);
                var contrastScript = """
                    (()=>{
                        const rgb=c=>(c.match(/[\d.]+/g)||[]).slice(0,3).map(Number);
                        const luminance=c=>rgb(c).map(v=>{v/=255;return v<=.04045?v/12.92:Math.pow((v+.055)/1.055,2.4);}).reduce((s,v,i)=>s+v*[.2126,.7152,.0722][i],0);
                        const failures=[];
                        for(const el of document.querySelectorAll('h1,h2,.ec-lead,.ec-eyebrow,.ec-step span,.ec-card p,.ec-map-access p,.ec-button,.btn-eco,.ec-theme-toggle,.ec-field label,.form-label')){
                            if(!el.getBoundingClientRect().height)continue;
                            const style=getComputedStyle(el);
                            let node=el,bg;
                            while(node){const color=getComputedStyle(node).backgroundColor;if(color!=='transparent'&&color!=='rgba(0, 0, 0, 0)'){bg=color;break;}node=node.parentElement;}
                            if(!bg)continue;
                            const a=luminance(style.color),b=luminance(bg),ratio=(Math.max(a,b)+.05)/(Math.min(a,b)+.05);
                            const large=parseFloat(style.fontSize)>=24||(parseFloat(style.fontSize)>=18.66&&parseInt(style.fontWeight)>=700);
                            if(ratio<(large?3:4.5)-.02)failures.push({element:el.tagName,classe:el.className,contraste:ratio.toFixed(2)});
                        }
                        return JSON.stringify(failures);
                    })()
                    """;
                async Task Capture(string name, string url, int width, string theme)
                {
                    await Navigate(url, width); await Theme(theme);
                    Check((await Eval("document.documentElement.scrollWidth<=innerWidth+1")).GetBoolean(), "Sem transbordamento: " + name);
                    Check((await Eval("[...document.images].filter(i=>i.getBoundingClientRect().height>0&&(!i.complete||!i.naturalWidth)).length===0")).GetBoolean(), "Imagens carregadas: " + name);
                    Check((await Eval("window.__interfaceErrors.length===0")).GetBoolean(), "Sem erros JavaScript: " + name);
                    var contrast = (await Eval(contrastScript)).GetString();
                    if (contrast != "[]") Console.WriteLine("Contraste a revisar: " + contrast);
                    Check(contrast == "[]", "Contraste dos componentes principais: " + name);
                    var shot = await cdp.Call("Page.captureScreenshot", new { format = "png", captureBeyondViewport = true, fromSurface = true });
                    File.WriteAllBytes(Path.Combine(captureDir, name + ".png"), Convert.FromBase64String(shot.GetProperty("data").GetString()!));
                }

                await Cookies(guest);
                await cdp.Call("Emulation.setEmulatedMedia", new { features = new[] { new { name = "prefers-color-scheme", value = "dark" } } });
                await Navigate("/");
                Check((await Eval("document.documentElement.dataset.theme==='dark'")).GetBoolean(), "Primeiro acesso respeita o tema escuro do sistema");
                await Theme("light"); await Navigate("/Mapa");
                Check((await Eval("document.documentElement.dataset.theme==='light'")).GetBoolean(), "Preferência explícita prevalece sobre o tema do sistema ao navegar");
                await Theme("dark");
                await cdp.Call("Page.reload", new { ignoreCache = true }); await Task.Delay(600);
                Check((await Eval("document.documentElement.dataset.theme==='dark'&&localStorage.getItem('ecocanta.theme')==='dark'")).GetBoolean(), "Preferência escura persiste após recarregar");
                Check((await Eval("document.querySelector('script[src*=\"theme.js\"]').compareDocumentPosition(document.querySelector('link[rel=\"stylesheet\"]'))&Node.DOCUMENT_POSITION_FOLLOWING")).GetInt32() != 0, "Tema inicial aplicado antes das folhas de estilo");
                await Capture("inicio-claro", "/", 1440, "light");
                await Capture("inicio-escuro", "/", 1440, "dark");
                await Capture("mapa-visitante-escuro", "/Mapa", 1440, "dark");
                await Capture("login-escuro", "/Login", 1440, "dark");
                await Capture("cadastro-escuro", "/Cadastro", 390, "dark");
                await Capture("inicio-escuro-tablet", "/", 768, "dark");
                await Capture("inicio-escuro-celular", "/", 390, "dark");
                Check((await Eval("getComputedStyle(document.querySelector('[data-theme-toggle]')).display!=='none'")).GetBoolean(), "Alternância de tema visível com o menu móvel fechado");
                await Eval("document.querySelector('.ec-menu').click();true");
                Check((await Eval("[...document.querySelectorAll('#navegacao a')].some(a=>a.textContent.trim()==='Mapa'&&a.getAttribute('href')==='/Mapa'&&a.getBoundingClientRect().height>0)")).GetBoolean(), "Mapa fácil de encontrar no menu móvel");
                await cdp.Call("Input.dispatchKeyEvent", new { type = "keyDown", key = "Escape", code = "Escape", windowsVirtualKeyCode = 27 });
                await Eval("document.querySelector('[data-theme-toggle]').focus();true");
                await cdp.Call("Input.dispatchKeyEvent", new { type = "keyDown", key = "Enter", code = "Enter", windowsVirtualKeyCode = 13, text = "\r", unmodifiedText = "\r" });
                await cdp.Call("Input.dispatchKeyEvent", new { type = "keyUp", key = "Enter", code = "Enter", windowsVirtualKeyCode = 13 });
                Check((await Eval("document.documentElement.dataset.theme==='light'&&document.querySelector('[data-theme-toggle]').getAttribute('aria-pressed')==='false'")).GetBoolean(), "Alternância de tema funciona pelo teclado e atualiza estado acessível");

                await Cookies(student);
                await Capture("mapa-bloqueado-escuro", "/Mapa", 1440, "dark");
                await Capture("mapa-bloqueado-celular", "/Mapa", 390, "dark");
                await Capture("recompensas-escuro", "/Jornada/Progresso", 1440, "dark");
                await Capture("recompensas-claro", "/Jornada/Progresso", 1440, "light");
                await Capture("recompensas-celular", "/Jornada/Progresso", 390, "dark");
                await Capture("resultado-avatar-escuro", "/Jornada/Resultado/" + first, 1440, "dark");
                await Capture("resultado-escudo-escuro", "/Jornada/Resultado/" + second, 1440, "dark");
                await Capture("catalogo-escuro", "/Especies", 1440, "dark");
                int species; using (var db = Db()) species = db.Especies.First().Id;
                await Capture("ficha-escuro", "/Especies/Detalhe/" + species, 390, "dark");
                await Capture("jornada-escuro", "/Jornada", 1440, "dark");
                await student.Post("/Jornada/Iniciar", new() { ["desafio"] = "1" }, "/Jornada");
                int active; using (var db = Db()) active = db.JornadaTentativas.Where(t => t.UsuarioId == uid && t.ConcluidaEm == null).OrderByDescending(t => t.Id).First().Id;
                await Capture("questao-escuro", "/Jornada/Jogar/" + active, 390, "dark");
                await Capture("campanhas-escuro", "/Campanhas", 1440, "dark");
                int campaign; using (var db = Db()) campaign = db.Campanhas.First(c => c.Ativa && c.StatusAprovacao == "Aprovado").Id;
                await Capture("mural-escuro", "/Campanhas/Detalhes/" + campaign, 390, "dark");
                await Capture("voz-estudantil-escuro", "/Educacao", 1440, "dark");
                await Capture("publicar-escuro", "/Educacao/Publicar", 390, "dark");
                await Capture("perfil-escuro", "/Perfil", 1440, "dark");
                await Capture("ajuda-escuro", "/Faq", 390, "dark");

                var third = await Attempt(3, true); await RewardState(3, true);
                var mapLink = await student.Client.GetAsync("/Mapa");
                Check(mapLink.StatusCode == HttpStatusCode.Redirect && mapLink.Headers.Location!.ToString().Contains("Avistamentos"), "Três desafios fazem o atalho abrir o mapa real");
                Check((await student.Client.GetAsync("/Avistamentos")).StatusCode == HttpStatusCode.OK, "Proteção do mapa continua liberando estudante após a jornada");
                var fresh = new Session(); await fresh.Login(email, password);
                Check((await fresh.Client.GetStringAsync("/Jornada/Progresso")).Contains("data-guardian-accessory"), "Avatar e escudo preservados após novo login");
                await Capture("resultado-mapa-escuro", "/Jornada/Resultado/" + third, 1440, "dark");
                await Capture("mapa-liberado-escuro", "/Avistamentos", 1440, "dark");
                await Capture("mapa-liberado-celular", "/Avistamentos", 390, "dark");

                await Cookies(admin);
                await Capture("admin-escuro", "/Admin", 1440, "dark");
                await Capture("admin-celular-escuro", "/Admin", 390, "dark");
                await Capture("admin-especies-escuro", "/AdminEspecies", 390, "dark");
                await Capture("admin-formulario-escuro", "/AdminEspecies/Edit/" + species, 1440, "dark");
                await Capture("admin-quiz-escuro", "/AdminEspecieQuiz/Gerenciar/" + species, 1440, "dark");
                await Capture("admin-desafios-escuro", "/AdminJornada/Edit", 390, "dark");
                await Capture("admin-campanha-escuro", "/AdminCampanhas/Create", 1440, "dark");
                await Capture("admin-usuarios-escuro", "/Admin/Contas", 1440, "dark");

                var blockedStorage = await cdp.Call("Page.addScriptToEvaluateOnNewDocument", new { source = "Object.defineProperty(window,'localStorage',{get(){throw new DOMException('Bloqueado','SecurityError');}});" });
                await Navigate("/Login");
                Check((await Eval("document.documentElement.dataset.theme==='dark'")).GetBoolean(), "Armazenamento bloqueado usa preferência do sistema");
                await Theme("light");
                Check((await Eval("window.__interfaceErrors.length===0")).GetBoolean(), "Tema funciona sem erros quando armazenamento está indisponível");
                await cdp.Call("Page.removeScriptToEvaluateOnNewDocument", new { identifier = blockedStorage.GetProperty("identifier").GetString() });
                await cdp.Call("Emulation.setEmulatedMedia", new { features = new[] { new { name = "prefers-reduced-motion", value = "reduce" } } });
                await Navigate("/");
                Check((await Eval("getComputedStyle(document.documentElement).scrollBehavior==='auto'")).GetBoolean(), "Preferência por movimento reduzido preservada");
            }
            finally { if (!chrome.HasExited) chrome.Kill(true); }
            File.WriteAllText(Path.Combine(output, "resultado.json"), JsonSerializer.Serialize(new { total = checks.Count, checks }, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine("REVISÃO DE INTERFACE CONCLUÍDA: " + checks.Count);
        }
        catch (Exception ex)
        {
            File.WriteAllText(Path.Combine(output, "falha.txt"), ex.ToString());
            File.WriteAllText(Path.Combine(output, "resultado.json"), JsonSerializer.Serialize(new { total = checks.Count, checks }, new JsonSerializerOptions { WriteIndented = true }));
            Console.Error.WriteLine(ex); Environment.ExitCode = 1;
        }
        finally
        {
            if (!server.HasExited) server.Kill(true);
            await server.WaitForExitAsync();
            File.WriteAllText(Path.Combine(output, "servidor.log"), await stdout + await stderr);
        }
    }
}

