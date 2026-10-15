using System;
using System.Collections.Generic;
using System.Text;

namespace GameficandoEstudos.Application.DTOs
{
    public record MaterialBaseConhecimentoDto(
        Guid DocumentoId,
        string TituloMaterial,
        int QuantidadeChunks,
        DateTime UltimaAtualizacao
    );
}
