using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace EuropeanListofDigitalServices.Areas.Identity.Pages.Account;

/// <summary>
/// Página apresentada após o registo, a pedir ao utilizador que confirme o email.
/// </summary>
[AllowAnonymous]
public class RegisterConfirmationModel(UserManager<IdentityUser> userManager, IWebHostEnvironment environment) : PageModel
{
    public string Email { get; set; } = string.Empty;

    /// <summary>Em desenvolvimento mostra o link direto de confirmação (sem aceder ao email).</summary>
    public bool DisplayConfirmAccountLink { get; set; }

    public string? EmailConfirmationUrl { get; set; }

    public async Task<IActionResult> OnGetAsync(string? email, string? returnUrl = null)
    {
        if (email == null)
            return RedirectToPage("/Index");

        returnUrl ??= Url.Content("~/");

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            return NotFound($"Não foi possível encontrar um utilizador com o email '{email}'.");

        Email = email;

        // Conveniência de desenvolvimento: permite confirmar a conta sem servidor de email.
        DisplayConfirmAccountLink = environment.IsDevelopment();
        if (DisplayConfirmAccountLink)
        {
            var userId = await userManager.GetUserIdAsync(user);
            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            EmailConfirmationUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId, code, returnUrl },
                protocol: Request.Scheme);
        }

        return Page();
    }
}