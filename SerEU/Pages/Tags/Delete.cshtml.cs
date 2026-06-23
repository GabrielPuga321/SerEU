using SerEU.Data;
using SerEU.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace SerEU.Pages.Tags;

[Authorize(Roles = "Admin")]
public class DeleteModel(ApplicationDbContext db) : PageModel
{
    public Tag? Tag { get; set; }
    public int TotalServicos { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Tag = await db.Tags.Include(t => t.Servicos).FirstOrDefaultAsync(t => t.Id == id);
        if (Tag == null) return NotFound();
        TotalServicos = Tag.Servicos.Count;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var tag = await db.Tags.Include(t => t.Servicos).FirstOrDefaultAsync(t => t.Id == id);
        if (tag == null) return NotFound();

        db.Tags.Remove(tag);
        await db.SaveChangesAsync();

        TempData["Sucesso"] = "Tag eliminada com sucesso.";
        return RedirectToPage("Index");
    }
}
