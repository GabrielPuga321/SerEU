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

        // Verificar se a categoria tem outros serviços (proteção de integridade)
        db.ServicosDigitais.Remove(servico);
        await db.SaveChangesAsync();

        // Se o serviço removido estava pendente, atualizar o contador nos admins
        if (estavaPendente)
        {
            var totalPendentes = await db.ServicosDigitais.CountAsync(s => !s.Aprovado);
            await hubContext.Clients.Group(NotificacoesHub.GrupoAdministradores)
                .SendAsync("AprovacoesAtualizadas", totalPendentes);
        }

        TempData["Sucesso"] = "Serviço eliminado com sucesso.";
        return RedirectToPage("Index");
    }
}
