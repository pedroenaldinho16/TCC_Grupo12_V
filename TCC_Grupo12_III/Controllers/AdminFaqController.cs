using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TCC_Grupo12_III.Data;
using TCC_Grupo12_III.Models;

namespace TCC_Grupo12_III.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminFaqController : Controller
    {
        private readonly AppDbContext _context;

        public AdminFaqController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? status, string? categoria, string? busca)
        {
            var query = _context.FaqTickets
                .Include(t => t.Usuario)
                .Include(t => t.Mensagens)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && status != "todos")
            {
                query = query.Where(t => t.Status.ToLower() == status.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(categoria) && categoria != "todas")
            {
                query = query.Where(t => t.Categoria.ToLower() == categoria.ToLower());
            }

            if (!string.IsNullOrWhiteSpace(busca))
            {
                var termo = busca.Trim().ToLower();
                query = query.Where(t =>
                    t.Assunto.ToLower().Contains(termo) ||
                    (t.Usuario != null && t.Usuario.Nome.ToLower().Contains(termo)) ||
                    (t.Usuario != null && t.Usuario.Email.ToLower().Contains(termo)) ||
                    t.Mensagens.Any(m => m.Conteudo.ToLower().Contains(termo)));
            }

            var tickets = await query
                .OrderByDescending(t => t.Status == "Aberto")
                .ThenByDescending(t => t.AtualizadoEm)
                .ToListAsync();

            ViewBag.StatusFiltro = status ?? "todos";
            ViewBag.CategoriaFiltro = categoria ?? "todas";
            ViewBag.TermoBusca = busca ?? string.Empty;

            ViewBag.TotalAbertos = await _context.FaqTickets.CountAsync(t => t.Status == "Aberto");
            ViewBag.TotalRespondidos = await _context.FaqTickets.CountAsync(t => t.Status == "Respondido");
            ViewBag.TotalFechados = await _context.FaqTickets.CountAsync(t => t.Status == "Fechado");
            ViewBag.TotalGeral = await _context.FaqTickets.CountAsync();

            return View(tickets);
        }

        public async Task<IActionResult> Atender(int id)
        {
            var ticket = await _context.FaqTickets
                .Include(t => t.Usuario)
                .Include(t => t.Mensagens)
                    .ThenInclude(m => m.Usuario)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Responder(int ticketId, string conteudo, string? novoStatus)
        {
            var ticket = await _context.FaqTickets.FindAsync(ticketId);
            if (ticket == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(conteudo))
            {
                TempData["Erro"] = "A resposta não pode ser vazia.";
                return RedirectToAction(nameof(Atender), new { id = ticketId });
            }

            var ehAdmin = User.IsInRole("Admin");
            var cargo = ehAdmin ? "Administrador" : "Moderador";

            var msg = new FaqMensagem
            {
                FaqTicketId = ticketId,
                UsuarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0"),
                Conteudo = conteudo.Trim(),
                CriadoEm = DateTime.Now,
                IsEquipeModOuAdmin = true,
                CargoAutor = cargo
            };

            _context.FaqMensagens.Add(msg);

            ticket.AtualizadoEm = DateTime.Now;
            ticket.UltimaRespostaPor = $"{User.Identity?.Name ?? cargo} ({cargo})";
            ticket.Status = string.IsNullOrWhiteSpace(novoStatus) ? "Respondido" : novoStatus;

            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Resposta oficial enviada com sucesso ao usuário!";
            return RedirectToAction(nameof(Atender), new { id = ticketId });
        }

        [HttpGet]
        public Task<IActionResult> Chat(int id) => Atender(id);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarStatus(int id, string status)
        {
            var ticket = await _context.FaqTickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            ticket.Status = status;
            ticket.AtualizadoEm = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"Status do atendimento alterado para '{status}'.";
            return RedirectToAction(nameof(Atender), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Encerrar(int id)
        {
            var ticket = await _context.FaqTickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            ticket.Status = "Fechado";
            ticket.AtualizadoEm = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Atendimento finalizado e encerrado com sucesso!";
            return RedirectToAction(nameof(Atender), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reabrir(int id)
        {
            var ticket = await _context.FaqTickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            ticket.Status = "Aberto";
            ticket.AtualizadoEm = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Atendimento reaberto com sucesso.";
            return RedirectToAction(nameof(Atender), new { id });
        }
    }
}
