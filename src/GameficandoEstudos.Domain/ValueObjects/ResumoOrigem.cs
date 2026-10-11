using System;
using System.Collections.Generic;
using System.Text;

namespace GameficandoEstudos.Domain.ValueObjects
{
    public record ResumoOrigem(Guid DocumentoId, string Titulo, int QuantidadeChunks, DateTime UltimaAtualizacao);
}

