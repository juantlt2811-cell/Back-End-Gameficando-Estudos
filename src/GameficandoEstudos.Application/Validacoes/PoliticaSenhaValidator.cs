using System;
using System.Collections.Generic;
using System.Text;

namespace GameficandoEstudos.Application.Validacoes
{
    public class PoliticaSenhaValidator
    {
        public bool EhValida(string senha)
        {
            if(string.IsNullOrWhiteSpace(senha) || senha.Length < 8)
                return false;
            bool temMaiuscula = false;
            bool temMinuscula = false;
            bool temNumero = false;
            foreach (char c in senha)
            {
                if (char.IsUpper(c)) temMaiuscula = true;
                else if (char.IsLower(c)) temMinuscula = true;
                else if (char.IsDigit(c)) temNumero = true;
                if (temMaiuscula && temMinuscula && temNumero)
                    return true;
            }
            return false;
        }
    }
}
