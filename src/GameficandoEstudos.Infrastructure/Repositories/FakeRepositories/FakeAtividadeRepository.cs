using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameficandoEstudos.Infrastructure.Repositories.FakeRepositories
{
    public class FakeAtividadeRepository : IAtividadeRepository
    {
        private readonly List<Atividade> _atividades = new();
        private int _contadorId = 1;

        public Task<Atividade> AdicionarAsync(Atividade atividade)
        {
            typeof(Atividade).GetProperty("Id")?.SetValue(atividade, _contadorId++);
            _atividades.Add(atividade);
            return Task.FromResult(atividade);
        }

        public Task<Atividade?> ObterPorIdAsync(int id)
        {
            var atividade = _atividades.FirstOrDefault(a => a.Id == id);
            return Task.FromResult(atividade);
        }

        public Task<IEnumerable<Atividade>> ObterPorMateriaAsync(int idMateria)
        {
            var atividades = _atividades.Where(a => a.IdMateria == idMateria).AsEnumerable();
            return Task.FromResult(atividades);
        }

        public Task<bool> PossuiRespostasAsync(int idAtividade)
        {
            bool possui = idAtividade % 2 == 0; 
            return Task.FromResult(possui);
        }

        public Task AtualizarAsync(Atividade atividade)
        {
            var index = _atividades.FindIndex(a => a.Id == atividade.Id);
            if (index != -1) _atividades[index] = atividade;
            return Task.CompletedTask;
        }

        public Task RemoverAsync(Atividade atividade)
        {
            _atividades.Remove(atividade);
            return Task.CompletedTask;
        }
    }
}