using System;

namespace GameficandoEstudos.Domain.Entities
{
    public class PerfilGamificacao
    {
        public int OfensivaAtual { get; private set; }
        public int MaiorOfensiva { get; private set; }
        public int AtividadesFeitasHoje { get; private set; }
        public DateTime? UltimaAtividadeRealizadaEm { get; private set; }

        public const int LIMITE_DIARIO = 3;

        public PerfilGamificacao()
        {
            OfensivaAtual = 0;
            MaiorOfensiva = 0;
            AtividadesFeitasHoje = 0;
        }

        public bool PodeRealizarAtividade()
        {
            var hoje = DateTime.UtcNow.Date;

            if (UltimaAtividadeRealizadaEm?.Date == hoje)
                return AtividadesFeitasHoje < LIMITE_DIARIO;

            return true;
        }

        public void RegistrarAtividadeConcluida()
        {
            if (!PodeRealizarAtividade())
                throw new InvalidOperationException("Limite diário de atividades atingido. Descanse e volte amanhã!");

            var hoje = DateTime.UtcNow.Date;
            var dataUltima = UltimaAtividadeRealizadaEm?.Date;

            if (dataUltima == hoje)
            {
                AtividadesFeitasHoje++;
            }
            else
            {
                AtividadesFeitasHoje = 1;

                if (dataUltima == hoje.AddDays(-1))
                {
                    OfensivaAtual++;
                }
                else
                {
                    OfensivaAtual = 1;
                }
            }

            if (OfensivaAtual > MaiorOfensiva)
                MaiorOfensiva = OfensivaAtual;

            UltimaAtividadeRealizadaEm = DateTime.UtcNow;
        }
    }
}