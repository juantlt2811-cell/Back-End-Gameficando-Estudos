namespace GameficandoEstudos.Application.Interfaces.Security
{
    public interface ILoginAttemptGuard
    {
        bool EstaBloqueado(string email);
        void RegistrarFalha(string email);
        void RegistrarSucesso(string email);
    }
}