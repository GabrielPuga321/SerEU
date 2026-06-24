using SerEU.Data;
using SerEU.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace SerEU.Controllers.Api;

/// <summary>
/// API REST para gestão de serviços digitais.
/// </summary>
[ApiController]
[Route("api/servicos")]
[Produces("application/json")]
[EnableRateLimiting("api")]
public class ServicosApiController(ApplicationDbContext db) : ControllerBase
{
    /// <summary>
    /// Obtém todos os serviços aprovados.
    /// </summary>
    /// <param name="pesquisa">Filtro por nome ou descrição (opcional)</param>
    /// <param name="categoriaId">Filtro por categoria (opcional)</param>
    [HttpGet]
    public async Task<IActionResult> GetAll(string? pesquisa = null, int? categoriaId = null)
    {
        var query = db.ServicosDigitais
            .Where(s => s.Aprovado)
            .Include(s => s.Categoria)
            .Include(s => s.Tags)
            .Include(s => s.Avaliacoes)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(pesquisa))
            query = query.Where(s => s.Nome.Contains(pesquisa) || s.Descricao.Contains(pesquisa));

        if (categoriaId.HasValue)
            query = query.Where(s => s.CategoriaId == categoriaId.Value);

        var servicos = await query.OrderBy(s => s.Nome).Select(s => new
        {
            s.Id,
            s.Nome,
            s.Descricao,
            s.Url,
            s.Pais,
            s.Licenca,
            s.LogotipoUrl,
            s.DataSubmissao,
            Categoria = new { s.Categoria.Id, s.Categoria.Nome },
            Tags = s.Tags.Select(t => new { t.Id, t.Nome }),
            MediaAvaliacoes = s.Avaliacoes.Any() ? s.Avaliacoes.Average(a => a.Nota) : 0,
            TotalAvaliacoes = s.Avaliacoes.Count
        }).ToListAsync();

        return Ok(servicos);
    }

    /// <summary>
    /// Obtém um serviço pelo seu ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var servico = await db.ServicosDigitais
            .Where(s => s.Id == id && s.Aprovado)
            .Include(s => s.Categoria)
            .Include(s => s.Tags)
            .Include(s => s.Avaliacoes)
            .Select(s => new
            {
                s.Id,
                s.Nome,
                s.Descricao,
                s.Url,
                s.Pais,
                s.Licenca,
                s.LogotipoUrl,
                s.DataSubmissao,
                Categoria = new { s.Categoria.Id, s.Categoria.Nome },
                Tags = s.Tags.Select(t => new { t.Id, t.Nome }),
                MediaAvaliacoes = s.Avaliacoes.Any() ? s.Avaliacoes.Average(a => a.Nota) : 0,
                TotalAvaliacoes = s.Avaliacoes.Count
            })
            .FirstOrDefaultAsync();

        if (servico == null) return NotFound(new { erro = "Serviço não encontrado." });
        return Ok(servico);
    }

    /// <summary>
    /// Cria um novo serviço (requer autenticação).
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] ServicoDigital servico)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var categoriaExiste = await db.Categorias.AnyAsync(c => c.Id == servico.CategoriaId);
        if (!categoriaExiste)
            return BadRequest(new { erro = "Categoria não encontrada." });

        servico.Aprovado = false;
        servico.DataSubmissao = DateTime.UtcNow;
        servico.Tags = [];

        db.ServicosDigitais.Add(servico);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = servico.Id }, new { servico.Id, servico.Nome });
    }

    /// <summary>
    /// Atualiza um serviço existente (requer autenticação de Admin).
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] ServicoDigital servicoAtualizado)
    {
        var servico = await db.ServicosDigitais.FindAsync(id);
        if (servico == null) return NotFound(new { erro = "Serviço não encontrado." });

        if (!ModelState.IsValid) return BadRequest(ModelState);

        servico.Nome = servicoAtualizado.Nome;
        servico.Descricao = servicoAtualizado.Descricao;
        servico.Url = servicoAtualizado.Url;
        servico.Pais = servicoAtualizado.Pais;
        servico.Licenca = servicoAtualizado.Licenca;
        servico.LogotipoUrl = servicoAtualizado.LogotipoUrl;
        servico.CategoriaId = servicoAtualizado.CategoriaId;
        servico.Aprovado = servicoAtualizado.Aprovado;

        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Elimina um serviço (requer autenticação de Admin).
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var servico = await db.ServicosDigitais.FindAsync(id);
        if (servico == null) return NotFound(new { erro = "Serviço não encontrado." });

        db.ServicosDigitais.Remove(servico);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
