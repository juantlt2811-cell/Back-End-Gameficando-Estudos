namespace GameficandoEstudos.Application.DTOs
{
    public record ConfirmacaoInicioSessaoDto(
        bool PodeIniciar, string? Motivo, int TentativasRestantesHoje, int TotalQuestoesDisponiveis);
}