using EuropeanListofDigitalServices.Data;
using EuropeanListofDigitalServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EuropeanListofDigitalServices.Pages.Tags;

[Authorize(Roles = "Admin")]
public class CreateModel(ApplicationDbContext db) : PageModel
{
    [BindProperty]
    public Tag Tag { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        db.Tags.Add(Tag);
        await db.SaveChangesAsync();

        TempData["Sucesso"] = $"Tag '{Tag.Nome}' criada com sucesso!";
        return RedirectToPage("Index");
    }
}
