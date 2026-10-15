using System.Collections.Generic;

namespace GameficandoEstudos.Application.DTOs
{
    public record DadosPessoaisExportadosDto(
        int Id,
        string Nome,
        string Email,
        string Tipo,
        DateTime CriadoEm,
        GamificacaoExportadaDto? Gamificacao,
        List<MateriaExportadaDto>? MateriasCriadas,
        List<SessaoPraticaExportadaDto>? SessoesDePratica);
}