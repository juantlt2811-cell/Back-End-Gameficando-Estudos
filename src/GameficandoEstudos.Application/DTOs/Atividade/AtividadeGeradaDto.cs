using System.Collections.Generic;

namespace GameficandoEstudos.Application.DTOs
{
    public record AtividadeGeradaDto(int Id, string Nome, string Tipo, string Conteudo, List<string> Alternativas, string Gabarito, int IdMateria);
}
