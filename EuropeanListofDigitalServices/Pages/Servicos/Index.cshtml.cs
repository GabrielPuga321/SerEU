using EuropeanListofDigitalServices.Data;
using EuropeanListofDigitalServices.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EuropeanListofDigitalServices.Pages.Servicos;

public class IndexModel(ApplicationDbContext db) : PageModel
{
    public List<ServicoDigital> Servicos { get; set; } = [];
    public List<Categoria> Categorias { get; set; } = [];
    public List<Tag> Tags { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? Pesquisa { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? CategoriaId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? TagId { get; set; }

    public string? UtilizadorAtualId { get; set; }

    public async Task OnGetAsync()
    {
        UtilizadorAtualId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        Categorias = await db.Categorias.OrderBy(c => c.Nome).ToListAsync();
        Tags = await db.Tags.OrderBy(t => t.Nome).ToListAsync();

        // Utilizadores admin vêem todos os serviços; utilizadores normais vêem só aprovados
        var query = db.ServicosDigitais
            .Include(s => s.Categoria)
            .Include(s => s.Tags)
            .Include(s => s.Avaliacoes)
            .AsQueryable();

        if (!User.IsInRole("Admin"))
            query = query.Where(s => s.Aprovado);

        if (!string.IsNullOrWhiteSpace(Pesquisa))
            query = query.Where(s => s.Nome.Contains(Pesquisa) || s.Descricao.Contains(Pesquisa));

        if (CategoriaId.HasValue)
            query = query.Where(s => s.CategoriaId == CategoriaId.Value);

        if (TagId.HasValue)
            query = query.Where(s => s.Tags.Any(t => t.Id == TagId.Value));

        Servicos = await query.OrderByDescending(s => s.DataSubmissao).ToListAsync();
    }
}
