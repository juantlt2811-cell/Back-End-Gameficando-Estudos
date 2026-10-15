using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Infrastructure.Data;

namespace GameficandoEstudos.Infrastructure.Services;

public class AuditoriaService : IAuditoriaService
{
    private readonly AppDbContext _context;

    public AuditoriaService(AppDbContext context)
    {
        _context = context;
    }

    public async Task RegistrarAsync(int idUsuario, string acao, string entidade, string idEntidade, string? detalhes = null)
    {
        var log = new LogAuditoria(idUsuario, acao, entidade, idEntidade, detalhes);
        await _context.LogsAuditoria.AddAsync(log);
        await _context.SaveChangesAsync();
    }
}