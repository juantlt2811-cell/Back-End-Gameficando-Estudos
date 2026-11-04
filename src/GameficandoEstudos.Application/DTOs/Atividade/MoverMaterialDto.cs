using System;
using System.ComponentModel.DataAnnotations;

namespace GameficandoEstudos.Application.DTOs
{
    /// <summary>
    /// Move um material (documento inteiro da Caixa de Fontes) para outro módulo da mesma matéria.
    /// IdModulo = null devolve o material para a matéria geral, sem módulo.
    /// </summary>
    public record MoverMaterialDto(
        [param: Range(1, int.MaxValue, ErrorMessage = "Id da matéria inválido.")]
        int IdMateria,

        Guid DocumentoId,

        [param: Range(1, int.MaxValue, ErrorMessage = "Id do módulo inválido.")]
        int? IdModulo);
}
