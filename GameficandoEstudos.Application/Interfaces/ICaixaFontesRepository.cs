using GameficandoEstudos.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameficandoEstudos.Application.Interfaces
{
    public interface ICaixaFontesRepository
    {
        Task<CaixaFontes> AdicionarAsync(CaixaFontes caixaFontes);
        Task<CaixaFontes?> ObterPorIdAsync(int id);
        Task<CaixaFontes> ObterPorMateriaAsync(int idMateria);
        Task<CaixaFontes> AtualizarAsync(CaixaFontes caixaFontes);
    }
}
