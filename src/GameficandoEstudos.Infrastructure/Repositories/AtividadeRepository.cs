using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameficandoEstudos.Infrastructure.Repositories;

public class AtividadeRepository : IAtividadeRepository
{
    private readonly AppDbContext _context;

    public AtividadeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Atividade> AdicionarAsync(Atividade atividade)
    {
        await _context.Atividades.AddAsync(atividade);
        await _context.SaveChangesAsync();
        return atividade;
    }

    public async Task<Atividade?> ObterPorIdAsync(int id)
    {
        return await _context.Atividades
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<IEnumerable<Atividade>> ObterPorMateriaAsync(int idMateria)
    {
        return await _context.Atividades
            .Where(a => a.IdMateria == idMateria)
            .Where(a => a.Ativa)
            .ToListAsync();
    }

    public async Task AtualizarAsync(Atividade atividade)
    {
        _context.Atividades.Update(atividade);
        await _context.SaveChangesAsync();
    }

    public async Task RemoverAsync(Atividade atividade)
    {
        _context.Atividades.Remove(atividade);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Atividade>> ObterAleatoriasPorMateriaAsync(int idMateria, int quantidade)
    {
        var ativas = await _context.Atividades
            .Where(a => a.IdMateria == idMateria && a.Ativa)
            .ToListAsync();

        // Embaralha em memoria (LINQ-to-Objects) — volume de questoes por materia e pequeno, nao justifica RANDOM() no banco.
        return ativas.OrderBy(_ => Guid.NewGuid()).Take(quantidade).ToList();
    }

    public async Task<bool> PossuiRespostasAsync(int idAtividade)
    {
        return await _context.TentativasResposta.AnyAsync(t => t.IdAtividade == idAtividade);
    }
}