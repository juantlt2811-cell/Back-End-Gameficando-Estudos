using System;

namespace GameficandoEstudos.Application.DTOs
{
    public record MateriaDto(
        int Id,
        string Nome,
        int CargaHoraria,
        string Dificuldade,
        int IdProfessor,
        DateTime CriadoEm
    );
}