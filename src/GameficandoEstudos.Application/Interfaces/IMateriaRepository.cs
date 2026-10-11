using GameficandoEstudos.Domain.Entities;

namespace GameficandoEstudos.Application.Interfaces
{
    public interface IMateriaRepository
    {
        Task<Materia> AdicionarAsync(Materia materia);
        Task<Materia?> ObterPorIdAsync(int id);
        Task<IEnumerable<Materia>> ObterPorProfessorAsync(int idProfessor);
        Task<IEnumerable<Materia>> ObterTodasAsync();
        Task<IEnumerable<Materia>> PesquisarAsync(string? nomeMateria, int? idProfessor);
        Task<Materia> AtualizarAsync(Materia materia);
        Task DeletarAsync(int id);
    }
}
