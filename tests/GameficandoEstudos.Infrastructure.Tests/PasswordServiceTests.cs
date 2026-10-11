using FluentAssertions;
using GameficandoEstudos.Infrastructure.Security; 
using System;
using Xunit;

namespace GameficandoEstudos.Infrastructure.Tests
{
    public class PasswordServiceTests
    {
        [Fact]
        public void HashPassword_DeveRetornarHashNoFormatoArgon2id()
        {
            var service = new PasswordService();
            string senhaPlana = "senhaSuperSegura123";

            string hash = service.HashPassword(senhaPlana);

            hash.Should().NotBeNullOrEmpty();
            hash.Should().StartWith("$argon2id$v=19$");
        }

        [Fact]
        public void VerifyPassword_QuandoSenhaForCorreta_DeveRetornarTrue()
        {
            var service = new PasswordService();
            string senhaOriginal = "minhaSenhaSecreta";
            string hashGerado = service.HashPassword(senhaOriginal);

            bool isValida = service.VerifyPassword(hashGerado, senhaOriginal);

            
            isValida.Should().BeTrue();
        }

        [Fact]
        public void VerifyPassword_QuandoSenhaForIncorreta_DeveRetornarFalse()
        {
            var service = new PasswordService();
            string senhaOriginal = "senhaCorreta";
            string hashGerado = service.HashPassword(senhaOriginal);

            bool isValida = service.VerifyPassword(hashGerado, "senhaIncorreta_DigitadaErrado");

            isValida.Should().BeFalse();
        }
    }
}