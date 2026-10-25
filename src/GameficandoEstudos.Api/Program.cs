using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Interfaces.Security;
using GameficandoEstudos.Application.Services.AtividadeService;
using GameficandoEstudos.Application.Services.MateriaService;
using GameficandoEstudos.Application.Services.UsuarioService;
using GameficandoEstudos.Application.Validacoes;
using GameficandoEstudos.Infrastructure.Data;
using GameficandoEstudos.Infrastructure.Repositories;
using GameficandoEstudos.Infrastructure.Security;
using GameficandoEstudos.Infrastructure.Services;
using GameficandoEstudos.Infrastructure.Services.FakeServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Polly;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
    {
        var origensPermitidas = builder.Configuration.GetSection("Cors:OrigensPermitidas").Get<string[]>()
                                  ?? new[] { "http://localhost:5173" };
        policy.WithOrigins(origensPermitidas)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    options.AddPolicy("ia", httpContext =>
    RateLimitPartition.GetFixedWindowLimiter(
        partitionKey: httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                      ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
        factory: _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 20,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});


builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<PoliticaSenhaValidator>();
builder.Services.AddSingleton(new CadastroOptions(builder.Configuration["Cadastro:CodigoProfessor"]));


builder.Services.AddMemoryCache(options =>
{
    options.SizeLimit = 10_000;
});


builder.Services.AddSingleton<ILoginAttemptGuard, LoginAttemptGuard>();



var applicationAssembly = typeof(CriarMateriaService).Assembly;

var servicosApp = applicationAssembly.GetTypes()
    .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Service"));

foreach (var servico in servicosApp)
{
    builder.Services.AddScoped(servico);
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.UseVector();
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.UseVector() 
    ));

var jwtChaveSecreta = builder.Configuration["Jwt:ChaveSecreta"];
if (string.IsNullOrWhiteSpace(jwtChaveSecreta))
    throw new InvalidOperationException("Configuração 'Jwt:ChaveSecreta' não foi definida.");

if (Encoding.UTF8.GetByteCount(jwtChaveSecreta) < 32)
    throw new InvalidOperationException(
        "Configuração 'Jwt:ChaveSecreta' precisa ter pelo menos 32 bytes para uso com HmacSha256.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Emissor"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtChaveSecreta)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddScoped<ICaixaFontesRepository, CaixaFontesRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IMateriaRepository, MateriaRepository>();
builder.Services.AddScoped<IAtividadeRepository, AtividadeRepository>();
builder.Services.AddScoped<IVectorSearchService, VectorSearchService>();
builder.Services.AddScoped<ISessaoPraticaRepository, SessaoPraticaRepository>();
builder.Services.AddScoped<IRankingRepository, RankingRepository>();
builder.Services.AddScoped<IExtratorDeTextoService, ExtratorDeTextoService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddSingleton(new DivisorDeTextoOptions());
builder.Services.AddScoped<IDivisorDeTextoService, DivisorDeTextoService>();
if (builder.Environment.IsDevelopment())
    builder.Services.AddScoped<IEmailService, FakeEmailService>();
else
    builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddHttpClient("groq")
    .AddResilienceHandler("groq-pipeline", pipeline =>
    {
        pipeline.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 2,
            BackoffType = DelayBackoffType.Exponential,
            Delay = TimeSpan.FromSeconds(1)
        });

        pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(30),
            MinimumThroughput = 5,
            BreakDuration = TimeSpan.FromSeconds(30)
        });

        pipeline.AddTimeout(TimeSpan.FromSeconds(15));
    });

builder.Services.AddHttpClient("gemini")
    .AddResilienceHandler("gemini-pipeline", pipeline =>
    {
        pipeline.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 2,
            BackoffType = DelayBackoffType.Exponential,
            Delay = TimeSpan.FromMilliseconds(500)
        });

        pipeline.AddTimeout(TimeSpan.FromSeconds(10));
    });

builder.Services.AddScoped<ILlmService, LlmService>();
var app = builder.Build();

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();

app.UseForwardedHeaders(forwardedHeadersOptions);
app.UseExceptionHandler(app2 =>
{
    app2.Run(async context =>
    {
        var exceptionHandler = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogError(exceptionHandler?.Error, "Erro nao tratado");

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { Erro = "Ocorreu um erro inesperado. Tente novamente mais tarde." });
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.UseHttpsRedirection();
app.UseCors("FrontendDev");
app.UseAuthentication(); 
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();

app.Run();