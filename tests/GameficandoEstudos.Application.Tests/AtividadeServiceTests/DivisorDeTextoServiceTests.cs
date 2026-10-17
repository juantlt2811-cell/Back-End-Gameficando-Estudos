using FluentAssertions;
using GameficandoEstudos.Application.Services.AtividadeService;
using System;
using System.Linq;
using Xunit;

namespace GameficandoEstudos.Application.Tests.AtividadeServiceTests
{
    public class DivisorDeTextoServiceTests
    {
        [Fact]
        public void Construtor_QuandoMaximoMenorQueAlvo_DeveLancar()
        {
            var opcoesInvalidas = new DivisorDeTextoOptions { TamanhoAlvo = 800, TamanhoMaximo = 400 };

            Action act = () => new DivisorDeTextoService(opcoesInvalidas);

            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Construtor_QuandoAlvoNaoPositivo_DeveLancar()
        {
            var opcoesInvalidas = new DivisorDeTextoOptions { TamanhoAlvo = 0 };

            Action act = () => new DivisorDeTextoService(opcoesInvalidas);

            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\n\n\t\n")]
        public void Dividir_SemConteudo_DeveRetornarVazio(string? texto)
        {
            var divisor = new DivisorDeTextoService(new DivisorDeTextoOptions());

            var resultado = divisor.Dividir(texto);

            resultado.Should().BeEmpty();
        }

        [Fact]
        public void Dividir_TextoCurto_DeveRetornarUmUnicoTrecho()
        {
            var divisor = new DivisorDeTextoService(new DivisorDeTextoOptions());

            var resultado = divisor.Dividir("Uma anotação curta do professor.");

            resultado.Should().ContainSingle()
                .Which.Should().Be("Uma anotação curta do professor.");
        }

        [Fact]
        public void Dividir_LinhasSeparadasPorQuebraSimples_NaoDeveCriarUmTrechoPorLinha()
        {
            var divisor = new DivisorDeTextoService(new DivisorDeTextoOptions());

            var texto = "# Título do documento\n"
                      + "Primeira linha com algum conteúdo.\n"
                      + "Segunda linha com algum conteúdo.\n"
                      + "Terceira linha com algum conteúdo.";

            var resultado = divisor.Dividir(texto);

            resultado.Should().ContainSingle();
            resultado[0].Should().Contain("Título do documento").And.Contain("Terceira linha");
        }

        [Fact]
        public void Dividir_ParagrafosCurtos_DeveAgruparAteOAlvo()
        {
            var divisor = new DivisorDeTextoService(new DivisorDeTextoOptions());

            var texto = string.Join("\n\n", Enumerable.Range(1, 4).Select(i => $"Parágrafo {i}."));

            var resultado = divisor.Dividir(texto);

            resultado.Should().ContainSingle();
            resultado[0].Should().Contain("Parágrafo 1.").And.Contain("Parágrafo 4.");
        }

        [Fact]
        public void Dividir_QuandoAcumuladoAtingeOAlvo_DeveFecharOTrecho()
        {
            var opcoes = new DivisorDeTextoOptions();
            var divisor = new DivisorDeTextoService(opcoes);

            var texto = string.Join("\n\n",
                Enumerable.Range(1, 6).Select(i => $"Bloco {i}. " + new string('x', 205)));

            var resultado = divisor.Dividir(texto);

            resultado.Should().HaveCountGreaterThan(1);
            resultado.Should().OnlyContain(t => t.Length <= opcoes.TamanhoMaximo);
        }

        [Fact]
        public void Dividir_NaoDeveDeixarUmTrechoFinalMuitoCurtoSolto()
        {
            var divisor = new DivisorDeTextoService(new DivisorDeTextoOptions());

            var texto = string.Join("\n\n",
                Enumerable.Range(1, 4).Select(i => $"Bloco {i}. " + new string('x', 240))) + "\n\nFim.";

            var resultado = divisor.Dividir(texto);

            resultado.Last().Should().EndWith("Fim.");
            resultado.Last().Length.Should().BeGreaterThan(10);
        }

        [Fact]
        public void Dividir_BlocoAcimaDoTeto_DeveQuebrarEmLimiteDeFrase()
        {
            var opcoes = new DivisorDeTextoOptions();
            var divisor = new DivisorDeTextoService(opcoes);

            var frase = "Esta é uma frase completa de tamanho razoável para o teste. ";
            var texto = string.Concat(Enumerable.Repeat(frase, 40));

            var resultado = divisor.Dividir(texto);

            resultado.Should().HaveCountGreaterThan(1);
            resultado.Should().OnlyContain(t => t.Length <= opcoes.TamanhoMaximo);
            resultado.Should().OnlyContain(t => t.StartsWith("Esta é uma frase"));
        }

        [Fact]
        public void Dividir_BlobSemPontuacao_DeveRespeitarOTetoESemPerderConteudo()
        {
            var opcoes = new DivisorDeTextoOptions();
            var divisor = new DivisorDeTextoService(opcoes);

            var resultado = divisor.Dividir(new string('x', 4000));

            resultado.Should().OnlyContain(t => t.Length <= opcoes.TamanhoMaximo);
            string.Concat(resultado).Length.Should().Be(4000);
        }

        [Fact]
        public void Dividir_NaoDeveProduzirTrechosVaziosOuComEspacoNasBordas()
        {
            var divisor = new DivisorDeTextoService(new DivisorDeTextoOptions());

            var texto = "  Primeiro bloco com conteúdo.  \n\n\n   \n\n  Segundo bloco com conteúdo.  ";

            var resultado = divisor.Dividir(texto);

            resultado.Should().NotBeEmpty();
            resultado.Should().OnlyContain(t => t.Length > 0 && t == t.Trim());
        }

        [Fact]
        public void Dividir_DeveRespeitarOpcoesCustomizadas()
        {
            var opcoes = new DivisorDeTextoOptions
            {
                TamanhoAlvo = 100,
                TamanhoMaximo = 150,
                TamanhoMinimoDoUltimoTrecho = 0
            };
            var divisor = new DivisorDeTextoService(opcoes);

            var texto = string.Join("\n\n",
                Enumerable.Range(1, 8).Select(i => $"Bloco {i}. " + new string('x', 50)));

            var resultado = divisor.Dividir(texto);

            resultado.Should().HaveCountGreaterThan(2);
            resultado.Should().OnlyContain(t => t.Length <= opcoes.TamanhoMaximo);
        }
    }
}