namespace GameficandoEstudos.Application.Interfaces.Security
{
    public interface ILoginAttemptGuard
    {
        Task<bool> EstaBloqueadoAsync(string email);
        Task RegistrarFalhaAsync(string email);
        Task RegistrarSucessoAsync(string email);
    }
}
