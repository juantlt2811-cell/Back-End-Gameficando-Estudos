using System;
using System.Collections.Generic;
using System.Text;

namespace GameficandoEstudos.Application.DTOs
{
    public record AtividadeDto(
        int Id,
        string Nome,
        string Tipo,
        string Conteudo,
        List<string> Alternativas,
        int IdMateria,
        bool Ativa
    );
}
