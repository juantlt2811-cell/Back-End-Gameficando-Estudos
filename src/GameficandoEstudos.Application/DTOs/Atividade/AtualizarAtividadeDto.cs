using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GameficandoEstudos.Application.DTOs
{
    public record AtualizarAtividadeDto(
        [param: Range(1, int.MaxValue, ErrorMessage = "Id da atividade inválido.")]
        int Id,

        [param: Required(ErrorMessage = "O nome da atividade é obrigatório.")]
        string Nome,

        [param: Required(ErrorMessage = "O conteúdo da atividade é obrigatório.")]
        string Conteudo,

        [param: Required(ErrorMessage = "O gabarito da atividade é obrigatório.")]
        string Gabarito,

        // Opcional: só é usada/validada para atividades de múltipla escolha (ver Atividade.Atualizar).
        List<string>? Alternativas = null);
}
