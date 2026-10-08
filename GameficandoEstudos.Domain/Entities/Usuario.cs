namespace GameficandoEstudos.Domain.Entities;

public class Usuario
{
    public int Id { get; private set; }
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public string Senha { get; private set; } 
    public int TipoUsuarioId { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime AtualizadoEm { get; private set; }

    public TipoUsuario TipoUsuario { get; private set; }

    public Usuario(string nome, string email, string senha, int tipoUsuarioId)
    {
        ArgumentException.ThrowIfNullOrEmpty(nome, nameof(nome));
        ArgumentException.ThrowIfNullOrEmpty(email, nameof(email));
        ArgumentException.ThrowIfNullOrEmpty(senha, nameof(senha));

        if(tipoUsuarioId <= 0)
            throw new ArgumentException("Tipo de usuário inválido.", nameof(tipoUsuarioId));
        

        Nome = nome;
        Email = email;
        Senha = senha;
        TipoUsuarioId = tipoUsuarioId;

        CriadoEm = DateTime.UtcNow;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void AtualizarSenha(string novaSenhaHash)
    {
        Senha = novaSenhaHash;
        AtualizadoEm = DateTime.UtcNow;
    }
}