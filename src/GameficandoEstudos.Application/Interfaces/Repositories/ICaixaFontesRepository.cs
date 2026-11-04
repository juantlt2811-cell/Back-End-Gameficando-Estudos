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

        /// <summary>
        /// Move todos os trechos de um documento da matéria para outro módulo (ou para nenhum, com <c>null</c>).
        /// Devolve quantos trechos foram atualizados — 0 significa que o documento não existe nessa matéria.
        /// </summary>
        Task<int> MoverDocumentoDeModuloAsync(int idMateria, Guid documentoId, int? idModulo);
        Task<IEnumerable<ResumoOrigem>> PesquisarResumoMateriaisAsync(int idMateria, string? termoBusca);
    }
}
