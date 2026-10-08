namespace GameficandoEstudos.Application.Interfaces
{
    public interface ILlmService
    {
        Task<string> GerarRespostaAsync(string prompt);
    }
}
