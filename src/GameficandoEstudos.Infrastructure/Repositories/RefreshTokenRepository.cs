using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameficandoEstudos.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AdicionarAsync(RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task<RefreshToken?> ObterPorHashAsync(string tokenHash)
    {
        return await _context.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == tokenHash);
    }

    public async Task AtualizarAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();
    }
    public async Task RevogarTodosPorUsuarioAsync(int idUsuario)
    {
        var tokensAtivos = await _context.RefreshTokens
            .Where(r => r.IdUsuario == idUsuario && r.RevogadoEm == null)
            .ToListAsync();

        foreach (var token in tokensAtivos)
            token.Revogar();

        await _context.SaveChangesAsync();
    }
}