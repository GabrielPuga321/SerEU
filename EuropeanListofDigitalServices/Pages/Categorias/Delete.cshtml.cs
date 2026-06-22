using EuropeanListofDigitalServices.Data;
using EuropeanListofDigitalServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EuropeanListofDigitalServices.Pages.Categorias;

[Authorize(Roles = "Admin")]
public class DeleteModel(ApplicationDbContext db) : PageModel
{
    public Categoria? Categoria { get; set; }
    public bool TemServicos { get; set; }
    public int TotalServicos { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Categoria = await db.Categorias
            .Include(c => c.Servicos)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (Categoria == null) return NotFound();

        TotalServicos = Categoria.Servicos.Count;
        TemServicos = TotalServicos > 0;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var categoria = await db.Categorias
            .Include(c => c.Servicos)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categoria == null) return NotFound();

        // Validar que não tem serviços associados
        if (categoria.Servicos.Count > 0)
        {
            TempData["Erro"] = "Não é possível eliminar uma categoria com serviços associados.";
            return RedirectToPage(new { id });
        }

        db.Categorias.Remove(categoria);
        await db.SaveChangesAsync();

        TempData["Sucesso"] = "Categoria eliminada com sucesso.";
        return RedirectToPage("Index");
    }
}
