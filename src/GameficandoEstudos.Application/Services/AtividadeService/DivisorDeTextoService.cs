using GameficandoEstudos.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GameficandoEstudos.Application.Services.AtividadeService
{
    
    public class DivisorDeTextoService : IDivisorDeTextoService
    {
        private static readonly Regex LinhaEmBranco =
            new(@"\r?\n[ \t]*\r?\n", RegexOptions.Compiled);

        private static readonly Regex FimDeFrase =
            new(@"(?<=[.!?…])\s+", RegexOptions.Compiled);

        private readonly DivisorDeTextoOptions _opcoes;

        public DivisorDeTextoService(DivisorDeTextoOptions opcoes)
        {
            ArgumentNullException.ThrowIfNull(opcoes);

            if (opcoes.TamanhoAlvo <= 0)
                throw new ArgumentException("TamanhoAlvo deve ser maior que zero.", nameof(opcoes));

            if (opcoes.TamanhoMaximo < opcoes.TamanhoAlvo)
                throw new ArgumentException("TamanhoMaximo não pode ser menor que TamanhoAlvo.", nameof(opcoes));

            if (opcoes.TamanhoMinimoDoUltimoTrecho < 0 ||
                opcoes.TamanhoMinimoDoUltimoTrecho > opcoes.TamanhoAlvo)
                throw new ArgumentException(
                    "TamanhoMinimoDoUltimoTrecho deve estar entre zero e TamanhoAlvo.", nameof(opcoes));

            _opcoes = opcoes;
        }

        public IReadOnlyList<string> Dividir(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return Array.Empty<string>();

            var blocos = LinhaEmBranco
                .Split(texto)
                .Select(bloco => bloco.Trim())
                .Where(bloco => bloco.Length > 0);

            var pedacos = blocos.SelectMany(QuebrarSeExcederOMaximo);

            return Agrupar(pedacos);
        }

        private IEnumerable<string> QuebrarSeExcederOMaximo(string bloco)
        {
            if (bloco.Length <= _opcoes.TamanhoMaximo)
            {
                yield return bloco;
                yield break;
            }

            var atual = new StringBuilder();

            foreach (var frase in FimDeFrase.Split(bloco))
            {
                foreach (var parte in QuebrarNoBruto(frase.Trim()))
                {
                    if (atual.Length > 0 && atual.Length + 1 + parte.Length > _opcoes.TamanhoAlvo)
                    {
                        yield return atual.ToString();
                        atual.Clear();
                    }

                    if (atual.Length > 0)
                        atual.Append(' ');

                    atual.Append(parte);
                }
            }

            if (atual.Length > 0)
                yield return atual.ToString();
        }

        private IEnumerable<string> QuebrarNoBruto(string frase)
        {
            if (frase.Length == 0)
                yield break;

            if (frase.Length <= _opcoes.TamanhoMaximo)
            {
                yield return frase;
                yield break;
            }

            for (var inicio = 0; inicio < frase.Length; inicio += _opcoes.TamanhoAlvo)
            {
                var comprimento = Math.Min(_opcoes.TamanhoAlvo, frase.Length - inicio);
                yield return frase.Substring(inicio, comprimento);
            }
        }

        private List<string> Agrupar(IEnumerable<string> pedacos)
        {
            var trechos = new List<string>();
            var atual = new StringBuilder();

            foreach (var pedaco in pedacos)
            {
                var estouraOTeto = atual.Length > 0
                                   && atual.Length + 2 + pedaco.Length > _opcoes.TamanhoMaximo;

                if (estouraOTeto)
                {
                    trechos.Add(atual.ToString());
                    atual.Clear();
                }

                if (atual.Length > 0)
                    atual.Append("\n\n");

                atual.Append(pedaco);

                if (atual.Length >= _opcoes.TamanhoAlvo)
                {
                    trechos.Add(atual.ToString());
                    atual.Clear();
                }
            }

            if (atual.Length == 0)
                return trechos;

            var sobraEhCurta = trechos.Count > 0
                               && atual.Length < _opcoes.TamanhoMinimoDoUltimoTrecho
                               && trechos[^1].Length + 2 + atual.Length <= _opcoes.TamanhoMaximo;

            if (sobraEhCurta)
                trechos[^1] = $"{trechos[^1]}\n\n{atual}";
            else
                trechos.Add(atual.ToString());

            return trechos;
        }
    }
}