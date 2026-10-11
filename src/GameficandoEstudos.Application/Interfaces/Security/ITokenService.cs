using GameficandoEstudos.Domain.Entities;

namespace GameficandoEstudos.Application.Interfaces
{
    public interface ITokenService
    {
        string GerarToken(Usuario usuario);
    }
}