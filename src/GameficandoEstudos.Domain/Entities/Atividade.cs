using GameficandoEstudos.Domain.Enums;
using System;

namespace GameficandoEstudos.Domain.Entities
{
    public class Atividade
    {
        public int Id { get; private set; }
        public string Nome { get; private set; }
        public TipoAtividade Tipo { get; private set; }
        public string Conteudo { get; private set; }
        public string Gabarito { get; private set; }
        public int IdMateria { get; private set; }
        public DateTime CriadoEm { get; private set; }
        public DateTime AtualizadoEm { get; private set; }

        public bool Ativa { get; private set; }

        public Materia Materia { get; private set; }

        private Atividade() { }
        public Atividade(string nome, TipoAtividade tipo, string conteudo, string gabarito, int idMateria)
        {
            ArgumentException.ThrowIfNullOrEmpty(nome, nameof(nome));
            ArgumentException.ThrowIfNullOrEmpty(conteudo, nameof(conteudo));
            ArgumentException.ThrowIfNullOrEmpty(gabarito, nameof(gabarito));

            if (!Enum.IsDefined(typeof(TipoAtividade), tipo))
                throw new ArgumentException("Tipo de atividade inválido.", nameof(tipo));

            if (idMateria <= 0)
                throw new ArgumentException("Id da matéria inválido.", nameof(idMateria));

            Nome = nome;
            Tipo = tipo;
            Conteudo = conteudo;
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
        public void Atualizar(string nome, string conteudo, string gabarito)
        {
            ArgumentException.ThrowIfNullOrEmpty(nome, nameof(nome));
            ArgumentException.ThrowIfNullOrEmpty(conteudo, nameof(conteudo));
            ArgumentException.ThrowIfNullOrEmpty(gabarito, nameof(gabarito));

            Nome = nome;
            Conteudo = conteudo;
            Gabarito = gabarito;
            AtualizadoEm = DateTime.UtcNow;
        }
    }
}