using System;

namespace GameficandoEstudos.Domain.Entities
{
    public class Modulo
    {
        public const int TamanhoMaximoNome = 100;

        public int Id { get; private set; }
        public string Nome { get; private set; }
        public int IdMateria { get; private set; }
        public DateTime CriadoEm { get; private set; }
        public DateTime AtualizadoEm { get; private set; }

        public Materia Materia { get; private set; }

        private Modulo() { }

        public Modulo(string nome, int idMateria)
        {
            if (idMateria <= 0)
                throw new ArgumentException("Id da matéria inválido.", nameof(idMateria));

            Nome = ValidarNome(nome);
            IdMateria = idMateria;
            CriadoEm = DateTime.UtcNow;
            AtualizadoEm = DateTime.UtcNow;
        }

        public void Renomear(string nome)
        {
            Nome = ValidarNome(nome);
            AtualizadoEm = DateTime.UtcNow;
        }

        private static string ValidarNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("O nome do módulo é obrigatório.", nameof(nome));

            var nomeLimpo = nome.Trim();
            if (nomeLimpo.Length > TamanhoMaximoNome)
                throw new ArgumentException(
                    $"O nome do módulo deve ter no máximo {TamanhoMaximoNome} caracteres.", nameof(nome));

            return nomeLimpo;
        }
    }
}
