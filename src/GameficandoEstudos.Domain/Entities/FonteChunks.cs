namespace GameficandoEstudos.Domain.Entities
{
    public class FonteChunks
    {
        public int Id { get; private set; }
        public string Conteudo { get; private set; }
        public int IdCaixaFonte { get; private set; }
        public DateTime CriadoEm { get; private set; }
        public DateTime AtualizadoEm { get; private set; }
        public float[] Embeddings { get; private set; }
        public Guid DocumentoId { get; private set; } 
        public string Origem { get; private set; }
        public CaixaFontes CaixaFonte { get; private set; }

        private FonteChunks() { }
        public FonteChunks(string conteudo, int idCaixaFonte, float[] embeddings, string origem, Guid documentoId)
        {
            ArgumentException.ThrowIfNullOrEmpty(conteudo, nameof(conteudo));
            ArgumentException.ThrowIfNullOrEmpty(origem, nameof(origem));
            if (idCaixaFonte <= 0)
                throw new ArgumentException("Id da caixa de fontes inválido.", nameof(idCaixaFonte));
            if (embeddings == null || embeddings.Length == 0)
                throw new ArgumentException("Embeddings não podem ser nulos ou vazios.", nameof(embeddings));
            if (documentoId == Guid.Empty) throw new ArgumentException("Id do documento inválido.");
            Conteudo = conteudo;
            IdCaixaFonte = idCaixaFonte;
            Embeddings = embeddings;
            Origem = origem;
            DocumentoId = documentoId;
            CriadoEm = DateTime.UtcNow;
            AtualizadoEm = DateTime.UtcNow;
        }
    }
}
