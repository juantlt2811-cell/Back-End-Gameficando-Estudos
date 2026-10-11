using FluentAssertions;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;
using GameficandoEstudos.Infrastructure.Security; 
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace GameficandoEstudos.Infrastructure.Tests
{
    public class TokenServiceTests
    {
        [Fact]
        public void GerarToken_DeveRetornarJwtValidoComClaimsCorretas()
        {
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["Jwt:ChaveSecreta"]).Returns("UmaChaveSuperSecretaMuitoLongaParaOhmacSha256_12345!");
            configMock.Setup(c => c["Jwt:Emissor"]).Returns("GameficandoEstudosAPI");
            configMock.Setup(c => c["Jwt:DuracaoEmMinutos"]).Returns("60");

            var service = new TokenService(configMock.Object);

            var usuario = new Usuario("Professor Carvalho", "carvalho@pokemon.com", "hash", TipoUsuario.Professor);
            typeof(Usuario).GetProperty("Id")?.SetValue(usuario, 42);

            var tokenString = service.GerarToken(usuario);

            tokenString.Should().NotBeNullOrEmpty();

            
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(tokenString);

            jwtToken.Issuer.Should().Be("GameficandoEstudosAPI");

            jwtToken.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value.Should().Be("42");

            jwtToken.Claims.First(c => c.Type == ClaimTypes.Name).Value.Should().Be("Professor Carvalho");

            jwtToken.Claims.First(c => c.Type == ClaimTypes.Role).Value.Should().Be("Professor");

            jwtToken.ValidTo.Should().BeAfter(DateTime.UtcNow);
        }
    }
}