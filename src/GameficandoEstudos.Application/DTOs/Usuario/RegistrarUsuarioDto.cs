using System.ComponentModel.DataAnnotations;

namespace GameficandoEstudos.Application.DTOs
{
    public record RegistrarUsuarioDto(
        [param: Required(ErrorMessage = "O nome é obrigatório.")]
        [param: MaxLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        string Nome,
        [param: Required(ErrorMessage = "O e-mail é obrigatório.")]
        [param: EmailAddress(ErrorMessage = "O e-mail informado não é válido.")]
        [param: MaxLength(150, ErrorMessage = "O e-mail deve ter no máximo 150 caracteres.")]
        string Email,
        [param: Required(ErrorMessage = "A senha é obrigatória.")]
        [param: MinLength(8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres.")]
        [param: MaxLength(128, ErrorMessage = "A senha deve ter no máximo 128 caracteres.")]
        string Senha,
        [param: MaxLength(200, ErrorMessage = "Código de professor inválido.")]
        string? CodigoProfessor = null);
}