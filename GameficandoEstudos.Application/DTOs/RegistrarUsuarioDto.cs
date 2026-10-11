namespace GameficandoEstudos.Application.DTOs
{
    public record RegistrarUsuarioDto(string Nome, string Email, string Senha, GameficandoEstudos.Domain.Enums.TipoUsuario TipoUsuario);
}

