using System;
using System.Collections.Generic;
using System.Text;

namespace GameficandoEstudos.Application.Services.AtividadeService
{
    public sealed class DivisorDeTextoOptions
    {
        public int TamanhoAlvo { get; init; } = 800;
        public int TamanhoMaximo { get; init; } = 1500;
        public int TamanhoMinimoDoUltimoTrecho { get; init; } = 120;
    }
}
