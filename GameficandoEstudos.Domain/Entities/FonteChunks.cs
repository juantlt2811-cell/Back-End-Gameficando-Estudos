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
        public CaixaFontes CaixaFonte { get; private set; }

        public FonteChunks(string conteudo, int idCaixaFonte, float[] embeddings)
        {
            ArgumentException.ThrowIfNullOrEmpty(conteudo, nameof(conteudo));
            if (idCaixaFonte <= 0)
                throw new ArgumentException("Id da caixa de fontes inválido.", nameof(idCaixaFonte));
            if (embeddings == null || embeddings.Length == 0)
                throw new ArgumentException("Embeddings não podem ser nulos ou vazios.", nameof(embeddings));
            Conteudo = conteudo;
            IdCaixaFonte = idCaixaFonte;
            Embeddings = embeddings;
            CriadoEm = DateTime.UtcNow;
            AtualizadoEm = DateTime.UtcNow;
        }
    }
}
