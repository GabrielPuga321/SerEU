using SerEU.Data;
using SerEU.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace SerEU.Pages.Servicos;

public class DetailsModel(ApplicationDbContext db) : PageModel
{
    public ServicoDigital? Servico { get; set; }
    public List<ServicoDigital> ServicosRelacionados { get; set; } = [];
    public string? UtilizadorAtualId { get; set; }
    public bool JaAvaliou { get; set; }

    // Avaliação que o utilizador atual já deixou neste serviço (se existir).
    // Serve para pré-preencher o formulário de edição.
    public Avaliacao? MinhaAvaliacao { get; set; }

    [BindProperty]
    public Avaliacao NovaAvaliacao { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        UtilizadorAtualId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        Servico = await db.ServicosDigitais
            .Include(s => s.Categoria)
            .Include(s => s.Tags)
            .Include(s => s.Avaliacoes).ThenInclude(a => a.Utilizador)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (Servico == null) return NotFound();

        // Serviço não aprovado: apenas o dono ou admin pode ver
        if (!Servico.Aprovado && !User.IsInRole("Admin") && Servico.UtilizadorId != UtilizadorAtualId)
            return Forbid();

        MinhaAvaliacao = UtilizadorAtualId == null
            ? null
            : Servico.Avaliacoes.FirstOrDefault(a => a.UtilizadorId == UtilizadorAtualId);
        JaAvaliou = MinhaAvaliacao != null;

        // Pré-preencher o formulário com a avaliação existente para permitir a edição.
        if (MinhaAvaliacao != null)
            NovaAvaliacao = MinhaAvaliacao;

        ServicosRelacionados = await db.ServicosDigitais
            .Where(s => s.CategoriaId == Servico.CategoriaId && s.Id != id && s.Aprovado)
            .Include(s => s.Categoria)
            .Take(4)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAvaliarAsync()
    {
        UtilizadorAtualId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (UtilizadorAtualId == null) return Challenge();

        // As propriedades de navegação não vêm do formulário; o EF preenche-as a
        // partir das chaves estrangeiras. Sem isto, o Nullable=enable trata-as como
        // [Required] e invalida o ModelState, impedindo a submissão.
        ModelState.Remove("NovaAvaliacao.ServicoDigital");
        ModelState.Remove("NovaAvaliacao.Utilizador");

        // O serviço tem de existir e estar aprovado para poder ser avaliado
        var servicoValido = await db.ServicosDigitais
            .AnyAsync(s => s.Id == NovaAvaliacao.ServicoDigitalId && s.Aprovado);
        if (!servicoValido)
        {
            TempData["Erro"] = "Não é possível avaliar este serviço.";
            return RedirectToPage("Index");
        }

        // Verificar duplicado
        var jaExiste = await db.Avaliacoes.AnyAsync(a =>
            a.ServicoDigitalId == NovaAvaliacao.ServicoDigitalId &&
            a.UtilizadorId == UtilizadorAtualId);

        if (jaExiste)
        {
            TempData["Aviso"] = "Já avaliou este serviço anteriormente.";
            return RedirectToPage(new { id = NovaAvaliacao.ServicoDigitalId });
        }

        if (!ModelState.IsValid)
        {
            Servico = await db.ServicosDigitais
                .Include(s => s.Categoria).Include(s => s.Tags)
                .Include(s => s.Avaliacoes).ThenInclude(a => a.Utilizador)
                .AsSplitQuery()
                .FirstOrDefaultAsync(s => s.Id == NovaAvaliacao.ServicoDigitalId);
            return Page();
        }

        NovaAvaliacao.UtilizadorId = UtilizadorAtualId;
        NovaAvaliacao.Data = DateTime.UtcNow;
        db.Avaliacoes.Add(NovaAvaliacao);
        await db.SaveChangesAsync();

        TempData["Sucesso"] = "Avaliação submetida com sucesso!";
        return RedirectToPage(new { id = NovaAvaliacao.ServicoDigitalId });
    }

    public async Task<IActionResult> OnPostEditarAvaliacaoAsync()
    {
        UtilizadorAtualId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (UtilizadorAtualId == null) return Challenge();

        ModelState.Remove("NovaAvaliacao.ServicoDigital");
        ModelState.Remove("NovaAvaliacao.Utilizador");

        var avaliacao = await db.Avaliacoes.FindAsync(NovaAvaliacao.Id);
        if (avaliacao == null) return NotFound();

        // Só o autor da avaliação (ou um admin) a pode editar
        if (avaliacao.UtilizadorId != UtilizadorAtualId && !User.IsInRole("Admin"))
            return Forbid();

        if (!ModelState.IsValid)
        {
            Servico = await db.ServicosDigitais
                .Include(s => s.Categoria).Include(s => s.Tags)
                .Include(s => s.Avaliacoes).ThenInclude(a => a.Utilizador)
                .FirstOrDefaultAsync(s => s.Id == avaliacao.ServicoDigitalId);
            MinhaAvaliacao = avaliacao;
            JaAvaliou = true;
            return Page();
        }

        // Atualizar apenas os campos editáveis; a data passa a refletir a alteração
        avaliacao.Nota = NovaAvaliacao.Nota;
        avaliacao.Comentario = NovaAvaliacao.Comentario;
        avaliacao.Data = DateTime.UtcNow;
        await db.SaveChangesAsync();

        TempData["Sucesso"] = "Avaliação atualizada com sucesso!";
        return RedirectToPage(new { id = avaliacao.ServicoDigitalId });
    }

    public async Task<IActionResult> OnPostEliminarAvaliacaoAsync(int avaliacaoId)
    {
        UtilizadorAtualId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var avaliacao = await db.Avaliacoes.FindAsync(avaliacaoId);

        if (avaliacao == null) return NotFound();
        if (avaliacao.UtilizadorId != UtilizadorAtualId && !User.IsInRole("Admin"))
            return Forbid();

        var servicoId = avaliacao.ServicoDigitalId;
        db.Avaliacoes.Remove(avaliacao);
        await db.SaveChangesAsync();

        TempData["Sucesso"] = "Avaliação eliminada.";
        return RedirectToPage(new { id = servicoId });
    }
}
