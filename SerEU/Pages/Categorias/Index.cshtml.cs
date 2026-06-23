using SerEU.Data;
using SerEU.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace SerEU.Pages.Categorias;

public class IndexModel(ApplicationDbContext db) : PageModel
{
    public List<Categoria> Categorias { get; set; } = [];

    public async Task OnGetAsync()
    {
        Categorias = await db.Categorias
            .Include(c => c.Servicos)
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }
}
