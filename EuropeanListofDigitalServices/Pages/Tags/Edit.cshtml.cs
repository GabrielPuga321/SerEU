using EuropeanListofDigitalServices.Data;
using EuropeanListofDigitalServices.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace EuropeanListofDigitalServices.Pages.Tags;

[Authorize(Roles = "Admin")]
public class EditModel(ApplicationDbContext db) : PageModel
{
    [BindProperty]
    public Tag Tag { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Tag = await db.Tags.FindAsync(id) ?? null!;
        if (Tag == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        db.Attach(Tag).State = EntityState.Modified;
        await db.SaveChangesAsync();

        TempData["Sucesso"] = "Tag atualizada com sucesso!";
        return RedirectToPage("Index");
    }
}
