using EuropeanListofDigitalServices.Data;
using EuropeanListofDigitalServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EuropeanListofDigitalServices.Pages.Servicos;

[Authorize(Roles = "Admin")]
public class DeleteModel(ApplicationDbContext db) : PageModel
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

        // Verificar se a categoria tem outros serviços (proteção de integridade)
        db.ServicosDigitais.Remove(servico);
        await db.SaveChangesAsync();

        TempData["Sucesso"] = "Serviço eliminado com sucesso.";
        return RedirectToPage("Index");
    }
}
