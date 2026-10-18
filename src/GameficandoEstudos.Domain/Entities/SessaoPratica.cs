using System;
using System.Collections.Generic;
using System.Linq;
using GameficandoEstudos.Domain.Enums;

namespace GameficandoEstudos.Domain.Entities
{
    public class SessaoPratica
    {
        public int Id { get; private set; }
        public int IdUsuario { get; private set; }
        public int IdMateria { get; private set; }
        public StatusSessaoPratica Status { get; private set; }
        public int TotalQuestoes { get; private set; }
        public int TotalAcertos { get; private set; }
        public DateTime IniciadaEm { get; private set; }
        public DateTime? ConcluidaEm { get; private set; }

        public Usuario Usuario { get; private set; }
        public Materia Materia { get; private set; }

        private readonly List<TentativaResposta> _tentativas = new();
        public IReadOnlyCollection<TentativaResposta> Tentativas => _tentativas;

        private SessaoPratica() { }

        public SessaoPratica(int idUsuario, int idMateria, int totalQuestoes)
        {
            if (idUsuario <= 0) throw new ArgumentException("Id do usuário inválido.", nameof(idUsuario));
            if (idMateria <= 0) throw new ArgumentException("Id da matéria inválido.", nameof(idMateria));
            if (totalQuestoes <= 0) throw new ArgumentException("A sessão precisa de ao menos uma questão.", nameof(totalQuestoes));

            IdUsuario = idUsuario;
            IdMateria = idMateria;
            TotalQuestoes = totalQuestoes;
            Status = StatusSessaoPratica.EmAndamento;
            IniciadaEm = DateTime.UtcNow;
        }

        public void RegistrarTentativa(TentativaResposta tentativa)
        {
            if (Status != StatusSessaoPratica.EmAndamento)
                throw new InvalidOperationException("Esta sessão de prática já foi encerrada.");

            if (_tentativas.Any(t => t.IdAtividade == tentativa.IdAtividade))
                throw new InvalidOperationException("Essa questão já foi respondida nesta sessão.");

            _tentativas.Add(tentativa);
            if (tentativa.Acertou)
                TotalAcertos++;
        }

        public void Concluir()
        {
            if (Status != StatusSessaoPratica.EmAndamento)
                throw new InvalidOperationException("Esta sessão de prática já foi encerrada.");

            if (_tentativas.Count == 0)
                throw new InvalidOperationException("Responda ao menos uma questão antes de concluir a sessão.");

            Status = StatusSessaoPratica.Concluida;
            ConcluidaEm = DateTime.UtcNow;
        }

        public double PercentualAcerto =>
            _tentativas.Count == 0 ? 0 : (double)TotalAcertos / _tentativas.Count * 100;

        
        public double NotaMedia =>
            _tentativas.Count == 0 ? 0 : _tentativas.Average(t => t.Pontuacao);
    }
}