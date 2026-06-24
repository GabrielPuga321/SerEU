using SerEU.Data;
using SerEU.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace SerEU.Controllers.Api;

/// <summary>
/// API REST para gestão de categorias.
/// </summary>
[ApiController]
[Route("api/categorias")]
[Produces("application/json")]
[EnableRateLimiting("api")]
public class CategoriasApiController(ApplicationDbContext db) : ControllerBase
{
    /// <summary>
    /// Obtém todas as categorias com a contagem de serviços.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categorias = await db.Categorias
            .Include(c => c.Servicos)
            .OrderBy(c => c.Nome)
            .Select(c => new
            {
                c.Id,
                c.Nome,
                c.Descricao,
                c.Icone,
                TotalServicos = c.Servicos.Count(s => s.Aprovado)
            })
            .ToListAsync();

        return Ok(categorias);
    }

    /// <summary>
    /// Obtém uma categoria pelo seu ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var cat = await db.Categorias
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id,
                c.Nome,
                c.Descricao,
                c.Icone,
                TotalServicos = c.Servicos.Count(s => s.Aprovado)
            })
            .FirstOrDefaultAsync();

        if (cat == null) return NotFound(new { erro = "Categoria não encontrada." });
        return Ok(cat);
    }

    /// <summary>
    /// Cria uma nova categoria (requer Admin).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] Categoria categoria)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        db.Categorias.Add(categoria);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, new { categoria.Id, categoria.Nome });
    }

    /// <summary>
    /// Atualiza uma categoria (requer Admin).
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] Categoria categoriaAtualizada)
    {
        var cat = await db.Categorias.FindAsync(id);
        if (cat == null) return NotFound(new { erro = "Categoria não encontrada." });

        cat.Nome = categoriaAtualizada.Nome;
        cat.Descricao = categoriaAtualizada.Descricao;
        cat.Icone = categoriaAtualizada.Icone;

        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Elimina uma categoria (requer Admin). Falha se houver serviços associados.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var cat = await db.Categorias.Include(c => c.Servicos).FirstOrDefaultAsync(c => c.Id == id);
        if (cat == null) return NotFound(new { erro = "Categoria não encontrada." });

        if (cat.Servicos.Count > 0)
            return Conflict(new { erro = "Não é possível eliminar uma categoria com serviços associados." });

        db.Categorias.Remove(cat);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
