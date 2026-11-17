using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;

namespace TCC_Grupo12_III.Controllers
{
    public class FaqItem
    {
        public string Categoria { get; set; } = string.Empty;
        public string Pergunta { get; set; } = string.Empty;
        public string RespostaHtml { get; set; } = string.Empty;
        public string Icone { get; set; } = "bi-question-circle";
    }

    public class FaqIndexViewModel
    {
        public string TermoBusca { get; set; } = string.Empty;
        public string CategoriaAtiva { get; set; } = "todas";
        public List<FaqItem> ItensFaq { get; set; } = new();
        public List<FaqTicket> MeusTickets { get; set; } = new();
        public List<FaqTicket> TicketsAguardandoModeracao { get; set; } = new();
        public bool UsuarioAutenticado { get; set; }
        public bool IsEquipeModOuAdmin { get; set; }
    }

    public class FaqController : Controller
    {
        private readonly AppDbContext _context;

        public FaqController(AppDbContext context)
        {
            _context = context;
        }

        private int UsuarioAtualId()
        {
            var valor = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(valor, out var id) ? id : 0;
        }

        public static List<FaqItem> ObterBaseConhecimento()
        {
            return new List<FaqItem>
            {
                // Diretrizes e Fórum
                new()
                {
                    Categoria = "Diretrizes",
                    Pergunta = "Quais são as diretrizes de convivência e respeito no EcoCanta?",
                    Icone = "bi-shield-check",
                    RespostaHtml = @"<p>O EcoCanta é uma comunidade voltada à educação ambiental, conservação e ciência cidadã. Nossas diretrizes fundamentais são:</p>
                    <ul>
                        <li><strong>Respeito Mútuo:</strong> Diálogo construtivo, livre de ofensas, assédio, discriminação ou desrespeito.</li>
                        <li><strong>Veracidade Científica:</strong> Compartilhe apenas observações autênticas da fauna e flora. É proibida a disseminação de fake news ou desinformação sobre animais silvestres.</li>
                        <li><strong>Proteção à Fauna:</strong> Jamais divulgue coordenadas exatas de animais ameaçados ou ninhos que possam atrair caçadores ou comércio ilegal.</li>
                        <li><strong>Linguagem Adequada:</strong> Publicações com palavrões, spam ou publicidade não autorizada serão removidas pela moderação.</li>
                    </ul>"
                },
                new()
                {
                    Categoria = "Diretrizes",
                    Pergunta = "Como funciona a suspensão temporária e o banimento de usuários?",
                    Icone = "bi-person-x",
                    RespostaHtml = @"<p>Para assegurar a integridade da plataforma, aplicamos sanções proporcionais em casos de descumprimento das diretrizes:</p>
                    <ul>
                        <li><strong>Suspensão do Fórum (Moderadores):</strong> Moderadores podem suspender usuários de postar novos tópicos e comentários por <strong>até 1 mês (30 dias)</strong>. Durante a suspensão, o usuário mantém apenas permissão de leitura.</li>
                        <li><strong>Suspensão por Administradores:</strong> Administradores podem estipular suspensões por prazos personalizados ou superiores a 30 dias.</li>
                        <li><strong>Banimento Permanente:</strong> Exclusivo de Administradores. Contas banidas têm o acesso bloqueado imediatamente em toda a plataforma.</li>
                    </ul>"
                },
                new()
                {
                    Categoria = "Diretrizes",
                    Pergunta = "Como solicitar a credencial oficial de Jornalista / Imprensa?",
                    Icone = "bi-newspaper",
                    RespostaHtml = @"<p>Para obter autorização para publicar notícias oficiais no portal e propor campanhas ambientais, siga estes passos:</p>
                    <ol>
                        <li>Acesse as <strong>Configurações de Perfil</strong> no menu superior.</li>
                        <li>Clique no botão <strong>Solicitar Credencial de Jornalista</strong>.</li>
                        <li>No formulário modal, informe seu Registro Profissional (MTB / Imprensa), o veículo de comunicação ou coletivo ao qual pertence, link para portfólio e sua linha editorial de cobertura na Cantareira.</li>
                        <li>Nossa equipe de moderação avaliará suas credenciais e você receberá o selo de Jornalista Oficial.</li>
                    </ol>"
                },

                // CantaDex & Cartinhas
                new()
                {
                    Categoria = "CantaDex",
                    Pergunta = "Quais são os motivos e vantagens de conquistar cartinhas na CantaDex?",
                    Icone = "bi-journal-bookmark",
                    RespostaHtml = @"<p>A <strong>CantaDex</strong> é o catálogo oficial da fauna e flora da Serra da Cantareira. Conquistar as cartinhas na sua coleção traz benefícios fundamentais na plataforma:</p>
                    <ul>
                        <li><strong>1. Liberar Registro de Avistamentos no Mapa:</strong> Para garantir a confiabilidade dos dados da ciência cidadã, você só pode registrar avistamentos no mapa das espécies cuja cartinha já conquistou na CantaDex.</li>
                        <li><strong>2. Conexão com Estudos e Pesquisas:</strong> Na ficha de cada espécie, você tem acesso rápido aos estudos científicos, pesquisas de campo e relatórios de biólogos sobre a preservação daquele animal.</li>
                        <li><strong>3. Ganho Expressivo de XP (+40 XP):</strong> Conquistar cada cartinha concede pontos de experiência para subir de nível e desbloquear novas ferramentas (Fórum, Campanhas e Criação de Pesquisas).</li>
                        <li><strong>4. Selo Oficial de Conhecimento:</strong> As espécies dominadas recebem o selo dourado de identificação comprovada no seu álbum.</li>
                    </ul>"
                },
                new()
                {
                    Categoria = "CantaDex",
                    Pergunta = "Como conquistar uma cartinha na CantaDex? O Jogo Diário ou abrir a ficha do animal libera a cartinha?",
                    Icone = "bi-patch-question",
                    RespostaHtml = @"<p><strong>Não.</strong> A cartinha de uma espécie <strong>só pode ser conquistada realizando o Quiz de Conhecimento</strong> daquele animal com aproveitamento de pelo menos <strong>70% de acertos</strong>.</p>
                    <p>Apenas visitar a página do animal ou salvá-lo no Jogo Diário não concede a cartinha (o Jogo Diário recarrega a sua reserva de avistamentos). Para conquistar a cartinha oficial:</p>
                    <ol>
                        <li>Acesse a aba <strong>Animais</strong> no menu superior e clique na espécie desejada.</li>
                        <li>Na ficha do animal, abra a aba <strong>Quiz</strong>.</li>
                        <li>Responda às perguntas sobre habitat, dieta, características e conservação.</li>
                        <li>Ao atingir 70% de acertos, a cartinha é desbloqueada instantaneamente na sua CantaDex e o registro de avistamentos daquela espécie fica liberado no mapa!</li>
                    </ol>"
                },

                // Mapa de Avistamentos
                new()
                {
                    Categoria = "Avistamentos",
                    Pergunta = "Como registrar um novo avistamento no mapa da Serra da Cantareira?",
                    Icone = "bi-geo-alt",
                    RespostaHtml = @"<p>O mapa interativo permite registrar observações de espécies nativas na região da Cantareira:</p>
                    <ol>
                        <li>Acesse o menu <strong>Avistamentos</strong> (desbloqueado no Nível 2).</li>
                        <li>No painel lateral, selecione o animal avistado, data, quantidade e envie uma fotografia de comprovação.</li>
                        <li>Clique diretamente no mapa para marcar a coordenada exata onde o animal foi observado. As coordenadas devem estar situadas dentro do polígono da Serra da Cantareira.</li>
                        <li>Clique em <strong>Registrar Avistamento</strong> para salvar e compartilhar com a comunidade!</li>
                    </ol>"
                },
                new()
                {
                    Categoria = "Avistamentos",
                    Pergunta = "Por que preciso de 'Reserva de Avistamento' e como recarregá-la?",
                    Icone = "bi-controller",
                    RespostaHtml = @"<p>Para incentivar o estudo e garantir a precisão das espécies catalogadas, cada avistamento consome 1 unidade da sua reserva para aquele animal específico.</p>
                    <p>Para recarregar sua reserva gratuitamente, jogue o <strong>Jogo Diário</strong> da plataforma. Ao completar as fases e resgatar espécies no quiz ecológico, você recebe +1 avistamento disponível para registrar no mapa!</p>"
                },

                // Campanhas Ambientais
                new()
                {
                    Categoria = "Campanhas",
                    Pergunta = "Como funcionam as 'Próximas Campanhas' e a Pré-Inscrição antecipada?",
                    Icone = "bi-calendar-check",
                    RespostaHtml = @"<p>As campanhas ambientais do EcoCanta são divididas em duas categorias principais:</p>
                    <ul>
                        <li><strong>Campanhas em Andamento:</strong> Ações contínuas ou que já estão ocorrendo no momento. Você pode clicar em <em>Participar</em> imediatamente.</li>
                        <li><strong>Próximas Campanhas:</strong> Ações com data agendada para o futuro. Nessas campanhas, os voluntários podem clicar em <strong>Fazer Pré-inscrição</strong> para reservar sua vaga com antecedência e receber alertas sobre ponto de encontro e orientações de segurança.</li>
                    </ul>"
                },
                new()
                {
                    Categoria = "Campanhas",
                    Pergunta = "Quem pode propor uma nova campanha ambiental e como ela é aprovada?",
                    Icone = "bi-megaphone",
                    RespostaHtml = @"<p>Para garantir ações estruturadas e seguras, a proposição de novas campanhas é permitida para:</p>
                    <ul>
                        <li><strong>Jornalistas Oficiais Credenciados;</strong></li>
                        <li><strong>Pesquisadores com pelo menos 15 estudos científicos aprovados</strong> na plataforma;</li>
                        <li><strong>Administradores e Moderadores.</strong></li>
                    </ul>
                    <p>Ao submeter uma proposta pelo botão <em>Propor Nova Campanha</em>, a ação é encaminhada para a fila de moderação, onde Administradores e Moderadores homologam o cronograma, ponto de encontro e metas ecológicas antes de abrir para o público.</p>"
                },

                // Denúncias Ambientais
                new()
                {
                    Categoria = "Denúncias",
                    Pergunta = "Como denunciar um crime ambiental ou descarte irregular de lixo?",
                    Icone = "bi-exclamation-triangle",
                    RespostaHtml = @"<p>Se você presenciou desmatamento, queimadas, caça ilegal, ocupação irregular ou poluição nas trilhas da Cantareira:</p>
                    <ol>
                        <li>Acesse o menu <strong>Denúncia</strong> no cabeçalho.</li>
                        <li>Selecione o tipo de ocorrência e descreva o local com o máximo de detalhes possível.</li>
                        <li>Anexe uma fotografia da infração (se seguro para você registrar).</li>
                        <li>Envie a denúncia. A equipe de moderação e fiscalização receberá o alerta no painel administrativo para encaminhamento aos órgãos competentes.</li>
                    </ol>"
                },
                new()
                {
                    Categoria = "Denúncias",
                    Pergunta = "Minha identidade fica segura ao fazer uma denúncia?",
                    Icone = "bi-incognito",
                    RespostaHtml = @"<p><strong>Sim, 100% segura.</strong> Você pode marcar a opção <em>Denúncia Anônima</em> para não enviar nenhum dado de contato. Caso prefira informar nome e e-mail para receber retorno, seus dados ficam estritamente confidenciais e restritos à equipe gestora.</p>"
                },

                // Educação e Jogos
                new()
                {
                    Categoria = "Educação",
                    Pergunta = "Como subir de nível e ganhar pontos de experiência (XP)?",
                    Icone = "bi-trophy",
                    RespostaHtml = @"<p>Você acumula XP ao participar ativamente das ações de preservação:</p>
                    <ul>
                        <li><strong>Jogar o Jogo Diário:</strong> +10 a +30 XP por fase concluída com sucesso.</li>
                        <li><strong>Registrar um Avistamento no Mapa:</strong> +15 XP por animal registrado.</li>
                        <li><strong>Participar de uma Campanha Ambiental:</strong> +20 XP.</li>
                        <li><strong>Submeter Denúncia Verificada:</strong> +15 XP.</li>
                        <li><strong>Ter um Estudo Científico Aprovado:</strong> +25 XP.</li>
                    </ul>
                    <p>Ao subir de nível, você desbloqueia novos recursos como o mapa de avistamentos e insígnias especiais de guardião da floresta.</p>"
                }
            };
        }

