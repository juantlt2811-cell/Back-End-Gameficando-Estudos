using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameficandoEstudos.Infrastructure.Repositories;

public class SessaoPraticaRepository : ISessaoPraticaRepository
{
    private readonly AppDbContext _context;

    public SessaoPraticaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SessaoPratica> AdicionarAsync(SessaoPratica sessao)
    {
        await _context.SessoesPratica.AddAsync(sessao);
        await _context.SaveChangesAsync();
        return sessao;
    }

    public async Task<SessaoPratica?> ObterPorIdComTentativasAsync(int id)
    {
        return await _context.SessoesPratica
            .Include(s => s.Tentativas)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task AtualizarAsync(SessaoPratica sessao)
    {
        _context.SessoesPratica.Update(sessao);
        await _context.SaveChangesAsync();
    }
    public async Task<List<SessaoPratica>> ListarPorUsuarioAsync(int idUsuario)
    {
        return await _context.SessoesPratica
            .Include(s => s.Tentativas)
            .Where(s => s.IdUsuario == idUsuario)
            .ToListAsync();
    }
}