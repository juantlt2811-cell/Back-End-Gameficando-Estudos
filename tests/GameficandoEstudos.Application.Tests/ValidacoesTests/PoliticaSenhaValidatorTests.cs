using FluentAssertions;
using GameficandoEstudos.Application.Validacoes;
using Xunit;

namespace GameficandoEstudos.Application.Tests.ValidacoesTests
{
    public class PoliticaSenhaValidatorTests
    {
        private readonly PoliticaSenhaValidator _validator = new();

        [Theory]
        [InlineData("SenhaAbc123")]
        [InlineData("Senha@2024")]               
        [InlineData("Minha Senha Forte 2024")]   
        [InlineData("Ação-Café#9z")]             
        public void EhValida_QuandoAtendeAPolitica_DeveRetornarTrue(string senha)
        {
            _validator.EhValida(senha).Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData("        ")]
        [InlineData("Ab1")]            
        [InlineData("senhaabc123")]    
        [InlineData("SENHAABC123")]    
        [InlineData("SenhaAbcdef")]    
        [InlineData("12345678!@#$")]   
        public void EhValida_QuandoNaoAtendeAPolitica_DeveRetornarFalse(string senha)
        {
            _validator.EhValida(senha).Should().BeFalse();
        }

        [Fact]
        public void EhValida_QuandoSenhaTemExatamente128Caracteres_DeveRetornarTrue()
        {
            var senha = "Aa1" + new string('x', 125);
            senha.Length.Should().Be(128);

            _validator.EhValida(senha).Should().BeTrue();
        }

        [Fact]
        public void EhValida_QuandoSenhaPassaDe128Caracteres_DeveRetornarFalse()
        {
            var senha = "Aa1" + new string('x', 126);
            senha.Length.Should().Be(129);

            _validator.EhValida(senha).Should().BeFalse();
        }
    }
}