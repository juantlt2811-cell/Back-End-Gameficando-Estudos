using System.ComponentModel.DataAnnotations;

namespace GameficandoEstudos.Application.DTOs
{
    public record LoginDto(
        [param: Required(ErrorMessage = "O e-mail é obrigatório.")]
        [param: EmailAddress(ErrorMessage = "O e-mail informado não é válido.")]
        [param: MaxLength(150, ErrorMessage = "O e-mail informado não é válido.")]
        string Email,

        [param: Required(ErrorMessage = "A senha é obrigatória.")]
        [param: MaxLength(128, ErrorMessage = "A senha deve ter no máximo 128 caracteres.")]
        string Senha);
}