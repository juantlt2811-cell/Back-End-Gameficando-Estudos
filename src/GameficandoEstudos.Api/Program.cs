using Npgsql;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services.MateriaService;
using GameficandoEstudos.Application.Services.UsuarioService;
using GameficandoEstudos.Infrastructure.Data;
using GameficandoEstudos.Infrastructure.Repositories;
using GameficandoEstudos.Infrastructure.Security;
using GameficandoEstudos.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITokenService, TokenService>();
var applicationAssembly = typeof(CriarMateriaService).Assembly;

var servicosApp = applicationAssembly.GetTypes()
    .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Service"));

foreach (var servico in servicosApp)
{
    builder.Services.AddScoped(servico);
}

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Cria a fonte de dados do Npgsql e injeta o suporte nativo a vetores
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.UseVector();
var dataSource = dataSourceBuilder.Build();

// Repassa a fonte de dados pronta para o Entity Framework
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(dataSource, o => o.UseVector()));

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
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:ChaveSecreta"] ?? "ChavePadraoSuperSecreta123!"))
    };
});
builder.Services.AddSingleton<ICaixaFontesRepository, FakeCaixaFontesRepository>();
builder.Services.AddSingleton<IUsuarioRepository, FakeUsuarioRepository>();
builder.Services.AddSingleton<IMateriaRepository, FakeMateriaRepository>();
builder.Services.AddSingleton<IAtividadeRepository, FakeAtividadeRepository>();
builder.Services.AddSingleton<IVectorSearchService, FakeVectorSearchService>();
builder.Services.AddSingleton<ILlmService, FakeLlmService>();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();

app.Run();