        public async Task<IActionResult> Index(string? busca, string? categoria)
        {
            var vm = new FaqIndexViewModel
            {
                TermoBusca = busca ?? string.Empty,
                CategoriaAtiva = string.IsNullOrWhiteSpace(categoria) ? "todas" : categoria.ToLowerInvariant(),
                UsuarioAutenticado = User.Identity != null && User.Identity.IsAuthenticated
            };

            var todosItens = ObterBaseConhecimento();

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim().ToLowerInvariant();
                todosItens = todosItens.Where(i =>
                    i.Pergunta.ToLowerInvariant().Contains(termo) ||
                    i.RespostaHtml.ToLowerInvariant().Contains(termo) ||
                    i.Categoria.ToLowerInvariant().Contains(termo)
                ).ToList();
            }

            if (vm.CategoriaAtiva != "todas")
            {
                todosItens = todosItens.Where(i => i.Categoria.ToLowerInvariant() == vm.CategoriaAtiva).ToList();
            }

            vm.ItensFaq = todosItens;

            if (vm.UsuarioAutenticado)
            {
                var uid = UsuarioAtualId();
                vm.MeusTickets = await _context.FaqTickets
                    .Include(t => t.Mensagens)
                    .Where(t => t.UsuarioId == uid)
                    .OrderByDescending(t => t.AtualizadoEm)
                    .ToListAsync();

                var ehEquipe = User.IsInRole("Admin");
                vm.IsEquipeModOuAdmin = ehEquipe;
                if (ehEquipe)
                {
                    vm.TicketsAguardandoModeracao = await _context.FaqTickets
                        .Include(t => t.Usuario)
                        .Include(t => t.Mensagens)
                        .OrderByDescending(t => t.Status == "Aberto")
                        .ThenByDescending(t => t.AtualizadoEm)
                        .Take(10)
                        .ToListAsync();
                }
            }

