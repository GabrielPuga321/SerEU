using EuropeanListofDigitalServices.Data;
using EuropeanListofDigitalServices.Hubs;
using EuropeanListofDigitalServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EuropeanListofDigitalServices.Pages.Admin;

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

        TempData["Sucesso"] = $"Serviço '{servico.Nome}' aprovado e publicado com sucesso!";
        return RedirectToPage();
    }
}
