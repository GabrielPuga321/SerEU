using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace SerEU.Areas.Identity.Pages.Account;

/// <summary>
/// Página de registo de novos utilizadores.
/// </summary>
[AllowAnonymous]
public class RegisterModel(
    UserManager<IdentityUser> userManager,
    IUserStore<IdentityUser> userStore,
    SignInManager<IdentityUser> signInManager,
    IEmailSender emailSender,
    ILogger<RegisterModel> logger) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Introduza um email válido.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A palavra-passe é obrigatória.")]
        [StringLength(100, ErrorMessage = "A palavra-passe deve ter entre {2} e {1} caracteres.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        [Display(Name = "Palavra-passe")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar palavra-passe")]
        [Compare(nameof(Password), ErrorMessage = "As palavras-passe não coincidem.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
        {
            ReturnUrl = returnUrl;
            return Page();
        }

        var user = new IdentityUser();
        await userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);

        var emailStore = (IUserEmailStore<IdentityUser>)userStore;
        await emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

        var result = await userManager.CreateAsync(user, Input.Password);

        if (result.Succeeded)
        {
            logger.LogInformation("Novo utilizador registado.");

            // Atribui o papel base de utilizador
            await userManager.AddToRoleAsync(user, "Utilizador");

            // Gera o token de confirmação e constrói o link de validação
            var userId = await userManager.GetUserIdAsync(user);
            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId, code, returnUrl },
                protocol: Request.Scheme);

            await emailSender.SendEmailAsync(Input.Email, "Confirme o seu email — Serviços Digitais Europeus",
                $"Olá!<br/><br/>Obrigado por se registar nos <strong>Serviços Digitais Europeus</strong>.<br/>" +
                $"Confirme a sua conta clicando <a href='{HtmlEncoder.Default.Encode(callbackUrl ?? string.Empty)}'>neste link</a>.<br/><br/>" +
                "Se não foi você a criar esta conta, ignore este email.");

            // Caso seja exigida confirmação de conta, encaminha para a página respetiva
            if (userManager.Options.SignIn.RequireConfirmedAccount)
                return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });

            await signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(returnUrl);
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        ReturnUrl = returnUrl;
        return Page();
    }
}