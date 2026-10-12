using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameficandoEstudos.Infrastructure.Repositories.FakeRepositories
{
    public class FakeMateriaRepository : IMateriaRepository
    {
        private readonly List<Materia> _materias = new();
        private int _contadorId = 1;

        public Task<Materia> AdicionarAsync(Materia materia)
        {
            typeof(Materia).GetProperty("Id")?.SetValue(materia, _contadorId++);

            _materias.Add(materia);
            return Task.FromResult(materia);
        }

        public Task<Materia?> ObterPorIdAsync(int id)
        {
            var materia = _materias.FirstOrDefault(m => m.Id == id);
            return Task.FromResult(materia);
        }

        public Task<IEnumerable<Materia>> ObterPorProfessorAsync(int idProfessor)
        {
            var materias = _materias.Where(m => m.IdProfessor == idProfessor).AsEnumerable();
            return Task.FromResult(materias);
        }

        public Task<IEnumerable<Materia>> ObterTodasAsync()
        {
            return Task.FromResult(_materias.AsEnumerable());
        }

        public Task<IEnumerable<Materia>> PesquisarAsync(string? termo, int? idProfessor)
        {
            var query = _materias.AsQueryable();

            if (!string.IsNullOrWhiteSpace(termo))
            {
                query = query.Where(m => m.Nome.ToLower().Contains(termo.ToLower()));
            }

            if (idProfessor.HasValue)
            {
                query = query.Where(m => m.IdProfessor == idProfessor.Value);
            }

            return Task.FromResult(query.AsEnumerable());
        }

        public Task<Materia> AtualizarAsync(Materia materia)
        {
            var index = _materias.FindIndex(m => m.Id == materia.Id);
            if (index != -1)
            {
                _materias[index] = materia;
            }
            return Task.FromResult(materia);
        }

        public Task DeletarAsync(int id)
        {
            var materia = _materias.FirstOrDefault(m => m.Id == id);
            if (materia != null)
            {
                _materias.Remove(materia);
            }
            return Task.CompletedTask;
        }
    }
}