using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;

namespace TCC_Grupo12_III.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDenunciasController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AdminDenunciasController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(
            string? aba,
            string? tipo,
            string? status,
            string? busca,
            string? statusJornalista,
            string? buscaJornalista,
            string? statusEstudo,
            string? buscaEstudo,
            string? statusCampanha,
            string? buscaCampanha,
            string? statusComunidade,
            string? buscaComunidade)
        {
            aba = string.IsNullOrWhiteSpace(aba) ? "denuncias" : aba.ToLowerInvariant();
            if (aba != "denuncias" && aba != "jornalistas" && aba != "estudos" && aba != "campanhas" && aba != "comunidades")
            {
                aba = "denuncias";
            }

            var vm = new AdminAprovacoesViewModel
            {
                AbaAtiva = aba,
                TipoDenuncia = tipo ?? "Todos",
                StatusDenuncia = status ?? "todos",
                BuscaDenuncia = busca ?? string.Empty,
                StatusJornalistaFiltro = statusJornalista ?? "todos",
                BuscaJornalista = buscaJornalista ?? string.Empty,
                StatusEstudoFiltro = statusEstudo ?? "todos",
                BuscaEstudo = buscaEstudo ?? string.Empty,
                StatusCampanhaFiltro = statusCampanha ?? "todos",
                BuscaCampanha = buscaCampanha ?? string.Empty,
                StatusComunidadeFiltro = statusComunidade ?? "todos",
                BuscaComunidade = buscaComunidade ?? string.Empty
            };

            // Métricas globais de denúncias
            vm.TotalDenuncias = await _context.Denuncias.CountAsync();
            vm.TotalDenunciasPendentes = await _context.Denuncias.CountAsync(d => !d.Resolvida);
            vm.TotalDenunciasResolvidas = vm.TotalDenuncias - vm.TotalDenunciasPendentes;
            vm.TiposDisponiveis = await _context.Denuncias
                .Select(d => d.Tipo)
                .Distinct()
                .ToListAsync();

            // Métricas globais de solicitações de jornalistas
            var queryJornalistasBase = _context.Usuarios
                .Where(u => !string.IsNullOrEmpty(u.StatusJornalista) || u.IsJornalista);

            vm.TotalJornalistas = await queryJornalistasBase.CountAsync();
            vm.TotalJornalistasPendentes = await queryJornalistasBase.CountAsync(u => u.StatusJornalista == "Pendente");
            vm.TotalJornalistasAprovados = await queryJornalistasBase.CountAsync(u => u.StatusJornalista == "Aprovado" || u.IsJornalista);
            vm.TotalJornalistasReprovados = await queryJornalistasBase.CountAsync(u => u.StatusJornalista == "Reprovado");

            // Carregamento de denúncias filtradas
            var queryDenuncias = _context.Denuncias.AsQueryable();

            if (!string.IsNullOrWhiteSpace(tipo) && tipo != "Todos")
            {
                queryDenuncias = queryDenuncias.Where(d => d.Tipo == tipo);
            }

            if (status == "pendente")
            {
                queryDenuncias = queryDenuncias.Where(d => !d.Resolvida);
            }
            else if (status == "resolvida")
            {
                queryDenuncias = queryDenuncias.Where(d => d.Resolvida);
            }

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim();
                queryDenuncias = queryDenuncias.Where(d => d.Localizacao.Contains(termo)
                    || d.Descricao.Contains(termo)
                    || (d.NomeContato != null && d.NomeContato.Contains(termo))
                    || (d.EmailContato != null && d.EmailContato.Contains(termo)));
            }

            vm.Denuncias = await queryDenuncias
                .OrderByDescending(d => d.CriadoEm)
                .ToListAsync();

            // Carregamento de jornalistas filtrados
            var queryJornalistas = queryJornalistasBase.AsQueryable();

            if (statusJornalista == "pendente")
            {
                queryJornalistas = queryJornalistas.Where(u => u.StatusJornalista == "Pendente");
            }
            else if (statusJornalista == "aprovado")
            {
                queryJornalistas = queryJornalistas.Where(u => u.StatusJornalista == "Aprovado" || u.IsJornalista);
            }
            else if (statusJornalista == "reprovado")
            {
                queryJornalistas = queryJornalistas.Where(u => u.StatusJornalista == "Reprovado");
            }

            if (!string.IsNullOrWhiteSpace(buscaJornalista))
            {
                var termoJornalista = buscaJornalista.Trim();
                queryJornalistas = queryJornalistas.Where(u =>
                    u.Nome.Contains(termoJornalista) ||
                    u.Email.Contains(termoJornalista) ||
                    (u.RegistroJornalista != null && u.RegistroJornalista.Contains(termoJornalista)) ||
                    (u.VeiculoJornalista != null && u.VeiculoJornalista.Contains(termoJornalista)) ||
                    (u.JustificativaJornalista != null && u.JustificativaJornalista.Contains(termoJornalista)));
            }

            vm.Jornalistas = await queryJornalistas
                .OrderByDescending(u => u.StatusJornalista == "Pendente")
                .ThenByDescending(u => u.DataSolicitacaoJornalista ?? DateTime.MinValue)
                .ThenBy(u => u.Nome)
                .ToListAsync();

            // Métricas globais de estudos científicos
            var queryEstudosBase = _context.EstudosCientificos
                .Include(e => e.Usuario)
                .Include(e => e.Especie)
                .AsQueryable();

            vm.TotalEstudos = await queryEstudosBase.CountAsync();
            vm.TotalEstudosPendentes = await queryEstudosBase.CountAsync(e => e.StatusAprovacao == "Pendente");
            vm.TotalEstudosAprovados = await queryEstudosBase.CountAsync(e => e.StatusAprovacao == "Aprovado");
            vm.TotalEstudosReprovados = await queryEstudosBase.CountAsync(e => e.StatusAprovacao == "Reprovado");

            // Carregamento de estudos filtrados
            var queryEstudos = queryEstudosBase.AsQueryable();

            if (statusEstudo == "pendente")
            {
                queryEstudos = queryEstudos.Where(e => e.StatusAprovacao == "Pendente");
            }
            else if (statusEstudo == "aprovado")
            {
                queryEstudos = queryEstudos.Where(e => e.StatusAprovacao == "Aprovado");
            }
            else if (statusEstudo == "reprovado")
            {
                queryEstudos = queryEstudos.Where(e => e.StatusAprovacao == "Reprovado");
            }

            if (!string.IsNullOrWhiteSpace(buscaEstudo))
            {
                var termoEstudo = buscaEstudo.Trim();
                queryEstudos = queryEstudos.Where(e =>
                    e.Titulo.Contains(termoEstudo) ||
                    e.Subtitulo.Contains(termoEstudo) ||
                    e.Autor.Contains(termoEstudo) ||
                    (e.Instituicao != null && e.Instituicao.Contains(termoEstudo)) ||
                    (e.EspecieNome != null && e.EspecieNome.Contains(termoEstudo)));
            }

            vm.Estudos = await queryEstudos
                .OrderByDescending(e => e.StatusAprovacao == "Pendente")
                .ThenByDescending(e => e.DataSubmissao)
                .ToListAsync();

            // Métricas globais de propostas de campanhas
            var queryCampanhasBase = _context.Campanhas
                .Include(c => c.Usuario)
                .AsQueryable();

            vm.TotalCampanhas = await queryCampanhasBase.CountAsync();
            vm.TotalCampanhasPendentes = await queryCampanhasBase.CountAsync(c => c.StatusAprovacao == "Pendente");
            vm.TotalCampanhasAprovadas = await queryCampanhasBase.CountAsync(c => c.StatusAprovacao == "Aprovado" || (string.IsNullOrEmpty(c.StatusAprovacao) && c.Ativa));
            vm.TotalCampanhasReprovadas = await queryCampanhasBase.CountAsync(c => c.StatusAprovacao == "Reprovado");

            // Carregamento de campanhas filtradas
            var queryCampanhas = queryCampanhasBase.AsQueryable();

            if (statusCampanha == "pendente")
            {
                queryCampanhas = queryCampanhas.Where(c => c.StatusAprovacao == "Pendente");
            }
            else if (statusCampanha == "aprovado")
            {
                queryCampanhas = queryCampanhas.Where(c => c.StatusAprovacao == "Aprovado" || (string.IsNullOrEmpty(c.StatusAprovacao) && c.Ativa));
            }
            else if (statusCampanha == "reprovado")
            {
                queryCampanhas = queryCampanhas.Where(c => c.StatusAprovacao == "Reprovado");
            }

            if (!string.IsNullOrWhiteSpace(buscaCampanha))
            {
                var termoCampanha = buscaCampanha.Trim().ToLower();
                queryCampanhas = queryCampanhas.Where(c =>
                    c.Titulo.ToLower().Contains(termoCampanha) ||
                    (c.Descricao != null && c.Descricao.ToLower().Contains(termoCampanha)) ||
                    (c.Usuario != null && c.Usuario.Nome.ToLower().Contains(termoCampanha)));
            }

            vm.Campanhas = await queryCampanhas
                .OrderByDescending(c => c.StatusAprovacao == "Pendente")
                .ThenByDescending(c => c.DataCriacao)
                .ToListAsync();

            // Métricas e carregamento de comunidades do fórum
            var queryComunidadesBase = _context.ForumComunidades
                .Include(c => c.Criador)
                .Include(c => c.Topicos)
                .Include(c => c.Campanhas)
                .AsQueryable();

            vm.TotalComunidades = await queryComunidadesBase.CountAsync();
            vm.TotalComunidadesPendentes = await queryComunidadesBase.CountAsync(c => c.StatusAprovacao == "Pendente");
            vm.TotalComunidadesAprovadas = await queryComunidadesBase.CountAsync(c => c.StatusAprovacao == "Aprovada");
            vm.TotalComunidadesReprovadas = await queryComunidadesBase.CountAsync(c => c.StatusAprovacao == "Rejeitada");

            var queryComunidades = queryComunidadesBase.AsQueryable();

            if (statusComunidade == "pendente")
            {
                queryComunidades = queryComunidades.Where(c => c.StatusAprovacao == "Pendente");
            }
            else if (statusComunidade == "aprovada")
            {
                queryComunidades = queryComunidades.Where(c => c.StatusAprovacao == "Aprovada");
            }
            else if (statusComunidade == "rejeitada")
            {
                queryComunidades = queryComunidades.Where(c => c.StatusAprovacao == "Rejeitada");
            }

            if (!string.IsNullOrWhiteSpace(buscaComunidade))
            {
                var termoCom = buscaComunidade.Trim().ToLower();
                queryComunidades = queryComunidades.Where(c =>
                    c.Nome.ToLower().Contains(termoCom) ||
                    c.Descricao.ToLower().Contains(termoCom) ||
                    (c.Criador != null && c.Criador.Nome.ToLower().Contains(termoCom)));
            }

            vm.Comunidades = await queryComunidades
                .OrderByDescending(c => c.StatusAprovacao == "Pendente")
                .ThenByDescending(c => c.CriadoEm)
                .ToListAsync();

            return View(vm);
        }

        public async Task<IActionResult> Detalhes(int id)
        {
            var denuncia = await _context.Denuncias.FindAsync(id);
            if (denuncia == null)
            {
                return NotFound();
            }

            return View(denuncia);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlternarStatus(int id, string? observacaoAdmin, string? returnUrl)
        {
            var denuncia = await _context.Denuncias.FindAsync(id);
            if (denuncia == null)
            {
                return NotFound();
            }

            denuncia.Resolvida = !denuncia.Resolvida;
            if (observacaoAdmin != null)
            {
                denuncia.ObservacaoAdmin = observacaoAdmin;
            }

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = denuncia.Resolvida
                ? "Denúncia marcada como resolvida!"
                : "Denúncia reaberta como pendente.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index), new { aba = "denuncias" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarObservacao(int id, string? observacaoAdmin)
        {
            var denuncia = await _context.Denuncias.FindAsync(id);
            if (denuncia == null)
            {
                return NotFound();
            }

            denuncia.ObservacaoAdmin = observacaoAdmin;
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Observação interna salva com sucesso!";
            return RedirectToAction(nameof(Detalhes), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var denuncia = await _context.Denuncias.FindAsync(id);
            if (denuncia == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(denuncia.FotoPath))
            {
                var caminhoFoto = Path.Combine(_env.WebRootPath, denuncia.FotoPath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(caminhoFoto))
                {
                    System.IO.File.Delete(caminhoFoto);
                }
            }

            _context.Denuncias.Remove(denuncia);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Denúncia removida com sucesso!";
            return RedirectToAction(nameof(Index), new { aba = "denuncias" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprovarJornalista(int id, string? observacaoAdmin, string? returnUrl)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            usuario.IsJornalista = true;
            usuario.StatusJornalista = "Aprovado";
            usuario.DataRespostaJornalista = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(observacaoAdmin))
            {
                usuario.MotivoReprovacaoJornalista = observacaoAdmin.Trim();
            }

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"Credencial aprovada com sucesso! {usuario.Nome} agora é jornalista verificado e pode publicar notícias.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index), new { aba = "jornalistas" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReprovarJornalista(int id, string motivoReprovacao, string? returnUrl)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            usuario.IsJornalista = false;
            usuario.StatusJornalista = "Reprovado";
            usuario.DataRespostaJornalista = DateTime.Now;
            usuario.MotivoReprovacaoJornalista = string.IsNullOrWhiteSpace(motivoReprovacao)
                ? "Credenciais não validadas pela moderação."
                : motivoReprovacao.Trim();

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"Solicitação de credencial de {usuario.Nome} foi reprovada.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index), new { aba = "jornalistas" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevogarJornalista(int id, string? motivo, string? returnUrl)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            usuario.IsJornalista = false;
            usuario.StatusJornalista = "Reprovado";
            usuario.DataRespostaJornalista = DateTime.Now;
            usuario.MotivoReprovacaoJornalista = string.IsNullOrWhiteSpace(motivo)
                ? "Credencial de jornalista revogada pela moderação."
                : motivo.Trim();

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"A credencial de jornalista de {usuario.Nome} foi revogada.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index), new { aba = "jornalistas" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprovarEstudo(int id, string? observacaoAdmin, string? returnUrl)
        {
            var estudo = await _context.EstudosCientificos.Include(e => e.Usuario).FirstOrDefaultAsync(e => e.Id == id);
            if (estudo == null)
            {
                return NotFound();
            }

            estudo.StatusAprovacao = "Aprovado";
            estudo.DataResposta = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(observacaoAdmin))
            {
                estudo.ObservacoesAdmin = observacaoAdmin.Trim();
            }

            if (estudo.Usuario != null)
            {
                estudo.Usuario.Xp += 25;
            }

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"Estudo '{estudo.Titulo}' aprovado com sucesso! Já está visível na Biblioteca de Estudos (+25 XP concedido ao autor).";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index), new { aba = "estudos" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReprovarEstudo(int id, string? motivo, string? returnUrl)
        {
            var estudo = await _context.EstudosCientificos.FindAsync(id);
            if (estudo == null)
            {
                return NotFound();
            }

            estudo.StatusAprovacao = "Reprovado";
            estudo.DataResposta = DateTime.Now;
            estudo.MotivoReprovacao = string.IsNullOrWhiteSpace(motivo)
                ? "Estudo não atende aos critérios científicos da plataforma."
                : motivo.Trim();

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"O estudo '{estudo.Titulo}' foi reprovado.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index), new { aba = "estudos" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirEstudo(int id, string? returnUrl)
        {
            var estudo = await _context.EstudosCientificos.FindAsync(id);
            if (estudo == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(estudo.ImagemUrl) && estudo.ImagemUrl.StartsWith("/uploads/estudos/"))
            {
                var caminhoImagem = Path.Combine(_env.WebRootPath, estudo.ImagemUrl.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(caminhoImagem))
                {
                    System.IO.File.Delete(caminhoImagem);
                }
            }

            _context.EstudosCientificos.Remove(estudo);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Estudo científico excluído com sucesso.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index), new { aba = "estudos" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprovarCampanha(int id, string? returnUrl)
        {
            var campanha = await _context.Campanhas.FindAsync(id);
            if (campanha == null)
            {
                return NotFound();
            }

            campanha.StatusAprovacao = "Aprovado";
            campanha.Ativa = true;
            campanha.DataResposta = DateTime.Now;
            campanha.AprovadoPorNome = User.Identity?.Name ?? "Moderação";
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"A campanha \"{campanha.Titulo}\" foi homologada e aprovada com sucesso!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index), new { aba = "campanhas" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReprovarCampanha(int id, string? motivo, string? returnUrl)
        {
            var campanha = await _context.Campanhas.FindAsync(id);
            if (campanha == null)
            {
                return NotFound();
            }

            campanha.StatusAprovacao = "Reprovado";
            campanha.DataResposta = DateTime.Now;
            campanha.MotivoReprovacao = string.IsNullOrWhiteSpace(motivo)
                ? "A proposta de campanha não atende aos critérios ecológicos ou logísticos da plataforma."
                : motivo.Trim();
            campanha.AprovadoPorNome = User.Identity?.Name ?? "Moderação";
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"A proposta da campanha \"{campanha.Titulo}\" foi reprovada.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index), new { aba = "campanhas" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprovarComunidade(int id, string? returnUrl)
        {
            var comunidade = await _context.ForumComunidades.FindAsync(id);
            if (comunidade == null)
            {
                return NotFound();
            }

            comunidade.StatusAprovacao = "Aprovada";
            comunidade.AprovadoEm = DateTime.Now;
            var uidStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(uidStr, out var uid))
            {
                comunidade.AprovadoPorId = uid;
            }

            // Conceder XP de liderança ao criador
            if (comunidade.CriadorId.HasValue)
            {
                var criador = await _context.Usuarios.FindAsync(comunidade.CriadorId.Value);
                if (criador != null)
                {
                    criador.Xp += 30;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"A comunidade \"{comunidade.Nome}\" foi aprovada e homologada com sucesso!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index), new { aba = "comunidades" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejeitarComunidade(int id, string? motivo, string? returnUrl)
        {
            var comunidade = await _context.ForumComunidades.FindAsync(id);
            if (comunidade == null)
            {
                return NotFound();
            }

            comunidade.StatusAprovacao = "Rejeitada";
            comunidade.MotivoRejeicao = string.IsNullOrWhiteSpace(motivo)
                ? "A proposta de comunidade não atende às diretrizes temáticas da Cantareira."
                : motivo.Trim();

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"A proposta da comunidade \"{comunidade.Nome}\" foi rejeitada.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index), new { aba = "comunidades" });
        }
    }
}
