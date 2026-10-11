using System;
using System.Collections.Generic;
using System.Text;

namespace GameficandoEstudos.Application.DTOs
{
    public record AtualizarMateriaDto(int Id, string Nome, int CargaHoraria, string Dificuldade);
}
