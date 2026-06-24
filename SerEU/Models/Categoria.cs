using System.ComponentModel.DataAnnotations;

namespace SerEU.Models;

public class Categoria
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome não pode ter mais de 100 caracteres.")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "A descrição não pode ter mais de 500 caracteres.")]
    [Display(Name = "Descrição")]
    public string? Descricao { get; set; }

    // Nome do ícone Bootstrap Icons (ex: "bi-envelope", "bi-shield-lock")
    [StringLength(50)]
    [Display(Name = "Ícone")]
    public string? Icone { get; set; }

    public ICollection<ServicoDigital> Servicos { get; set; } = new List<ServicoDigital>();
}
