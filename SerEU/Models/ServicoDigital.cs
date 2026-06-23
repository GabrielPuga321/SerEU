using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SerEU.Models;

public class ServicoDigital
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(200, ErrorMessage = "O nome não pode ter mais de 200 caracteres.")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(2000, ErrorMessage = "A descrição não pode ter mais de 2000 caracteres.")]
    [Display(Name = "Descrição")]
    public string Descricao { get; set; } = string.Empty;

    [Required(ErrorMessage = "O URL é obrigatório.")]
    [Url(ErrorMessage = "O URL introduzido não é válido.")]
    [StringLength(500)]
    [Display(Name = "URL")]
    public string Url { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "País de Origem")]
    public string? Pais { get; set; }

    [StringLength(100)]
    [Display(Name = "Licença")]
    public string? Licenca { get; set; }

    [StringLength(500)]
    [Url(ErrorMessage = "O URL do logótipo não é válido.")]
    [Display(Name = "URL do Logótipo")]
    public string? LogotipoUrl { get; set; }

    [Display(Name = "Data de Submissão")]
    public DateTime DataSubmissao { get; set; } = DateTime.UtcNow;

    [Display(Name = "Aprovado")]
    public bool Aprovado { get; set; } = false;

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    [Display(Name = "Categoria")]
    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;

    public string? UtilizadorId { get; set; }
    public IdentityUser? Utilizador { get; set; }

    // Relação muitos-para-muitos com Tag
    public ICollection<Tag> Tags { get; set; } = new List<Tag>();

    // Relação um-para-muitos com Avaliacao
    public ICollection<Avaliacao> Avaliacoes { get; set; } = new List<Avaliacao>();

    public double MediaAvaliacoes => Avaliacoes.Any() ? Avaliacoes.Average(a => a.Nota) : 0;
    public int TotalAvaliacoes => Avaliacoes.Count;
}
