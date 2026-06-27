using SerEU.Data;
using SerEU.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace SerEU.Pages.Servicos;

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

    [BindProperty(SupportsGet = true)]
    public string? OrdenarPor { get; set; } = "recentes";

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
            .AsSplitQuery()
            .AsQueryable();

        if (!User.IsInRole("Admin"))
            query = query.Where(s => s.Aprovado);

        if (!string.IsNullOrWhiteSpace(Pesquisa))
            query = query.Where(s => s.Nome.Contains(Pesquisa) || s.Descricao.Contains(Pesquisa));

        if (CategoriaId.HasValue)
            query = query.Where(s => s.CategoriaId == CategoriaId.Value);

        if (TagId.HasValue)
            query = query.Where(s => s.Tags.Any(t => t.Id == TagId.Value));

        // Aplicar ordenação com base no parâmetro
        Servicos = await ApplyOrdering(query).ToListAsync();
    }

    private IOrderedQueryable<ServicoDigital> ApplyOrdering(IQueryable<ServicoDigital> query)
    {
        return OrdenarPor switch
        {
            // Ordenação por nome
            "nome-az" => query.OrderBy(s => s.Nome),
            "nome-za" => query.OrderByDescending(s => s.Nome),
            
            // Ordenação por data de submissão
            "recentes" => query.OrderByDescending(s => s.DataSubmissao),
            "antigos" => query.OrderBy(s => s.DataSubmissao),
            
            // Ordenação por avaliação - usar expressões que EF Core consiga traduzir
            "melhor-avaliado" => query.OrderByDescending(s => s.Avaliacoes.Average(a => a.Nota)).ThenByDescending(s => s.Avaliacoes.Count),
            "pior-avaliado" => query.OrderBy(s => s.Avaliacoes.Average(a => a.Nota)).ThenBy(s => s.Avaliacoes.Count),
            
            // Ordenação por número de avaliações
            "mais-avaliacoes" => query.OrderByDescending(s => s.Avaliacoes.Count).ThenByDescending(s => s.Avaliacoes.Average(a => a.Nota)),
            "menos-avaliacoes" => query.OrderBy(s => s.Avaliacoes.Count).ThenBy(s => s.Avaliacoes.Average(a => a.Nota)),
            
            // Ordenação por país
            "pais-az" => query.OrderBy(s => s.Pais ?? ""),
            "pais-za" => query.OrderByDescending(s => s.Pais ?? ""),
            
            // Default: mais recentes primeiro
            _ => query.OrderByDescending(s => s.DataSubmissao)
        };
    }
}
