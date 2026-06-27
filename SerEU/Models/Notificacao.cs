using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SerEU.Models;

/// <summary>
/// Notificação pessoal dirigida a um utilizador (ex.: serviço aprovado ou rejeitado).
/// É persistida para que o utilizador a veja mesmo que não esteja online no momento.
/// </summary>
public class Notificacao
{
    public int Id { get; set; }

    // Destinatário da notificação.
    [Required]
    public string UtilizadorId { get; set; } = string.Empty;
    public IdentityUser? Utilizador { get; set; }

    [Required]
    [StringLength(500)]
    public string Mensagem { get; set; } = string.Empty;

    // "sucesso" (aprovado) ou "erro" (rejeitado). Usado para o estilo visual.
    [StringLength(20)]
    public string Tipo { get; set; } = "info";

    public bool Lida { get; set; }

    public DateTime Data { get; set; } = DateTime.UtcNow;
}
