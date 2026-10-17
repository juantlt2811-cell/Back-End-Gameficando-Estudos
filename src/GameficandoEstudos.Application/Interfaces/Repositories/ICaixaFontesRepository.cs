using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.ValueObjects;
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
        Task AdicionarChunksAsync(IEnumerable<FonteChunks> chunks);
        Task RemoverChunksPorDocumentoIdAsync(int idMateria, Guid documentoId);
        Task<IEnumerable<ResumoOrigem>> PesquisarResumoMateriaisAsync(int idMateria, string? termoBusca);
    }
}
