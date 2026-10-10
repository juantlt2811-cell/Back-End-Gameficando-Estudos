using System;

namespace GameficandoEstudos.Domain.Entities
{
    public class PerfilGamificacao
    {
        public int PontosXP { get; private set; }
        public int Nivel { get; private set; }

        public PerfilGamificacao()
        {
            PontosXP = 0;
            Nivel = 1;
        }

        public void AdicionarXP(int xp)
        {
            if (xp < 0) throw new ArgumentException("XP não pode ser negativo");

            PontosXP += xp;
            CalcularLevelUp();
        }

        private void CalcularLevelUp()
        {
            // Regra simples para o MVP: a cada 1000 XP, sobe 1 nível
            if (PontosXP >= Nivel * 1000)
            {
                Nivel++;
            }
        }
    }
}