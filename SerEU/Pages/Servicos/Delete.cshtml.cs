using SerEU.Data;
using SerEU.Hubs;
using SerEU.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace SerEU.Pages.Servicos;

[Authorize(Roles = "Admin")]
public class DeleteModel(ApplicationDbContext db, IHubContext<NotificacoesHub> hubContext) : PageModel
{
    public ServicoDigital? Servico { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Servico = await db.ServicosDigitais
            .Include(s => s.Categoria)
            .Include(s => s.Avaliacoes)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (Servico == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var servico = await db.ServicosDigitais.FindAsync(id);
        if (servico == null) return NotFound();

        var estavaPendente = !servico.Aprovado;
        var nomeServico = servico.Nome;
        var autorId = servico.UtilizadorId;

        // Verificar se a categoria tem outros serviços (proteção de integridade)
        db.ServicosDigitais.Remove(servico);
        await db.SaveChangesAsync();

        // Se o serviço removido estava pendente, é uma rejeição: notificar o autor e
        // atualizar o contador de pendentes nos administradores.
        if (estavaPendente)
        {
            await NotificarAutorAsync(autorId,
                $"O seu serviço '{nomeServico}' não foi aprovado e foi removido.",
                "erro");

            var totalPendentes = await db.ServicosDigitais.CountAsync(s => !s.Aprovado);
            await hubContext.Clients.Group(NotificacoesHub.GrupoAdministradores)
                .SendAsync("AprovacoesAtualizadas", totalPendentes);
        }

        TempData["Sucesso"] = "Serviço eliminado com sucesso.";
        return RedirectToPage("Index");
    }

    // Cria uma notificação pessoal persistida para o autor e, se estiver online, envia-a em tempo real.
    private async Task NotificarAutorAsync(string? autorId, string mensagem, string tipo)
    {
        if (string.IsNullOrEmpty(autorId)) return;

        var notificacao = new Notificacao
        {
            UtilizadorId = autorId,
            Mensagem = mensagem,
            Tipo = tipo
        };
        db.Notificacoes.Add(notificacao);
        await db.SaveChangesAsync();

        var naoLidas = await db.Notificacoes.CountAsync(n => n.UtilizadorId == autorId && !n.Lida);
        await hubContext.Clients.User(autorId)
            .SendAsync("NotificacaoPessoal", mensagem, tipo, naoLidas);
    }
}
