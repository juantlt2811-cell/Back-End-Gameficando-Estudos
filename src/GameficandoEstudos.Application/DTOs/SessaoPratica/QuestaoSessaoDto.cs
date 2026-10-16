using System.Collections.Generic;
using GameficandoEstudos.Domain.Enums;

namespace GameficandoEstudos.Application.DTOs
{
    public record QuestaoSessaoDto(int Id, string Nome, TipoAtividade Tipo, string Conteudo, List<string> Alternativas);
}
