using SerEU.Data;
using SerEU.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace SerEU.Pages.Servicos;

[Authorize]
public class EditModel(ApplicationDbContext db) : PageModel
{
    [BindProperty]
    public ServicoDigital Servico { get; set; } = null!;

    [BindProperty]
    public List<int> TagsSelecionadas { get; set; } = [];

    public SelectList CategoriasSelectList { get; set; } = null!;
    public List<Tag> Tags { get; set; } = [];
    public List<int> TagsDoServico { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var utilizadorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        Servico = await db.ServicosDigitais
            .Include(s => s.Tags)
            .FirstOrDefaultAsync(s => s.Id == id) ?? null!;

        if (Servico == null) return NotFound();
        if (Servico.UtilizadorId != utilizadorId && !User.IsInRole("Admin")) return Forbid();

        TagsDoServico = Servico.Tags.Select(t => t.Id).ToList();
        await CarregarDadosFormulario();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var utilizadorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var servicoOriginal = await db.ServicosDigitais
            .Include(s => s.Tags)
            .FirstOrDefaultAsync(s => s.Id == Servico.Id);

        if (servicoOriginal == null) return NotFound();
        if (servicoOriginal.UtilizadorId != utilizadorId && !User.IsInRole("Admin")) return Forbid();

        ModelState.Remove("Servico.Categoria");

        if (!ModelState.IsValid)
        {
            TagsDoServico = TagsSelecionadas;
            await CarregarDadosFormulario();
            return Page();
        }

        servicoOriginal.Nome = Servico.Nome;
        servicoOriginal.Descricao = Servico.Descricao;
        servicoOriginal.Url = Servico.Url;
        servicoOriginal.Pais = Servico.Pais;
        servicoOriginal.Licenca = Servico.Licenca;
        servicoOriginal.LogotipoUrl = Servico.LogotipoUrl;
        servicoOriginal.CategoriaId = Servico.CategoriaId;

        if (User.IsInRole("Admin"))
            servicoOriginal.Aprovado = Servico.Aprovado;

        // Atualizar tags (muitos-para-muitos)
        var novasTags = await db.Tags.Where(t => TagsSelecionadas.Contains(t.Id)).ToListAsync();
        servicoOriginal.Tags.Clear();
        foreach (var tag in novasTags)
            servicoOriginal.Tags.Add(tag);

        await db.SaveChangesAsync();

        TempData["Sucesso"] = "Serviço atualizado com sucesso!";
        return RedirectToPage("Details", new { id = Servico.Id });
    }

    private async Task CarregarDadosFormulario()
    {
        var categorias = await db.Categorias.OrderBy(c => c.Nome).ToListAsync();
        CategoriasSelectList = new SelectList(categorias, "Id", "Nome");
        Tags = await db.Tags.OrderBy(t => t.Nome).ToListAsync();
    }
}