            return View(vm);
        }

        [Authorize]
        public async Task<IActionResult> Conversa(int id)
        {
            var uid = UsuarioAtualId();
            var ticket = await _context.FaqTickets
                .Include(t => t.Usuario)
                .Include(t => t.Mensagens)
                    .ThenInclude(m => m.Usuario)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            var ehEquipe = User.IsInRole("Admin");
            if (ticket.UsuarioId != uid && !ehEquipe)
            {
                TempData["Erro"] = "Você não tem permissão para visualizar este atendimento.";
                return RedirectToAction(nameof(Index));
            }

            return View(ticket);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NovaPergunta(string assunto, string categoria, string mensagem)
        {
            var uid = UsuarioAtualId();

            if (string.IsNullOrWhiteSpace(assunto) || assunto.Length > 150 || string.IsNullOrWhiteSpace(mensagem) || mensagem.Length > 2000)
            {
                TempData["Erro"] = "Preencha o assunto e a sua dúvida detalhada.";
                return RedirectToAction(nameof(Index));
            }

            var ticket = new FaqTicket
            {
                UsuarioId = uid,
                Assunto = assunto.Trim(),
                Categoria = string.IsNullOrWhiteSpace(categoria) ? "Geral" : categoria.Trim(),
                Status = "Aberto",
                CriadoEm = DateTime.Now,
                AtualizadoEm = DateTime.Now
            };

            _context.FaqTickets.Add(ticket);
            await _context.SaveChangesAsync();

            var msgInicial = new FaqMensagem
            {
                FaqTicketId = ticket.Id,
                UsuarioId = uid,
                Conteudo = mensagem.Trim(),
                CriadoEm = DateTime.Now,
                IsEquipeModOuAdmin = false,
                CargoAutor = "Estudante"
            };

            _context.FaqMensagens.Add(msgInicial);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Sua pergunta foi enviada para os administração! Acompanhe as respostas por este chat.";
            return RedirectToAction(nameof(Conversa), new { id = ticket.Id });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarMensagem(int ticketId, string conteudo)
        {
            var uid = UsuarioAtualId();
            var ticket = await _context.FaqTickets.FindAsync(ticketId);

            if (ticket == null)
            {
                return NotFound();
            }

            var ehEquipe = User.IsInRole("Admin");
            if (ticket.UsuarioId != uid && !ehEquipe)
            {
                TempData["Erro"] = "Você não tem permissão para interagir neste chat.";
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrWhiteSpace(conteudo) || conteudo.Length > 2000)
            {
                TempData["Erro"] = "A mensagem não pode ser vazia.";
                return RedirectToAction(nameof(Conversa), new { id = ticketId });
            }

            var novaMsg = new FaqMensagem
            {
                FaqTicketId = ticketId,
                UsuarioId = uid,
                Conteudo = conteudo.Trim(),
                CriadoEm = DateTime.Now,
                IsEquipeModOuAdmin = ehEquipe,
                CargoAutor = ehEquipe ? (User.IsInRole("Admin") ? "Administrador" : "Moderador") : "Guardião"
            };

            _context.FaqMensagens.Add(novaMsg);

            ticket.AtualizadoEm = DateTime.Now;
            if (ehEquipe)
            {
                ticket.Status = "Respondido";
                ticket.UltimaRespostaPor = User.Identity?.Name ?? (User.IsInRole("Admin") ? "Administrador" : "Moderador");
            }
            else
            {
                ticket.Status = "Aberto"; // Reabre para a equipe responder
            }

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Mensagem enviada com sucesso!";
            return RedirectToAction(nameof(Conversa), new { id = ticketId });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Fechar(int id)
        {
            var uid = UsuarioAtualId();
            var ticket = await _context.FaqTickets.FindAsync(id);

            if (ticket == null)
            {
                return NotFound();
            }

            var ehEquipe = User.IsInRole("Admin");
            if (ticket.UsuarioId != uid && !ehEquipe)
            {
                TempData["Erro"] = "Permissão negada.";
                return RedirectToAction(nameof(Index));
            }

            ticket.Status = "Fechado";
            ticket.AtualizadoEm = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Dúvida marcada como resolvida / encerrada com sucesso!";
            return RedirectToAction(nameof(Conversa), new { id });
        }
    }
}
