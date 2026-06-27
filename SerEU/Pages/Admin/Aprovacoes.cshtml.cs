using SerEU.Data;
using SerEU.Hubs;
using SerEU.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace SerEU.Pages.Admin;

[Authorize(Roles = "Admin")]
public class AprovacoesModel(ApplicationDbContext db, IHubContext<NotificacoesHub> hubContext) : PageModel
{
    public List<ServicoDigital> Servicos { get; set; } = [];
    public int ServicosPendentes { get; set; }

    public async Task OnGetAsync()
    {
        Servicos = await db.ServicosDigitais
            .Where(s => !s.Aprovado)
            .Include(s => s.Categoria)
            .Include(s => s.Tags)
            .AsSplitQuery()
            .OrderBy(s => s.DataSubmissao)
            .ToListAsync();

        ServicosPendentes = Servicos.Count;
    }

    public async Task<IActionResult> OnPostAprovarAsync(int id)
    {
        var servico = await db.ServicosDigitais.FindAsync(id);
        if (servico == null) return NotFound();

        servico.Aprovado = true;
        await db.SaveChangesAsync();

        // Notificar todos os clientes via SignalR que um novo serviço foi aprovado
        await hubContext.Clients.All.SendAsync("NovoServicoAprovado", servico.Nome);

        // Notificar pessoalmente o autor do serviço (persistido + tempo real)
        await NotificarAutorAsync(servico.UtilizadorId,
            $"O seu serviço '{servico.Nome}' foi aprovado e já está publicado. 🎉",
            "sucesso");

        // Atualizar o contador de aprovações pendentes nos administradores
        var totalPendentes = await db.ServicosDigitais.CountAsync(s => !s.Aprovado);
        await hubContext.Clients.Group(NotificacoesHub.GrupoAdministradores)
            .SendAsync("AprovacoesAtualizadas", totalPendentes);

        TempData["Sucesso"] = $"Serviço '{servico.Nome}' aprovado e publicado com sucesso!";
        return RedirectToPage();
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
