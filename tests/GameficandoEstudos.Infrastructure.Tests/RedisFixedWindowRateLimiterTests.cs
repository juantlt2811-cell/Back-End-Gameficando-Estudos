using FluentAssertions;
using GameficandoEstudos.Infrastructure.RateLimiting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using StackExchange.Redis;
using System.Threading.RateLimiting;

namespace GameficandoEstudos.Infrastructure.Tests;

public class RedisFixedWindowRateLimiterTests
{
    private const int Limite = 3;
    private static readonly TimeSpan Janela = TimeSpan.FromMinutes(1);

    private static RedisFixedWindowRateLimiter Criar(IConnectionMultiplexer redis) =>
        new(redis, "gameficando:rl:teste:ip:1", Limite, Janela, NullLogger.Instance);

    private static RedisResult Resposta(long contagem, long ttlMs) =>
        RedisResult.Create(new[]
        {
            RedisResult.Create((RedisValue)contagem, ResultType.Integer),
            RedisResult.Create((RedisValue)ttlMs, ResultType.Integer)
        });

    private static Mock<IConnectionMultiplexer> RedisQueResponde(long contagem, long ttlMs)
    {
        var db = new Mock<IDatabase>();
        db.Setup(d => d.ScriptEvaluateAsync(
                It.IsAny<string>(), It.IsAny<RedisKey[]>(), It.IsAny<RedisValue[]>(), It.IsAny<CommandFlags>()))
          .ReturnsAsync(Resposta(contagem, ttlMs));

        var redis = new Mock<IConnectionMultiplexer>();
        redis.Setup(r => r.IsConnected).Returns(true);
        redis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(db.Object);
        return redis;
    }

    [Fact]
    public async Task ContagemDentroDoLimite_DevePermitir()
    {
        using var limiter = Criar(RedisQueResponde(Limite, 30_000).Object);

        using var lease = await limiter.AcquireAsync();

        lease.IsAcquired.Should().BeTrue();
    }

    [Fact]
    public async Task ContagemAcimaDoLimite_DeveRejeitarComRetryAfter()
    {
        using var limiter = Criar(RedisQueResponde(Limite + 1, 30_000).Object);

        using var lease = await limiter.AcquireAsync();

        lease.IsAcquired.Should().BeFalse();
        lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter).Should().BeTrue();
        retryAfter.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public async Task RedisDesconectado_DeveUsarLimiteLocal()
    {
        var redis = new Mock<IConnectionMultiplexer>();
        redis.Setup(r => r.IsConnected).Returns(false);
        using var limiter = Criar(redis.Object);

        for (var i = 0; i < Limite; i++)
        {
            using var ok = await limiter.AcquireAsync();
            ok.IsAcquired.Should().BeTrue();
        }

        using var excedente = await limiter.AcquireAsync();
        excedente.IsAcquired.Should().BeFalse();
    }

    [Fact]
    public async Task RedisComErro_DeveUsarLimiteLocalSemLancarExcecao()
    {
        var redis = new Mock<IConnectionMultiplexer>();
        redis.Setup(r => r.IsConnected).Returns(true);
        redis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
             .Throws(new RedisServerException("Redis fora do ar"));
        using var limiter = Criar(redis.Object);

        for (var i = 0; i < Limite; i++)
        {
            using var ok = await limiter.AcquireAsync();
            ok.IsAcquired.Should().BeTrue();
        }

        using var excedente = await limiter.AcquireAsync();
        excedente.IsAcquired.Should().BeFalse();
    }

    [Fact]
    public void AcquireSincrono_NuncaAprova_ParaForcarConsultaAoRedis()
    {
        var redis = new Mock<IConnectionMultiplexer>();
        using var limiter = Criar(redis.Object);

        using var lease = limiter.AttemptAcquire();

        lease.IsAcquired.Should().BeFalse();
        redis.VerifyNoOtherCalls();
    }
}
