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

        /// <summary>
        /// Apenas o enunciado da questão. As opções (quando houver) ficam em <see cref="Alternativas"/>,
        /// para que o cliente não precise reinterpretar uma string única com regex.
        /// </summary>
        public string Conteudo { get; private set; }

        /// <summary>
        /// Opções de resposta, uma por item (ex.: "A) Texto da opção"). Só é preenchida quando
        /// <see cref="Tipo"/> é <see cref="TipoAtividade.MultiplaEscolha"/>; para os demais tipos fica vazia.
        /// </summary>
        public List<string> Alternativas { get; private set; } = new();

        public string Gabarito { get; private set; }
        public int IdMateria { get; private set; }
        public DateTime CriadoEm { get; private set; }
        public DateTime AtualizadoEm { get; private set; }

        public bool Ativa { get; private set; }

        public Materia Materia { get; private set; }

        private Atividade() { }
        public Atividade(string nome, TipoAtividade tipo, string conteudo, string gabarito, int idMateria, IEnumerable<string>? alternativas = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(nome, nameof(nome));
            ArgumentException.ThrowIfNullOrEmpty(conteudo, nameof(conteudo));
            ArgumentException.ThrowIfNullOrEmpty(gabarito, nameof(gabarito));

            if (!Enum.IsDefined(typeof(TipoAtividade), tipo))
                throw new ArgumentException("Tipo de atividade inválido.", nameof(tipo));

            if (idMateria <= 0)
                throw new ArgumentException("Id da matéria inválido.", nameof(idMateria));

            var alternativasValidadas = ValidarAlternativas(tipo, alternativas);

            Nome = nome;
            Tipo = tipo;
            Conteudo = conteudo;
            Alternativas = alternativasValidadas;
            Gabarito = gabarito;
            IdMateria = idMateria;
            CriadoEm = DateTime.UtcNow;
            AtualizadoEm = DateTime.UtcNow;

            Ativa = true;
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
            // alternativas == null significa "não informado nesta chamada": preserva as alternativas atuais
            // em vez de apagá-las, para não quebrar clientes antigos que ainda não enviam esse campo.
            if (alternativas != null)
                Alternativas = ValidarAlternativas(Tipo, alternativas);
            Gabarito = gabarito;
            AtualizadoEm = DateTime.UtcNow;
        }

        private static List<string> ValidarAlternativas(TipoAtividade tipo, IEnumerable<string>? alternativas)
        {
            // alternativas == null significa "não informado" (ex.: chamadas antigas/testes que não lidam
            // com alternativas). Nesse caso não validamos quantidade — quem efetivamente gera ou edita o
            // conteúdo de uma atividade de múltipla escolha (GeradorAtividadeService, AtualizarAtividadeService)
            // sempre passa a lista, então a validação abaixo é o que protege o dado real.
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