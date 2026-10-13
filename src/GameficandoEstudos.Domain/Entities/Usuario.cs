using GameficandoEstudos.Domain.Enums;
using System.Security.Cryptography;
using System.Text;

namespace GameficandoEstudos.Domain.Entities
{
    public class Usuario
    {
        public int Id { get; private set; }
        public string Nome { get; private set; }
        public string Email { get; private set; }
        public string Senha { get; private set; }
        public DateTime CriadoEm { get; private set; }
        public DateTime AtualizadoEm { get; private set; }
        public TipoUsuario Tipo { get; private set; }
        public PerfilGamificacao? Gamificacao { get; private set; }
        public string? TokenRedefinicaoSenhaHash { get; private set; }
        public DateTime? TokenRedefinicaoSenhaExpiraEm { get; private set; }

        private Usuario() { }
        public Usuario(string nome, string email, string senha, TipoUsuario tipo)
        {
            ArgumentException.ThrowIfNullOrEmpty(nome, nameof(nome));
            ArgumentException.ThrowIfNullOrEmpty(email, nameof(email));
            ArgumentException.ThrowIfNullOrEmpty(senha, nameof(senha));

            if (!Enum.IsDefined(typeof(TipoUsuario), tipo))
                throw new ArgumentException("Tipo de usuário inválido.", nameof(tipo));


            Nome = nome;
            Email = email;
            Senha = senha;
            Tipo = tipo;

            if (tipo == TipoUsuario.Aluno)
            {
                Gamificacao = new PerfilGamificacao();
            }


            CriadoEm = DateTime.UtcNow;
            AtualizadoEm = DateTime.UtcNow;
        }

        public void AtualizarSenha(string novaSenhaHash)
        {
            Senha = novaSenhaHash;
            AtualizadoEm = DateTime.UtcNow;
        }
        public void DefinirTokenRedefinicaoSenha(string tokenHash, DateTime expiraEm)
        {
            ArgumentException.ThrowIfNullOrEmpty(tokenHash, nameof(tokenHash));
            TokenRedefinicaoSenhaHash = tokenHash;
            TokenRedefinicaoSenhaExpiraEm = expiraEm;
            AtualizadoEm = DateTime.UtcNow;
        }

        public bool TokenRedefinicaoSenhaEhValido(string tokenHash)
        {
            if (string.IsNullOrEmpty(TokenRedefinicaoSenhaHash) || TokenRedefinicaoSenhaExpiraEm == null)
                return false;

            if (DateTime.UtcNow > TokenRedefinicaoSenhaExpiraEm.Value)
                return false;

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(TokenRedefinicaoSenhaHash),
                Encoding.UTF8.GetBytes(tokenHash));
        }

        public void LimparTokenRedefinicaoSenha()
        {
            TokenRedefinicaoSenhaHash = null;
            TokenRedefinicaoSenhaExpiraEm = null;
            AtualizadoEm = DateTime.UtcNow;
        }
    }
}

