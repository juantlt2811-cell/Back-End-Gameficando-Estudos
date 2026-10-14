using System.Collections.Generic;

namespace GameficandoEstudos.Application.DTOs
{
    public record SessaoPraticaIniciadaDto(int IdSessao, List<QuestaoSessaoDto> Questoes);
}