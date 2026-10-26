using FluentAssertions;
using GameficandoEstudos.Application.DTOs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Xunit;

namespace GameficandoEstudos.Application.Tests.DtosTests
{
    public class LimitesDeEntradaTests
    {
        private static readonly Type[] DtosDeEntrada =
        {
            typeof(RegistrarUsuarioDto), typeof(LoginDto), typeof(AlterarSenhaDto), typeof(RedefinirSenhaDto),
            typeof(SolicitarRedefinicaoSenhaDto), typeof(AnonimizarContaDto), typeof(RefreshTokenDto),
            typeof(LogoutDto), typeof(EnviarRespostaDto), typeof(GerarAtividadeDto), typeof(AtualizarAtividadeDto),
            typeof(AdicionarMaterialDto), typeof(CriarMateriaDto), typeof(AtualizarMateriaDto)
        };

        private static IEnumerable<(Type Dto, ParameterInfo Parametro)> ParametrosDeTexto() =>
            DtosDeEntrada.SelectMany(t => t.GetConstructors().Single().GetParameters()
                .Where(p => p.ParameterType == typeof(string))
                .Select(p => (t, p)));

        [Fact]
        public void TodoCampoDeTextoDeEntradaDeveTerTamanhoMaximo()
        {
            var semLimite = ParametrosDeTexto()
                .Where(x => x.Parametro.GetCustomAttribute<MaxLengthAttribute>() is null
                         && x.Parametro.GetCustomAttribute<StringLengthAttribute>() is null)
                .Select(x => $"{x.Dto.Name}.{x.Parametro.Name}")
                .ToList();

            semLimite.Should().BeEmpty("campos de texto sem limite deixam a API aceitar corpos enormes");
        }

        [Fact]
        public void CamposDeSenhaNaoPodemPassarDe128Caracteres()
        {
            var acima = ParametrosDeTexto()
                .Where(x => x.Parametro.Name is "Senha" or "NovaSenha" or "SenhaAtual")
                .Where(x => (x.Parametro.GetCustomAttribute<MaxLengthAttribute>()?.Length ?? int.MaxValue) > 128)
                .Select(x => $"{x.Dto.Name}.{x.Parametro.Name}")
                .ToList();

            acima.Should().BeEmpty("o Argon2 consome CPU e memória proporcionais ao tamanho da senha");
        }

        [Fact]
        public void RespostaDoAlunoNaoPodePassarDe8000Caracteres()
        {
            var max = typeof(EnviarRespostaDto).GetConstructors().Single().GetParameters()
                .Single(p => p.Name == "Resposta")
                .GetCustomAttribute<MaxLengthAttribute>()?.Length;

            max.Should().NotBeNull();
            max!.Value.Should().BeLessThanOrEqualTo(8_000);
        }
    }
}