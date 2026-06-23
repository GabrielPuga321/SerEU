using SerEU.Data;
using SerEU.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace SerEU.Pages;

public class IndexModel(ApplicationDbContext db) : PageModel
{
    public List<ServicoDigital> ServicosDestaque { get; set; } = [];
    public List<Categoria> Categorias { get; set; } = [];

    public async Task OnGetAsync()
    {
        // Últimos 6 serviços aprovados para destaque
        ServicosDestaque = await db.ServicosDigitais
            .Where(s => s.Aprovado)
            .Include(s => s.Categoria)
            .Include(s => s.Tags)
            .Include(s => s.Avaliacoes)
            .OrderByDescending(s => s.DataSubmissao)
            .Take(6)
            .ToListAsync();

        // Categorias com contagem de serviços aprovados
        Categorias = await db.Categorias
            .Include(c => c.Servicos.Where(s => s.Aprovado))
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }
}
