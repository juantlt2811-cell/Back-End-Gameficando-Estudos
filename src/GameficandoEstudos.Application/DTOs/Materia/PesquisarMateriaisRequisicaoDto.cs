using System;
using System.Collections.Generic;
using System.Text;

namespace GameficandoEstudos.Application.DTOs
{
    public record PesquisarMateriaisRequisicaoDto(
        int IdMateria,
        int IdProfessorLogado,
        string? TermoPesquisa
    );
}
