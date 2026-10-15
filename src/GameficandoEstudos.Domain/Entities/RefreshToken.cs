using System;

namespace GameficandoEstudos.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; private set; }
        public string TokenHash { get; private set; }
        public int IdUsuario { get; private set; }
        public DateTime ExpiraEm { get; private set; }
        public DateTime CriadoEm { get; private set; }
        public DateTime? RevogadoEm { get; private set; }
        public string? SubstituidoPorHash { get; private set; }

        private RefreshToken() { }

        public RefreshToken(string tokenHash, int idUsuario, DateTime expiraEm)
        {
            ArgumentException.ThrowIfNullOrEmpty(tokenHash, nameof(tokenHash));
            if (idUsuario <= 0) throw new ArgumentException("Id do usuário inválido.", nameof(idUsuario));

            TokenHash = tokenHash;
            IdUsuario = idUsuario;
            ExpiraEm = expiraEm;
            CriadoEm = DateTime.UtcNow;
        }

        public bool EstaValido => RevogadoEm == null && DateTime.UtcNow < ExpiraEm;

        public void Revogar(string? substituidoPorHash = null)
        {
            RevogadoEm = DateTime.UtcNow;
            SubstituidoPorHash = substituidoPorHash;
        }
    }
}