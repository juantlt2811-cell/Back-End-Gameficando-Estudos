namespace GameficandoEstudos.Domain.Entities
{
    public class TipoUsuario
    {
        public int Id { get; private set; }
        public string Nome { get; private set; }
        public string Descricao { get; private set; }

        public TipoUsuario(string nome, string descricao)
        {
            ArgumentException.ThrowIfNullOrEmpty(nome, nameof(nome));
            ArgumentException.ThrowIfNullOrEmpty(descricao, nameof(descricao));

            Nome = nome;
            Descricao = descricao;
        }
    }
}
