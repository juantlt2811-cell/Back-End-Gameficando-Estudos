using GameficandoEstudos.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameficandoEstudos.Domain.Entities
{
    public class Atividade
    {
        public int Id { get; private set; }
        public string Nome { get; private set; }
        public TipoAtividade Tipo { get; private set; }

        
        public string Conteudo { get; private set; }

        public List<string> Alternativas { get; private set; } = new();

        public string Gabarito { get; private set; }
        public int IdMateria { get; private set; }

        
        public int? IdModulo { get; private set; }
        public DateTime CriadoEm { get; private set; }
        public DateTime AtualizadoEm { get; private set; }

        public bool Ativa { get; private set; }

        public Materia Materia { get; private set; }
        public Modulo? Modulo { get; private set; }

        private Atividade() { }
        public Atividade(string nome, TipoAtividade tipo, string conteudo, string gabarito, int idMateria,
            IEnumerable<string>? alternativas = null, int? idModulo = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(nome, nameof(nome));
            ArgumentException.ThrowIfNullOrEmpty(conteudo, nameof(conteudo));
            ArgumentException.ThrowIfNullOrEmpty(gabarito, nameof(gabarito));

            if (!Enum.IsDefined(typeof(TipoAtividade), tipo))
                throw new ArgumentException("Tipo de atividade inválido.", nameof(tipo));

            if (idMateria <= 0)
                throw new ArgumentException("Id da matéria inválido.", nameof(idMateria));

            if (idModulo.HasValue && idModulo.Value <= 0)
                throw new ArgumentException("Id do módulo inválido.", nameof(idModulo));

            var alternativasValidadas = ValidarAlternativas(tipo, alternativas);

            Nome = nome;
            Tipo = tipo;
            Conteudo = conteudo;
            Alternativas = alternativasValidadas;
            Gabarito = gabarito;
            IdMateria = idMateria;
            IdModulo = idModulo;
            CriadoEm = DateTime.UtcNow;
            AtualizadoEm = DateTime.UtcNow;

            Ativa = true;
        }

        
        public void MoverParaModulo(int? idModulo)
        {
            if (idModulo.HasValue && idModulo.Value <= 0)
                throw new ArgumentException("Id do módulo inválido.", nameof(idModulo));

            IdModulo = idModulo;
            AtualizadoEm = DateTime.UtcNow;
        }

        public void Inativar()
        {
            Ativa = false;
            AtualizadoEm = DateTime.UtcNow;
        }
        public void Atualizar(string nome, string conteudo, string gabarito, IEnumerable<string>? alternativas = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(nome, nameof(nome));
            ArgumentException.ThrowIfNullOrEmpty(conteudo, nameof(conteudo));
            ArgumentException.ThrowIfNullOrEmpty(gabarito, nameof(gabarito));

            Nome = nome;
            Conteudo = conteudo;
                Alternativas = ValidarAlternativas(Tipo, alternativas);
            Gabarito = gabarito;
            AtualizadoEm = DateTime.UtcNow;
        }

        private static List<string> ValidarAlternativas(TipoAtividade tipo, IEnumerable<string>? alternativas)
        {
            if (alternativas == null)
                return new List<string>();

            var lista = alternativas.Where(a => !string.IsNullOrWhiteSpace(a)).ToList();

            if (tipo == TipoAtividade.MultiplaEscolha && lista.Count < 2)
                throw new ArgumentException(
                    "Atividades de múltipla escolha precisam de pelo menos duas alternativas.", nameof(alternativas));

            return lista;
        }
    }
}