using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

namespace EuropeanListofDigitalServices.Services;

/// <summary>
/// Implementação de <see cref="IEmailSender"/> que envia emails através de um
/// servidor SMTP (por omissão, Gmail). É usada automaticamente pelo ASP.NET Core
/// Identity para confirmação de conta e recuperação de palavra-passe.
/// </summary>
public class SmtpEmailSender(IOptions<EmailSettings> options, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly EmailSettings _settings = options.Value;

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        // Se o SMTP não estiver configurado, regista um aviso em vez de falhar.
        if (string.IsNullOrWhiteSpace(_settings.User) || string.IsNullOrWhiteSpace(_settings.Password))
        {
            logger.LogWarning(
                "Email para {Email} não enviado: servidor SMTP não configurado (User/Password em falta).", email);
            return;
        }

        var remetente = string.IsNullOrWhiteSpace(_settings.FromEmail) ? _settings.User : _settings.FromEmail;

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = new NetworkCredential(_settings.User, _settings.Password)
        };

        using var mensagem = new MailMessage
        {
            From = new MailAddress(remetente, _settings.FromName),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };
        mensagem.To.Add(email);

        try
        {
            await client.SendMailAsync(mensagem);
            logger.LogInformation("Email enviado para {Email}.", email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao enviar email para {Email}.", email);
            throw;
        }
    }
}