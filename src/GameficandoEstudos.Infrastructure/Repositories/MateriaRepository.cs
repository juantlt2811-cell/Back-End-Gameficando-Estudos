using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameficandoEstudos.Infrastructure.Repositories;

public class MateriaRepository : IMateriaRepository
{
    private readonly AppDbContext _context;

    public MateriaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Materia> AdicionarAsync(Materia materia)
    {
        await _context.Materias.AddAsync(materia);
        await _context.SaveChangesAsync();
        return materia;
    }

    public async Task<Materia?> ObterPorIdAsync(int id)
    {
        return await _context.Materias
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Materia>> ObterPorProfessorAsync(int idProfessor)
    {
        return await _context.Materias
            .Where(m => m.IdProfessor == idProfessor)
            .ToListAsync();
    }

    public async Task<IEnumerable<Materia>> ObterTodasAsync()
    {
        return await _context.Materias.ToListAsync();
    }

    public async Task<IEnumerable<Materia>> PesquisarAsync(string? nomeMateria, int? idProfessor)
    {
        var query = _context.Materias.AsQueryable();

        if (!string.IsNullOrWhiteSpace(nomeMateria))
        {
            query = query.Where(m => m.Nome.Contains(nomeMateria));
        }

        if (idProfessor.HasValue)
        {
            query = query.Where(m => m.IdProfessor == idProfessor.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<Materia> AtualizarAsync(Materia materia)
    {
        _context.Materias.Update(materia);
        await _context.SaveChangesAsync();
        return materia;
    }

    public async Task DeletarAsync(int id)
    {
        var materia = await _context.Materias.FindAsync(id);
        if (materia != null)
        {
            _context.Materias.Remove(materia);
            await _context.SaveChangesAsync();
        }
    }
}