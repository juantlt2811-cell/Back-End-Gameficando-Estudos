namespace GameficandoEstudos.Domain.Entities;

public class Materia
{
    public int Id { get; private set; }
    public string Nome { get; private set; }
    public int CargaHoraria { get; private set; }
    public string Dificuldade { get; private set; }
    public int IdProfessor { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime AtualizadoEm { get; private set; }

    public Usuario Professor { get; private set; }

    public Materia(string nome, int cargaHoraria, string dificuldade, int idProfessor)
    {
        ArgumentException.ThrowIfNullOrEmpty(nome, nameof(nome));
        if (cargaHoraria <= 0)
            throw new ArgumentException("Carga horária deve ser maior que zero.", nameof(cargaHoraria));
        ArgumentException.ThrowIfNullOrEmpty(dificuldade, nameof(dificuldade));
        if (idProfessor <= 0)
            throw new ArgumentException("Id do professor inválido.", nameof(idProfessor));

        Nome = nome;
        CargaHoraria = cargaHoraria;
        Dificuldade = dificuldade;
        IdProfessor = idProfessor;

        CriadoEm = DateTime.UtcNow;
        AtualizadoEm = DateTime.UtcNow;
    }
}