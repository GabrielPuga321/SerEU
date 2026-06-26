using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;

namespace SerEU.Services;

/// <summary>
/// Implementação de <see cref="IEmailSender"/> que envia emails através de um
/// servidor SMTP usando a biblioteca MailKit (recomendada em vez de System.Net.Mail.SmtpClient).
/// É usada automaticamente pelo ASP.NET Core Identity para confirmação de conta e recuperação de palavra-passe.
/// </summary>
public class MailKitEmailSender(IOptions<EmailSettings> options, ILogger<MailKitEmailSender> logger) : IEmailSender
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

        var mensagem = new MimeMessage
        {
            Subject = subject,
            Body = new TextPart("html")
            {
                Text = htmlMessage
            }
        };

        mensagem.From.Add(new MailboxAddress(_settings.FromName, remetente));
        mensagem.To.Add(new MailboxAddress("", email));

        try
        {
            using var client = new SmtpClient();
            
            // Determinar o nível de segurança com base em EnableSsl, o proton mail usa STARTTLS
            var secureSocketOptions = _settings.EnableSsl 
                ? SecureSocketOptions.StartTlsWhenAvailable 
                : SecureSocketOptions.None;

            await client.ConnectAsync(_settings.Host, _settings.Port, secureSocketOptions);
            await client.AuthenticateAsync(_settings.User, _settings.Password);
            await client.SendAsync(mensagem);
            await client.DisconnectAsync(true);

            logger.LogInformation("Email enviado para {Email}.", email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Falha ao enviar email para {Email} com o SMTP {Host}:{Port}.", email, _settings.Host, _settings.Port);
            throw;
        }
    }
}
