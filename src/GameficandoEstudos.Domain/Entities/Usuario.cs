using GameficandoEstudos.Domain.Enums;

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
    }
}

