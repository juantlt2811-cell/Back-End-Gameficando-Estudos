namespace GameficandoEstudos.Application.DTOs
{
    public record SessaoPraticaExportadaDto(
        int Id, int IdMateria, string Status, int TotalAcertos, int TotalRespondidas,
        DateTime IniciadaEm, DateTime? ConcluidaEm);
}