using System;
using System.Collections.Generic;
using System.Text;

namespace GameficandoEstudos.Application.DTOs
{
    public record AdicionarMaterialDto(
        int IdMateria,
        string TituloMaterial,
        string TextoCompleto
    );
}
