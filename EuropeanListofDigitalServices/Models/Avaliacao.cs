using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace EuropeanListofDigitalServices.Models;

public class Avaliacao
{
    public int Id { get; set; }

    [Required]
    public int ServicoDigitalId { get; set; }
    public ServicoDigital ServicoDigital { get; set; } = null!;

    public string? UtilizadorId { get; set; }
    public IdentityUser? Utilizador { get; set; }

    [Required(ErrorMessage = "A nota é obrigatória.")]
    [Range(1, 5, ErrorMessage = "A nota deve estar entre 1 e 5.")]
    [Display(Name = "Nota (1-5)")]
    public int Nota { get; set; }

    [StringLength(1000, ErrorMessage = "O comentário não pode ter mais de 1000 caracteres.")]
    [Display(Name = "Comentário")]
    public string? Comentario { get; set; }

    [Display(Name = "Data")]
    public DateTime Data { get; set; } = DateTime.UtcNow;
}
