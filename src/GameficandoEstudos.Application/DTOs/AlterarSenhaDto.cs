using System.ComponentModel.DataAnnotations;

namespace GameficandoEstudos.Application.DTOs
{
    public record AlterarSenhaDto(
        [param: Required(ErrorMessage = "A senha atual é obrigatória.")]
        string SenhaAtual,

        [param: Required(ErrorMessage = "A nova senha é obrigatória.")]
        [param: MinLength(8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres.")]
        string NovaSenha);
}