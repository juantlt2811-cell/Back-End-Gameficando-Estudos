using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums; 
using GameficandoEstudos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameficandoEstudos.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObterPorIdAsync(int id)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Usuario?> ObterPorEmailAsync(string email)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<Usuario> AdicionarAsync(Usuario usuario)
    {
        await _context.Usuarios.AddAsync(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task<Usuario> AtualizarAsync(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }
    public async Task<IEnumerable<Usuario?>> ObterUsuariosPorTipoAsync(TipoUsuario tipoUsuario)
    {
        return await _context.Usuarios
            .Where(u => u.Tipo == tipoUsuario)
            .ToListAsync();
    }

    public async Task<bool> ExisteEmailAsync(string email)
    {
        return await _context.Usuarios
            .AnyAsync(u => u.Email == email);
    }
}