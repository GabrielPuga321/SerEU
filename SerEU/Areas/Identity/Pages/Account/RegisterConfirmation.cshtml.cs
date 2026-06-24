using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SerEU.Areas.Identity.Pages.Account;

/// <summary>
/// Página apresentada após o registo, a pedir ao utilizador que confirme o email.
/// </summary>
[AllowAnonymous]
public class RegisterConfirmationModel(UserManager<IdentityUser> userManager) : PageModel
{
    public string Email { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(string? email, string? returnUrl = null)
    {
        if (email == null)
            return RedirectToPage("/Index");

        returnUrl ??= Url.Content("~/");

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound($"Não foi possível encontrar um utilizador com o email '{email}'.");

        Email = email;

        return Page();
    }
}
