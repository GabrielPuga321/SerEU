using SerEU.Data;
using SerEU.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace SerEU.Pages.Tags;

public class IndexModel(ApplicationDbContext db) : PageModel
{
    public List<Tag> Tags { get; set; } = [];

    public async Task OnGetAsync()
    {
        Tags = await db.Tags
            .Include(t => t.Servicos)
            .OrderBy(t => t.Nome)
            .ToListAsync();
    }
}
