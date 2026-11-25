using FluentAssertions;
using GameficandoEstudos.Infrastructure.Security;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StackExchange.Redis;

namespace GameficandoEstudos.Infrastructure.Tests;

public class LoginAttemptGuardTests
{
    private static LoginAttemptGuard CriarGuardEmMemoria() =>
        new(new MemoryCache(new MemoryCacheOptions { SizeLimit = 100 }));

    [Fact]
    public async Task EmMemoria_AposLimiteDeFalhas_DeveBloquear()
    {
        var guard = CriarGuardEmMemoria();

        for (var i = 0; i < LoginAttemptGuard.LimiteTentativas - 1; i++)
            await guard.RegistrarFalhaAsync("a@email.com");
        (await guard.EstaBloqueadoAsync("a@email.com")).Should().BeFalse();

        await guard.RegistrarFalhaAsync("a@email.com");
        (await guard.EstaBloqueadoAsync("a@email.com")).Should().BeTrue();
    }

    [Fact]
    public async Task EmMemoria_EmailComCaixaOuEspacosDiferentes_ContaComoOMesmo()
    {
        var guard = CriarGuardEmMemoria();

        for (var i = 0; i < LoginAttemptGuard.LimiteTentativas; i++)
            await guard.RegistrarFalhaAsync("  A@Email.com ");

        (await guard.EstaBloqueadoAsync("a@email.com")).Should().BeTrue();
    }

    [Fact]
    public async Task EmMemoria_Sucesso_DeveLimparFalhas()
    {
        var guard = CriarGuardEmMemoria();

        for (var i = 0; i < LoginAttemptGuard.LimiteTentativas; i++)
            await guard.RegistrarFalhaAsync("a@email.com");
        await guard.RegistrarSucessoAsync("a@email.com");

        (await guard.EstaBloqueadoAsync("a@email.com")).Should().BeFalse();
    }

    [Fact]
    public async Task ComRedisIndisponivel_DeveUsarContagemLocalSemLancarExcecao()
    {
        var redis = new Mock<IConnectionMultiplexer>();
        redis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
             .Throws(new RedisServerException("Redis fora do ar"));
        var fallback = CriarGuardEmMemoria();
        var guard = new RedisLoginAttemptGuard(
            redis.Object, fallback, NullLogger<RedisLoginAttemptGuard>.Instance);

        (await guard.EstaBloqueadoAsync("a@email.com")).Should().BeFalse();

        for (var i = 0; i < LoginAttemptGuard.LimiteTentativas; i++)
            await guard.RegistrarFalhaAsync("a@email.com");
        (await guard.EstaBloqueadoAsync("a@email.com")).Should().BeTrue();

        await guard.RegistrarSucessoAsync("a@email.com");
        (await guard.EstaBloqueadoAsync("a@email.com")).Should().BeFalse();
    }
}
