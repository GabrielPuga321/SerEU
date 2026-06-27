using SerEU.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace SerEU.Controllers.Api;

/// <summary>
/// API para as notificações pessoais do utilizador autenticado.
/// </summary>
[ApiController]
[Route("api/notificacoes")]
[Authorize]
[Produces("application/json")]
public class NotificacoesApiController(ApplicationDbContext db) : ControllerBase
{
    /// <summary>
    /// Lista as notificações mais recentes do utilizador autenticado.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetMinhas()
    {
        var utilizadorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(utilizadorId)) return Unauthorized();

        var notificacoes = await db.Notificacoes
            .Where(n => n.UtilizadorId == utilizadorId)
            .OrderByDescending(n => n.Data)
            .Take(20)
            .Select(n => new { n.Id, n.Mensagem, n.Tipo, n.Lida, n.Data })
            .ToListAsync();

        var naoLidas = notificacoes.Count(n => !n.Lida);
        return Ok(new { naoLidas, notificacoes });
    }

    /// <summary>
    /// Marca todas as notificações do utilizador como lidas.
    /// </summary>
    [HttpPost("marcar-lidas")]
    public async Task<IActionResult> MarcarLidas()
    {
        var utilizadorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(utilizadorId)) return Unauthorized();

        await db.Notificacoes
            .Where(n => n.UtilizadorId == utilizadorId && !n.Lida)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.Lida, true));

        return NoContent();
    }
}
