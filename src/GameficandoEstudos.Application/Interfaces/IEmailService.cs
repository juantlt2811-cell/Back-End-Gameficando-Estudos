using System.Threading.Tasks;

namespace GameficandoEstudos.Application.Interfaces
{
    public interface IEmailService
    {
        Task EnviarEmailRedefinicaoSenhaAsync(string destinatario, string nomeUsuario, string token);
    }
}