namespace GameficandoEstudos.Domain.Entities
{
    public class CaixaFontes
    {
        public int Id { get; private set; }
        public string Nome { get; private set; }
        public int IdMateria { get; private set; }
        public DateTime CriadoEm { get; private set; }
        public DateTime AtualizadoEm { get; private set; }

        public Materia Materia { get; private set; }

        public CaixaFontes(string nome, int idMateria)
        {
            ArgumentException.ThrowIfNullOrEmpty(nome, nameof(nome));
            if (idMateria <= 0)
                throw new ArgumentException("Id da matéria inválido.", nameof(idMateria));
            Nome = nome;
            IdMateria = idMateria;
            CriadoEm = DateTime.UtcNow;
            AtualizadoEm = DateTime.UtcNow;
        }
        private readonly List<FonteChunks> _chunks = new();
        public IReadOnlyCollection<FonteChunks> Chunks => _chunks.AsReadOnly();

        public void AdicionarChunk(FonteChunks chunk)
        {
            ArgumentNullException.ThrowIfNull(chunk, nameof(chunk));
            _chunks.Add(chunk);
        }
    }
}
