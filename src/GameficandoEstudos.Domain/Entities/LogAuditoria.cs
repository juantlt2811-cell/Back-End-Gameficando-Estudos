using System;

namespace GameficandoEstudos.Domain.Entities
{
    public class LogAuditoria
    {
        public int Id { get; private set; }
        public int IdUsuario { get; private set; }
        public string Acao { get; private set; }
        public string Entidade { get; private set; }
        public string IdEntidade { get; private set; }
        public string? Detalhes { get; private set; }
        public DateTime RegistradoEm { get; private set; }

        private LogAuditoria() { }

        public LogAuditoria(int idUsuario, string acao, string entidade, string idEntidade, string? detalhes = null)
        {
            if (idUsuario <= 0) throw new ArgumentException("Id do usuário inválido.", nameof(idUsuario));
            ArgumentException.ThrowIfNullOrEmpty(acao, nameof(acao));
            ArgumentException.ThrowIfNullOrEmpty(entidade, nameof(entidade));
            ArgumentException.ThrowIfNullOrEmpty(idEntidade, nameof(idEntidade));

            IdUsuario = idUsuario;
            Acao = acao;
            Entidade = entidade;
            IdEntidade = idEntidade;
            Detalhes = detalhes;
            RegistradoEm = DateTime.UtcNow;
        }
    }
}