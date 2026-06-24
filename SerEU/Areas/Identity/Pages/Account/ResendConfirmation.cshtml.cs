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

[AllowAnonymous]
public class ResendConfirmationModel(
    UserManager<IdentityUser> userManager,
    IEmailSender emailSender,
    ILogger<ResendConfirmationModel> logger) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public bool Success { get; set; }
    public string? Email { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Introduza um email válido.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await userManager.FindByEmailAsync(Input.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Utilizador não encontrado.");
            return Page();
        }

        if (user.EmailConfirmed)
        {
            ModelState.AddModelError(string.Empty, "O email já está confirmado.");
            return Page();
        }

        // Gera novo token e envia email
        var userId = await userManager.GetUserIdAsync(user);
        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = Url.Page(
            "/Account/ConfirmEmail",
            pageHandler: null,
            values: new { area = "Identity", userId, code },
            protocol: Request.Scheme);

        var logoUrl = $"{Request.Scheme}://{Request.Host}/assets/logo-sereu.svg";
        var htmlmsg = $"<div style='text-align: center;'><img src='{logoUrl}' alt='SerEU Logo' style='height: 64;' /></div>" +
                      $"Olá!<br/><br/>Confirme a sua conta clicando <a href='{HtmlEncoder.Default.Encode(callbackUrl ?? string.Empty)}'>neste link</a>.<br/><br/>" +
                      "Se não solicitou este email, ignore esta mensagem.";

        try
        {
            await emailSender.SendEmailAsync(Input.Email, "Confirme o seu email — SerEU", htmlmsg);
            logger.LogInformation("Email de confirmação reenviado para {Email}", Input.Email);
            
            // Redireciona para a home com mensagem de sucesso
            TempData["Sucesso"] = "Novo email de confirmação enviado! Verifique a sua caixa de entrada.";
            return RedirectToPage("/Index");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao reenviar email de confirmação para {Email}", Input.Email);
            ModelState.AddModelError(string.Empty, "Não foi possível enviar o email. Tente novamente mais tarde.");
        }

        return Page();
    }
}
