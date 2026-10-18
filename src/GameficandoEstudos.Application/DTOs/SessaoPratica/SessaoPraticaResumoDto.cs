namespace GameficandoEstudos.Application.DTOs
{
    public record SessaoPraticaResumoDto(
        int TotalRespondidas, int TotalAcertos, double PercentualAcerto, double NotaMedia, int OfensivaAtual);
}