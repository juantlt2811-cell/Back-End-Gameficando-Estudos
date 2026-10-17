using System;
using System.Collections.Generic;
using System.Text;

namespace GameficandoEstudos.Application.Interfaces
{
    public interface IDivisorDeTextoService
    {
        IReadOnlyList<string> Dividir(string? texto);
    }
}
