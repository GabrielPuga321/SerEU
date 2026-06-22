using EuropeanListofDigitalServices.Data;
using EuropeanListofDigitalServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EuropeanListofDigitalServices.Pages.Admin;

[Authorize(Roles = "Admin")]
public class IndexModel(ApplicationDbContext db) : PageModel
{
    public int TotalServicos { get; set; }
    public int ServicosPendentes { get; set; }
    public int TotalCategorias { get; set; }
    public int TotalTags { get; set; }
    public int TotalUtilizadores { get; set; }
    public List<ServicoDigital> ServicosRecentes { get; set; } = [];

    public async Task OnGetAsync()
    {
        TotalServicos = await db.ServicosDigitais.CountAsync();
        ServicosPendentes = await db.ServicosDigitais.CountAsync(s => !s.Aprovado);
        TotalCategorias = await db.Categorias.CountAsync();
        TotalTags = await db.Tags.CountAsync();
        TotalUtilizadores = await db.Users.CountAsync();

        ServicosRecentes = await db.ServicosDigitais
            .Include(s => s.Categoria)
            .OrderByDescending(s => s.DataSubmissao)
            .Take(10)
            .ToListAsync();
    }
}
