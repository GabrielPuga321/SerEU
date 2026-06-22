using EuropeanListofDigitalServices.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace EuropeanListofDigitalServices.Controllers.Api;

/// <summary>
/// API REST para consulta de avaliações.
/// </summary>
[ApiController]
[Route("api/avaliacoes")]
[Produces("application/json")]
[EnableRateLimiting("api")]
public class AvaliacoesApiController(ApplicationDbContext db) : ControllerBase
{
    /// <summary>
    /// Obtém as avaliações de um serviço específico.
    /// </summary>
    [HttpGet("servico/{servicoId:int}")]
    public async Task<IActionResult> GetByServico(int servicoId)
    {
        var existe = await db.ServicosDigitais.AnyAsync(s => s.Id == servicoId && s.Aprovado);
        if (!existe) return NotFound(new { erro = "Serviço não encontrado." });

        var avaliacoes = await db.Avaliacoes
            .Where(a => a.ServicoDigitalId == servicoId)
            .Include(a => a.Utilizador)
            .OrderByDescending(a => a.Data)
            .Select(a => new
            {
                a.Id,
                a.Nota,
                a.Comentario,
                a.Data,
                Utilizador = a.Utilizador != null ? a.Utilizador.Email : "Anónimo"
            })
            .ToListAsync();

        var media = avaliacoes.Any() ? avaliacoes.Average(a => a.Nota) : 0;
        return Ok(new { Media = Math.Round(media, 1), Total = avaliacoes.Count, Avaliacoes = avaliacoes });
    }
}
