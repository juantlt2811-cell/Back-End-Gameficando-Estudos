using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.ValueObjects;
using GameficandoEstudos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameficandoEstudos.Infrastructure.Repositories;

public class CaixaFontesRepository : ICaixaFontesRepository
{
    private readonly AppDbContext _context;

    public CaixaFontesRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CaixaFontes> AdicionarAsync(CaixaFontes caixaFontes)
    {
        await _context.CaixaFontes.AddAsync(caixaFontes);
        await _context.SaveChangesAsync();
        return caixaFontes;
    }

    public async Task<CaixaFontes?> ObterPorIdAsync(int id)
    {
        return await _context.CaixaFontes
            .Include(c => c.Chunks)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CaixaFontes> ObterPorMateriaAsync(int idMateria)
    {
        return (await _context.CaixaFontes
            .Include(c => c.Chunks)
            .FirstOrDefaultAsync(c => c.IdMateria == idMateria))!;
    }

    public async Task<CaixaFontes> AtualizarAsync(CaixaFontes caixaFontes)
    {
        _context.CaixaFontes.Update(caixaFontes);
        await _context.SaveChangesAsync();
        return caixaFontes;
    }

    public async Task RemoverChunksPorDocumentoIdAsync(Guid documentoId)
    {
        await _context.FonteChunks
            .Where(fc => fc.DocumentoId == documentoId)
            .ExecuteDeleteAsync();
    }

    public async Task<IEnumerable<ResumoOrigem>> PesquisarResumoMateriaisAsync(int idMateria, string? termoBusca)
    {
        var query = _context.FonteChunks
            .Where(fc => fc.CaixaFonte.IdMateria == idMateria);

        if (!string.IsNullOrWhiteSpace(termoBusca))
        {
            query = query.Where(fc => fc.Origem.Contains(termoBusca) || fc.Conteudo.Contains(termoBusca));
        }

        var resumos = await query
            .GroupBy(fc => new { fc.DocumentoId, fc.Origem })
            .Select(g => new ResumoOrigem(
                g.Key.DocumentoId,
                g.Key.Origem, 
                g.Count(),
                g.Max(x => x.AtualizadoEm) 
            ))
            .ToListAsync();

        return resumos;
    }
}