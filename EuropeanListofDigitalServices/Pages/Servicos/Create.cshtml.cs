using EuropeanListofDigitalServices.Data;
using EuropeanListofDigitalServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EuropeanListofDigitalServices.Pages.Servicos;

[Authorize]
public class CreateModel(ApplicationDbContext db) : PageModel
{
    [BindProperty]
    public ServicoDigital Servico { get; set; } = new();

    [BindProperty]
    public List<int> TagsSelecionadas { get; set; } = [];

    public SelectList CategoriasSelectList { get; set; } = null!;
    public List<Tag> Tags { get; set; } = [];

    public async Task OnGetAsync()
    {
        await CarregarDadosFormulario();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ModelState.Remove("Servico.Categoria");
        ModelState.Remove("Servico.UtilizadorId");

        if (!ModelState.IsValid)
        {
            await CarregarDadosFormulario();
            return Page();
        }

        Servico.UtilizadorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Servico.DataSubmissao = DateTime.UtcNow;
        Servico.Aprovado = false;

        // Associar tags selecionadas
        var tags = await db.Tags.Where(t => TagsSelecionadas.Contains(t.Id)).ToListAsync();
        Servico.Tags = tags;

        db.ServicosDigitais.Add(Servico);
        await db.SaveChangesAsync();

        TempData["Sucesso"] = "Serviço submetido com sucesso! Será publicado após aprovação pelo administrador.";
        return RedirectToPage("Index");
    }

    private async Task CarregarDadosFormulario()
    {
        var categorias = await db.Categorias.OrderBy(c => c.Nome).ToListAsync();
        CategoriasSelectList = new SelectList(categorias, "Id", "Nome");
        Tags = await db.Tags.OrderBy(t => t.Nome).ToListAsync();
    }
}
