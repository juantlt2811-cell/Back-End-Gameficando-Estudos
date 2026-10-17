using System;
using System.Security.Cryptography;
using System.Text;
using GameficandoEstudos.Application.Interfaces;
using Konscious.Security.Cryptography;

namespace GameficandoEstudos.Infrastructure.Security 
{
    public class PasswordService : IPasswordService
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Parallelism = 1;
        private const int Iterations = 2;
        private const int MemoryKB = 19456;

        public string HashPassword(string senhaPlana)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = ComputeHash(senhaPlana, salt, MemoryKB, Iterations, Parallelism);

            return $"$argon2id$v=19$m={MemoryKB},t={Iterations},p={Parallelism}" +
                   $"${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        public bool VerifyPassword(string senhaHashArmazenado, string senhaDigitada)
        {
            var partes = senhaHashArmazenado.Split('$', StringSplitOptions.RemoveEmptyEntries);
            var parametros = partes[2].Split(',');

            int memory = int.Parse(parametros[0].Split('=')[1]);
            int time = int.Parse(parametros[1].Split('=')[1]);
            int parallel = int.Parse(parametros[2].Split('=')[1]);

            byte[] salt = Convert.FromBase64String(partes[3]);
            byte[] hashOriginal = Convert.FromBase64String(partes[4]);
            byte[] hashCalculado = ComputeHash(senhaDigitada, salt, memory, time, parallel);

            return CryptographicOperations.FixedTimeEquals(hashOriginal, hashCalculado);
        }

        private byte[] ComputeHash(string senha, byte[] salt, int memoryKB, int iterations, int parallelism)
        {
            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(senha))
            {
                Salt = salt,
                DegreeOfParallelism = parallelism,
                Iterations = iterations,
                MemorySize = memoryKB
            };
            return argon2.GetBytes(HashSize);
        }
    }
}