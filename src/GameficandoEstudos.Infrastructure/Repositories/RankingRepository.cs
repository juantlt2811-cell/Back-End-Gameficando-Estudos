using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Enums;
using GameficandoEstudos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GameficandoEstudos.Infrastructure.Repositories;

public class RankingRepository : IRankingRepository
{
    private readonly AppDbContext _context;

    public RankingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RankingOfensivaDto>> ObterRankingOfensivaAsync(int limite)
    {
        var alunos = await _context.Usuarios
            .Where(u => u.Tipo == TipoUsuario.Aluno)
            .OrderByDescending(u => u.Gamificacao!.OfensivaAtual)
            .ThenByDescending(u => u.Gamificacao!.MaiorOfensiva)
            .Take(limite)
            .Select(u => new { u.Nome, u.Gamificacao!.OfensivaAtual, u.Gamificacao.MaiorOfensiva })
            .ToListAsync();

        return alunos
            .Select((a, indice) => new RankingOfensivaDto(indice + 1, a.Nome, a.OfensivaAtual, a.MaiorOfensiva))
            .ToList();
    }

    public async Task<List<RankingMateriaDto>> ObterRankingPorMateriaAsync(int idMateria, int limite, int minimoTentativas)
    {
        var agregados = await _context.TentativasResposta
            .Where(t => t.IdMateria == idMateria)
            .GroupBy(t => t.IdUsuario)
            .Select(g => new { IdUsuario = g.Key, Total = g.Count(), Acertos = g.Count(t => t.Acertou) })
            .Where(x => x.Total >= minimoTentativas)
            .ToListAsync();

        var idsUsuarios = agregados.Select(a => a.IdUsuario).ToList();
        var nomes = await _context.Usuarios
            .Where(u => idsUsuarios.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Nome);

        return agregados
            .Select(a => new
            {
                Nome = nomes.GetValueOrDefault(a.IdUsuario, "Usuário removido"),
                a.Total,
                a.Acertos,
                Percentual = (double)a.Acertos / a.Total * 100
            })
            .OrderByDescending(a => a.Percentual)
            .ThenByDescending(a => a.Total)
            .Take(limite)
            .Select((a, indice) => new RankingMateriaDto(indice + 1, a.Nome, a.Total, a.Acertos, a.Percentual))
            .ToList();
    }

    public async Task<List<EstatisticaAlunoMateriaDto>> ObterEstatisticasPorMateriaAsync(int idMateria)
    {
        var tentativasPorAluno = await _context.TentativasResposta
            .Where(t => t.IdMateria == idMateria)
            .GroupBy(t => t.IdUsuario)
            .Select(g => new { IdUsuario = g.Key, Total = g.Count(), Acertos = g.Count(t => t.Acertou) })
            .ToListAsync();

        var sessoesPorAluno = await _context.SessoesPratica
            .Where(s => s.IdMateria == idMateria && s.Status == StatusSessaoPratica.Concluida)
            .GroupBy(s => s.IdUsuario)
            .Select(g => new { IdUsuario = g.Key, Sessoes = g.Count() })
            .ToListAsync();

        var idsUsuarios = tentativasPorAluno.Select(t => t.IdUsuario).ToList();
        var nomes = await _context.Usuarios
            .Where(u => idsUsuarios.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Nome);

        return tentativasPorAluno
            .Select(t => new EstatisticaAlunoMateriaDto(
                nomes.GetValueOrDefault(t.IdUsuario, "Usuário removido"),
                sessoesPorAluno.FirstOrDefault(s => s.IdUsuario == t.IdUsuario)?.Sessoes ?? 0,
                t.Total,
                t.Total == 0 ? 0 : (double)t.Acertos / t.Total * 100))
            .OrderByDescending(e => e.SessoesConcluidas)
            .ToList();
    }
}