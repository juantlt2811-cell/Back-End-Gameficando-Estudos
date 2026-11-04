namespace GameficandoEstudos.Domain.Enums
{
    public enum TamanhoSessao
    {
        Curta = 1,
        Media = 2,
        Longa = 3,
        Todas = 4
    }

    public static class TamanhoSessaoExtensions
    {
        public static int? QuantidadeMaxima(this TamanhoSessao tamanho) => tamanho switch
        {
            TamanhoSessao.Curta => 5,
            TamanhoSessao.Media => 10,
            TamanhoSessao.Longa => 20,
            TamanhoSessao.Todas => null,
            _ => throw new ArgumentException("Tamanho de sessão inválido.", nameof(tamanho))
        };
    }
}
