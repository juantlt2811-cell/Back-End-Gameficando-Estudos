using System.ComponentModel.DataAnnotations;

namespace GameficandoEstudos.Application.DTOs
{
    public record LogoutDto(
        [param: Required(ErrorMessage = "O refresh token é obrigatório.")]
        [param: MaxLength(200, ErrorMessage = "Refresh token inválido.")]
        string RefreshToken);
}