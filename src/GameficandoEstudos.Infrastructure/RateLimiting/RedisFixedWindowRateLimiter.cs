using System.Diagnostics;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace GameficandoEstudos.Infrastructure.RateLimiting
{
    /// <summary>
    /// Limiter de janela fixa cujo contador vive no Redis, entao o limite vale para
    /// todas as replicas da API juntas. Se o Redis estiver indisponivel, cai para um
    /// limiter local (por instancia), para a API nunca ficar sem protecao.
    /// </summary>
    public sealed class RedisFixedWindowRateLimiter : RateLimiter
    {
        // INCRBY + PEXPIRE na primeira chamada da janela, no mesmo script (atomico).
        // Se a chave existir sem expiracao por qualquer motivo, o script corrige.
        private const string Script = @"
            local n = redis.call('INCRBY', KEYS[1], ARGV[2])
            local ttl = redis.call('PTTL', KEYS[1])
            if ttl < 0 then
                redis.call('PEXPIRE', KEYS[1], ARGV[1])
                ttl = tonumber(ARGV[1])
            end
            return {n, ttl}";

        private readonly IConnectionMultiplexer _redis;
        private readonly string _chave;
        private readonly int _limite;
        private readonly TimeSpan _janela;
        private readonly ILogger _logger;
        private readonly Lazy<FixedWindowRateLimiter> _local;
        private long _ultimoUso = Stopwatch.GetTimestamp();

        public RedisFixedWindowRateLimiter(
            IConnectionMultiplexer redis, string chave, int limite, TimeSpan janela, ILogger logger)
        {
            _redis = redis;
            _chave = chave;
            _limite = limite;
            _janela = janela;
            _logger = logger;
            _local = new Lazy<FixedWindowRateLimiter>(() => new FixedWindowRateLimiter(
                new FixedWindowRateLimiterOptions
                {
                    PermitLimit = limite,
                    Window = janela,
                    QueueLimit = 0
                }));
        }

        public override TimeSpan? IdleDuration
        {
            get
            {
                // Enquanto a janela nao passou, o limiter conta como "em uso":
                // evita perder a contagem local de fallback no meio da janela.
                var ocioso = Stopwatch.GetElapsedTime(Volatile.Read(ref _ultimoUso));
                return ocioso < _janela ? TimeSpan.Zero : ocioso;
            }
        }

        public override RateLimiterStatistics? GetStatistics() => null;

        // O middleware do ASP.NET tenta primeiro o caminho sincrono e so depois o assincrono.
        // O sincrono nao pode consultar o Redis (bloquearia a thread) nem aprovar pelo limite
        // local (cada replica teria seu proprio contador). Por isso nunca aprova: a decisao
        // sempre vem de AcquireAsync, que consulta o Redis.
        protected override RateLimitLease AttemptAcquireCore(int permitCount) =>
            new RedisRateLimitLease(false, null);

        protected override async ValueTask<RateLimitLease> AcquireAsyncCore(
            int permitCount, CancellationToken cancellationToken)
        {
            Volatile.Write(ref _ultimoUso, Stopwatch.GetTimestamp());

            if (!_redis.IsConnected)
                return _local.Value.AttemptAcquire(permitCount);

            try
            {
                var resultado = await _redis.GetDatabase().ScriptEvaluateAsync(
                    Script,
                    new RedisKey[] { _chave },
                    new RedisValue[] { (long)_janela.TotalMilliseconds, permitCount });

                var itens = (RedisResult[])resultado!;
                var contagem = (long)itens[0];
                var ttlMs = (long)itens[1];

                return contagem <= _limite
                    ? new RedisRateLimitLease(true, null)
                    : new RedisRateLimitLease(false, TimeSpan.FromMilliseconds(Math.Max(ttlMs, 0)));
            }
            catch (Exception ex) when (ex is RedisException or TimeoutException)
            {
                _logger.LogWarning(ex, "Redis indisponivel; usando rate limit local para {Chave}.", _chave);
                return _local.Value.AttemptAcquire(permitCount);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _local.IsValueCreated)
                _local.Value.Dispose();

            base.Dispose(disposing);
        }

        private sealed class RedisRateLimitLease : RateLimitLease
        {
            private readonly TimeSpan? _retryAfter;

            public RedisRateLimitLease(bool adquirido, TimeSpan? retryAfter)
            {
                IsAcquired = adquirido;
                _retryAfter = retryAfter;
            }

            public override bool IsAcquired { get; }

            public override IEnumerable<string> MetadataNames =>
                _retryAfter is null ? Array.Empty<string>() : new[] { MetadataName.RetryAfter.Name };

            public override bool TryGetMetadata(string metadataName, out object? metadata)
            {
                if (_retryAfter is not null && metadataName == MetadataName.RetryAfter.Name)
                {
                    metadata = _retryAfter.Value;
                    return true;
                }

                metadata = null;
                return false;
            }
        }
    }
}
