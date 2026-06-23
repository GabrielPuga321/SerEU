using SerEU.Data;
using SerEU.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SerEU.Pages.Categorias;

[Authorize(Roles = "Admin")]
public class CreateModel(ApplicationDbContext db) : PageModel
{
    [BindProperty]
    public Categoria Categoria { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        db.Categorias.Add(Categoria);
        await db.SaveChangesAsync();

        TempData["Sucesso"] = $"Categoria '{Categoria.Nome}' criada com sucesso!";
        return RedirectToPage("Index");
    }
}
