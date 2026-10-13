using System;
using System.Security.Cryptography;
using System.Text;

namespace GameficandoEstudos.Application.Validacoes
{
    public static class TokenHashHelper
    {
        public static string GerarHash(string tokenBruto)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(tokenBruto));
            return Convert.ToBase64String(bytes);
        }
    }
}