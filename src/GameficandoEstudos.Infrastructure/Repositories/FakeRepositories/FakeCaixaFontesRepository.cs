using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameficandoEstudos.Infrastructure.Repositories.FakeRepositories
{
    public class FakeCaixaFontesRepository : ICaixaFontesRepository
    {
        private readonly List<CaixaFontes> _caixas = new();
        private int _contadorId = 1;

        public Task<CaixaFontes> AdicionarAsync(CaixaFontes caixa)
        {
            typeof(CaixaFontes).GetProperty("Id")?.SetValue(caixa, _contadorId++);
            _caixas.Add(caixa);
            return Task.FromResult(caixa);
        }

        public Task<CaixaFontes?> ObterPorIdAsync(int id)
        {
            var caixa = _caixas.FirstOrDefault(c => c.Id == id);
            return Task.FromResult(caixa);
        }

        public Task<CaixaFontes> ObterPorMateriaAsync(int idMateria)
        {
            var caixa = _caixas.FirstOrDefault(c => c.IdMateria == idMateria);
            return Task.FromResult(caixa);
        }

        public Task<CaixaFontes> AtualizarAsync(CaixaFontes caixa)
        {
            var index = _caixas.FindIndex(c => c.Id == caixa.Id);
            if (index != -1)
            {
                _caixas[index] = caixa;
            }
            return Task.FromResult(caixa);
        }

        public Task RemoverChunksPorDocumentoIdAsync(Guid documentoId)
        {
            foreach (var caixa in _caixas)
            {
                caixa.RemoverChunksPorDocumento(documentoId);
            }
            return Task.CompletedTask;
        }

        public Task<IEnumerable<ResumoOrigem>> PesquisarResumoMateriaisAsync(int idMateria, string? termoBusca)
        {
            var caixa = _caixas.FirstOrDefault(c => c.IdMateria == idMateria);
            if (caixa == null)
                return Task.FromResult(Enumerable.Empty<ResumoOrigem>());

            var query = caixa.Chunks.AsQueryable();

            if (!string.IsNullOrWhiteSpace(termoBusca))
            {
                query = query.Where(c => c.Origem.Contains(termoBusca, StringComparison.OrdinalIgnoreCase));
            }

            var resultado = query.GroupBy(c => new { c.DocumentoId, c.Origem })
                                 .Select(g => new ResumoOrigem(g.Key.DocumentoId, g.Key.Origem, g.Count(), g.Max(c => c.AtualizadoEm)))
                                 .AsEnumerable();

            return Task.FromResult(resultado);
        }
    }
}