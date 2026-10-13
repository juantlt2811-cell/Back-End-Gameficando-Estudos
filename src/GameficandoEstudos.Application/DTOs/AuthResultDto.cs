using GameficandoEstudos.Domain.Enums;

namespace GameficandoEstudos.Application.DTOs
{
    public record AuthResultDto(int Id, string Nome, string Token, TipoUsuario TipoUsuario);
}
