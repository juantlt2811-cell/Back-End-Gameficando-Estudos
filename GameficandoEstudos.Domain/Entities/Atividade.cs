namespace GameficandoEstudos.Domain.Entities
{
    public class Atividade
    {
        public int Id { get; private set; }
        public string Nome { get; private set; }
        public string Tipo { get; private set; }
        public string Conteudo { get; private set; }
        public int IdMateria { get; private set; }
        public DateTime CriadoEm { get; private set; }
        public DateTime AtualizadoEm { get; private set; }

        public Materia Materia { get; private set; }

        public Atividade(string nome, string tipo, string conteudo, int idMateria)
        {
            ArgumentException.ThrowIfNullOrEmpty(nome, nameof(nome));
            ArgumentException.ThrowIfNullOrEmpty(tipo, nameof(tipo));
            ArgumentException.ThrowIfNullOrEmpty(conteudo, nameof(conteudo));
            if (idMateria <= 0)
                throw new ArgumentException("Id da matéria inválido.", nameof(idMateria));
            Nome = nome;
            Tipo = tipo;
            IdMateria = idMateria;
            CriadoEm = DateTime.UtcNow;
            AtualizadoEm = DateTime.UtcNow;
        }


    }
}
