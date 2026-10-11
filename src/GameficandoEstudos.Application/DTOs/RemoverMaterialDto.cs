using System;
using System.Collections.Generic;
using System.Text;

namespace GameficandoEstudos.Application.DTOs
{
    public record RemoverMaterialDto(
        int IdMateria,
        Guid DocumentoId
    );
}
