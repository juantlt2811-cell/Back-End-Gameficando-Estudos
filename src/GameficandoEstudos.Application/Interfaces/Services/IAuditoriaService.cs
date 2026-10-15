namespace GameficandoEstudos.Application.Interfaces
{
    public interface IAuditoriaService
    {
        Task RegistrarAsync(int idUsuario, string acao, string entidade, string idEntidade, string? detalhes = null);
    }
}