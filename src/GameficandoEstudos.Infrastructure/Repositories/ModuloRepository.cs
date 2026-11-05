using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GameficandoEstudos.Infrastructure.Repositories;

public class ModuloRepository : IModuloRepository
{
    private readonly AppDbContext _context;

    public ModuloRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Modulo> AdicionarAsync(Modulo modulo)
    {
        await _context.Modulos.AddAsync(modulo);
        await SalvarTratandoNomeDuplicadoAsync();
        return modulo;
    }

    public async Task<Modulo?> ObterPorIdAsync(int id)
    {
        return await _context.Modulos.FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<List<ModuloDto>> ListarPorMateriaAsync(int idMateria)
    {
        var modulos = await _context.Modulos
            .Where(m => m.IdMateria == idMateria)
            .OrderBy(m => m.Id)
            .ToListAsync();

        if (modulos.Count == 0)
            return new List<ModuloDto>();

        var atividadesPorModulo = await _context.Atividades
            .Where(a => a.IdMateria == idMateria && a.Ativa && a.IdModulo != null)
            .GroupBy(a => a.IdModulo!.Value)
            .Select(g => new { IdModulo = g.Key, Quantidade = g.Count() })
            .ToDictionaryAsync(x => x.IdModulo, x => x.Quantidade);

        
        var documentosPorModulo = await _context.FonteChunks
            .Where(fc => fc.CaixaFonte.IdMateria == idMateria && fc.IdModulo != null)
            .Select(fc => new { IdModulo = fc.IdModulo!.Value, fc.DocumentoId })
            .Distinct()
            .ToListAsync();

        var materiaisPorModulo = documentosPorModulo
            .GroupBy(x => x.IdModulo)
            .ToDictionary(g => g.Key, g => g.Count());

        return modulos
            .Select(m => new ModuloDto(
                m.Id,
                m.IdMateria,
                m.Nome,
                atividadesPorModulo.GetValueOrDefault(m.Id),
                materiaisPorModulo.GetValueOrDefault(m.Id)))
            .ToList();
    }

    public async Task<bool> ExisteComNomeAsync(int idMateria, string nome, int? ignorarIdModulo)
    {
        var nomeNormalizado = nome.Trim().ToLower();

        return await _context.Modulos.AnyAsync(m =>
            m.IdMateria == idMateria &&
            m.Nome.ToLower() == nomeNormalizado &&
            (ignorarIdModulo == null || m.Id != ignorarIdModulo));
    }

    public async Task AtualizarAsync(Modulo modulo)
    {
        _context.Modulos.Update(modulo);
        await SalvarTratandoNomeDuplicadoAsync();
    }

    public async Task RemoverAsync(Modulo modulo)
    {
        _context.Modulos.Remove(modulo);
        await _context.SaveChangesAsync();
    }

    // O índice único (IdMateria, Nome) é a garantia final; a checagem prévia pode perder uma corrida entre duas requisições.
    private async Task SalvarTratandoNomeDuplicadoAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new InvalidOperationException("Já existe um módulo com esse nome nesta matéria.", ex);
        }
    }
}
