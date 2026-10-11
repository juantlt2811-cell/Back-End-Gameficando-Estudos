using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration; 
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Application.Interfaces;

namespace GameficandoEstudos.Infrastructure.Security
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GerarToken(Usuario usuario)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nome),
                new Claim(ClaimTypes.Role, usuario.Tipo.ToString())
            };

            var chave = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:ChaveSecreta"]));

            var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);
            var duracao = int.Parse(_configuration["Jwt:DuracaoEmMinutos"]);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Emissor"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(duracao),
                signingCredentials: credenciais
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}