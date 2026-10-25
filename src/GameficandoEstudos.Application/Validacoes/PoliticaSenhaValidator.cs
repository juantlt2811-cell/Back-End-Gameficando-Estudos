namespace GameficandoEstudos.Application.Validacoes
{
    public class PoliticaSenhaValidator
    {
        public const int TamanhoMinimo = 8;
        public const int TamanhoMaximo = 128;

        public bool EhValida(string senha)
        {
            if (string.IsNullOrWhiteSpace(senha) || senha.Length < TamanhoMinimo || senha.Length > TamanhoMaximo)
                return false;

            bool temMaiuscula = false;
            bool temMinuscula = false;
            bool temNumero = false;

            foreach (char c in senha)
            {
                if (char.IsUpper(c)) temMaiuscula = true;
                else if (char.IsLower(c)) temMinuscula = true;
                else if (char.IsDigit(c)) temNumero = true;
            }

            return temMaiuscula && temMinuscula && temNumero;
        }
    }
}