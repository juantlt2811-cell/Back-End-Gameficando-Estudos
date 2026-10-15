namespace GameficandoEstudos.Application.Interfaces
{
    public interface IExtratorDeTextoService
    {
        Task<string> ExtrairTextoAsync(Stream conteudoArquivo, string nomeArquivo);
    }
}