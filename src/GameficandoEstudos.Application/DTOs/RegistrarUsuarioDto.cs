using System.ComponentModel.DataAnnotations;

namespace GameficandoEstudos.Application.DTOs
{
    public record RegistrarUsuarioDto(
        [param: Required(ErrorMessage = "O nome é obrigatório.")]
        string Nome, 
        [param: Required(ErrorMessage = "O e-mail é obrigatório.")]
        [param: EmailAddress(ErrorMessage = "O e-mail informado não é válido.")]
        string Email, 
        [param: Required(ErrorMessage = "A senha é obrigatória.")]
        [param: MinLength(8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres.")]
        [param: RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d]{8,}$",
            ErrorMessage = "A senha deve conter pelo menos uma letra maiúscula, uma letra minúscula e um número.")]
        string Senha, 
        Domain.Enums.TipoUsuario TipoUsuario);
}

