using GameficandoEstudos.Application.Interfaces;

namespace GameficandoEstudos.Application.Services.AtividadeService
{
    public class MoverAtividadeDeModuloService
    {
        private readonly IAtividadeRepository _atividadeRepository;
        private readonly IMateriaRepository _materiaRepository;
        private readonly IModuloRepository _moduloRepository;
        private readonly IAuditoriaService _auditoriaService;

        public MoverAtividadeDeModuloService(
            IAtividadeRepository atividadeRepository,
            IMateriaRepository materiaRepository,
            IModuloRepository moduloRepository,
            IAuditoriaService auditoriaService)
        {
            _atividadeRepository = atividadeRepository;
            _materiaRepository = materiaRepository;
            _moduloRepository = moduloRepository;
            _auditoriaService = auditoriaService;
        }

        
        public async Task ExecutarAsync(int idAtividade, int? idModulo, int idProfessorLogado)
        {
            var atividade = await _atividadeRepository.ObterPorIdAsync(idAtividade);
            if (atividade == null)
                throw new InvalidOperationException("Atividade não encontrada.");

            var materia = await _materiaRepository.ObterPorIdAsync(atividade.IdMateria);
            if (materia == null || materia.IdProfessor != idProfessorLogado)
                throw new UnauthorizedAccessException("Você não tem permissão para alterar atividades de outros professores.");

            if (idModulo.HasValue)
            {
                var modulo = await _moduloRepository.ObterPorIdAsync(idModulo.Value);
                if (modulo == null || modulo.IdMateria != atividade.IdMateria)
                    throw new InvalidOperationException("Módulo não encontrado nesta matéria.");
            }

            atividade.MoverParaModulo(idModulo);
            await _atividadeRepository.AtualizarAsync(atividade);

            await _auditoriaService.RegistrarAsync(
                idProfessorLogado, "Atividade.MoverModulo", "Atividade", idAtividade.ToString(),
                idModulo.HasValue ? $"Módulo: {idModulo}" : "Sem módulo");
        }
    }
}
