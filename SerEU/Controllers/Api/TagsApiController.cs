using SerEU.Data;
using SerEU.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace SerEU.Controllers.Api;

/// <summary>
/// API REST para gestão de tags.
/// </summary>
[ApiController]
[Route("api/tags")]
[Produces("application/json")]
[EnableRateLimiting("api")]
public class TagsApiController(ApplicationDbContext db) : ControllerBase
{
    /// <summary>
    /// Obtém todas as tags com contagem de serviços.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tags = await db.Tags
            .Include(t => t.Servicos)
            .OrderBy(t => t.Nome)
            .Select(t => new
            {
                t.Id,
                t.Nome,
                TotalServicos = t.Servicos.Count(s => s.Aprovado)
            })
            .ToListAsync();

        return Ok(tags);
    }

    /// <summary>
    /// Obtém uma tag pelo seu ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var tag = await db.Tags
            .Where(t => t.Id == id)
            .Select(t => new
            {
                t.Id,
                t.Nome,
                TotalServicos = t.Servicos.Count(s => s.Aprovado)
            })
            .FirstOrDefaultAsync();

        if (tag == null) return NotFound(new { erro = "Tag não encontrada." });
        return Ok(tag);
    }

    /// <summary>
    /// Cria uma nova tag (requer Admin).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] Tag tag)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        db.Tags.Add(tag);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = tag.Id }, new { tag.Id, tag.Nome });
    }

    /// <summary>
    /// Atualiza uma tag (requer Admin).
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] Tag tagAtualizada)
    {
        var tag = await db.Tags.FindAsync(id);
        if (tag == null) return NotFound(new { erro = "Tag não encontrada." });

        tag.Nome = tagAtualizada.Nome;
        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Elimina uma tag (requer Admin).
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var tag = await db.Tags.FindAsync(id);
        if (tag == null) return NotFound(new { erro = "Tag não encontrada." });

        db.Tags.Remove(tag);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
