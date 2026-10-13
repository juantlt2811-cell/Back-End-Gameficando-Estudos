using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Interfaces.Security;
using GameficandoEstudos.Application.Services.MateriaService;
using GameficandoEstudos.Application.Services.UsuarioService;
using GameficandoEstudos.Application.Validacoes;
using GameficandoEstudos.Infrastructure.Data;
using GameficandoEstudos.Infrastructure.Repositories;
using GameficandoEstudos.Infrastructure.Security;
using GameficandoEstudos.Infrastructure.Services;
using GameficandoEstudos.Infrastructure.Services.FakeServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
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
        policy.WithOrigins("http://localhost:5173")
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
});

builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<PoliticaSenhaValidator>();
builder.Services.AddMemoryCache();
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
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:ChaveSecreta"]))
    };
});
builder.Services.AddScoped<ICaixaFontesRepository, CaixaFontesRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IMateriaRepository, MateriaRepository>();
builder.Services.AddScoped<IAtividadeRepository, AtividadeRepository>();
builder.Services.AddScoped<IVectorSearchService, VectorSearchService>();
if (builder.Environment.IsDevelopment())
    builder.Services.AddScoped<IEmailService, FakeEmailService>();
else
    builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddHttpClient<ILlmService, LlmService>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("FrontendDev");
app.UseRateLimiter();
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();