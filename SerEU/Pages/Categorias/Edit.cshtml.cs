using SerEU.Data;
using SerEU.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace SerEU.Pages.Categorias;

[Authorize(Roles = "Admin")]
public class EditModel(ApplicationDbContext db) : PageModel
{
    [BindProperty]
    public Categoria Categoria { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Categoria = await db.Categorias.FindAsync(id) ?? null!;
        if (Categoria == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        db.Attach(Categoria).State = EntityState.Modified;

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await db.Categorias.AnyAsync(c => c.Id == Categoria.Id))
                return NotFound();
            throw;
        }

        TempData["Sucesso"] = "Categoria atualizada com sucesso!";
        return RedirectToPage("Index");
    }
}
