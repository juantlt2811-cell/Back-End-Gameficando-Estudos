using System;

namespace GameficandoEstudos.Domain.Entities
{
    public class TentativaResposta
    {
        
        private const int LimiarAprovacao = 70;

        public int Id { get; private set; }
        public int IdSessaoPratica { get; private set; }
        public int IdAtividade { get; private set; }
        public int IdUsuario { get; private set; }
        public int IdMateria { get; private set; }
        public string RespostaAluno { get; private set; }
        public int Pontuacao { get; private set; }
        public bool Acertou { get; private set; }
        public string Feedback { get; private set; }
        public DateTime RespondidoEm { get; private set; }

        public Atividade Atividade { get; private set; }

        private TentativaResposta() { }

        public TentativaResposta(
            int idSessaoPratica, int idAtividade, int idUsuario, int idMateria,
            string respostaAluno, int pontuacao, string feedback)
        {
            ArgumentException.ThrowIfNullOrEmpty(respostaAluno, nameof(respostaAluno));

            if (pontuacao < 0 || pontuacao > 100)
                throw new ArgumentException("A pontuação deve estar entre 0 e 100.", nameof(pontuacao));

            IdSessaoPratica = idSessaoPratica;
            IdAtividade = idAtividade;
            IdUsuario = idUsuario;
            IdMateria = idMateria;
            RespostaAluno = respostaAluno;
            Pontuacao = pontuacao;
            Acertou = pontuacao >= LimiarAprovacao;
            Feedback = feedback ?? string.Empty;
            RespondidoEm = DateTime.UtcNow;
        }
    }
}