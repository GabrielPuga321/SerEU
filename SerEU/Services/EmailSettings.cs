namespace SerEU.Services;

/// <summary>
/// Definições do servidor SMTP usadas para o envio de emails.
/// Os valores sensíveis (utilizador/palavra-passe) devem ser guardados em
/// user-secrets ou variáveis de ambiente, nunca no controlo de versões.
/// </summary>
public class EmailSettings
{
    /// <summary>Servidor SMTP.</summary>
    public string Host { get; set; } = "smtp.protonmail.ch";

    /// <summary>Porta SMTP (587 para STARTTLS).</summary>
    public int Port { get; set; } = 587;

    /// <summary>Ativa TLS/SSL.</summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>Conta de autenticação SMTP</summary>
    public string User { get; set; } = string.Empty;

    /// <summary>Palavra-passe de aplicação .</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>Endereço de remetente. Se vazio, usa <see cref="User"/>.</summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>Nome apresentado como remetente.</summary>
    public string FromName { get; set; } = "SerEU";
}