using System.ComponentModel.DataAnnotations;

namespace EuropeanListofDigitalServices.Models;

public class Tag
{
    public int Id { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(50, ErrorMessage = "O nome não pode ter mais de 50 caracteres.")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    public ICollection<ServicoDigital> Servicos { get; set; } = new List<ServicoDigital>();
}
