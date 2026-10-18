namespace GameficandoEstudos.Application.DTOs
{
    public record ResultadoAvaliacaoDto(int Pontuacao, string Feedback)
    {
        public const int LimiarAprovacao = 70;

        public bool Aprovado => Pontuacao >= LimiarAprovacao;
    }
}

    
