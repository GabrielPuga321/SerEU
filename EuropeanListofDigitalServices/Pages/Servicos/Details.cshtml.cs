using EuropeanListofDigitalServices.Data;
using EuropeanListofDigitalServices.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EuropeanListofDigitalServices.Pages.Servicos;

public class DetailsModel(ApplicationDbContext db) : PageModel
{
    public ServicoDigital? Servico { get; set; }
    public List<ServicoDigital> ServicosRelacionados { get; set; } = [];
    public string? UtilizadorAtualId { get; set; }
    public bool JaAvaliou { get; set; }

    [BindProperty]
    public Avaliacao NovaAvaliacao { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        UtilizadorAtualId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        Servico = await db.ServicosDigitais
            .Include(s => s.Categoria)
            .Include(s => s.Tags)
            .Include(s => s.Avaliacoes).ThenInclude(a => a.Utilizador)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (Servico == null) return NotFound();

        // Serviço não aprovado: apenas o dono ou admin pode ver
        if (!Servico.Aprovado && !User.IsInRole("Admin") && Servico.UtilizadorId != UtilizadorAtualId)
            return Forbid();

        JaAvaliou = UtilizadorAtualId != null &&
                    Servico.Avaliacoes.Any(a => a.UtilizadorId == UtilizadorAtualId);

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
