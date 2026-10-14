using GameficandoEstudos.Domain.Entities;

namespace GameficandoEstudos.Application.Interfaces
{
    public interface ISessaoPraticaRepository
    {
        Task<SessaoPratica> AdicionarAsync(SessaoPratica sessao);
        Task<SessaoPratica?> ObterPorIdComTentativasAsync(int id);
        Task AtualizarAsync(SessaoPratica sessao);
    }
}