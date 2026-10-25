using System.IO.Compression;
using GameficandoEstudos.Application.Interfaces;

namespace GameficandoEstudos.Application.Services.AtividadeService
{
    public class ProcessarArquivoEstudoService
    {
        private const long TamanhoMaximoBytes = 10 * 1024 * 1024;
        
        private const long TamanhoMaximoDescompactadoBytes = 50L * 1024 * 1024;

        private static readonly HashSet<string> ExtensoesPermitidas = new(StringComparer.OrdinalIgnoreCase)
        {
            ".txt", ".md", ".csv", ".json", ".xml", ".yaml", ".yml", ".html", ".css", ".sql",
            ".cs", ".py", ".js", ".ts", ".java", ".c", ".cpp", ".h", ".go", ".rb", ".php",
            ".pdf", ".docx"
        };

        private readonly IExtratorDeTextoService _extratorDeTexto;
        private readonly ProcessarMaterialEstudoService _processarMaterialEstudoService;

        public ProcessarArquivoEstudoService(
            IExtratorDeTextoService extratorDeTexto,
            ProcessarMaterialEstudoService processarMaterialEstudoService)
        {
            _extratorDeTexto = extratorDeTexto;
            _processarMaterialEstudoService = processarMaterialEstudoService;
        }

        public async Task ExecutarAsync(
            int idMateria, string nomeArquivo, Stream conteudoArquivo, long tamanhoBytes, int idProfessorLogado)
        {
            var extensao = Path.GetExtension(nomeArquivo);
            if (string.IsNullOrEmpty(extensao) || !ExtensoesPermitidas.Contains(extensao))
                throw new InvalidOperationException($"Tipo de arquivo '{extensao}' não é permitido.");

            if (tamanhoBytes <= 0)
                throw new InvalidOperationException("Arquivo vazio.");

            if (tamanhoBytes > TamanhoMaximoBytes)
                throw new InvalidOperationException("Arquivo excede o tamanho máximo permitido (10 MB).");

            await ValidarAssinaturaAsync(conteudoArquivo, extensao);

            var texto = await _extratorDeTexto.ExtrairTextoAsync(conteudoArquivo, nomeArquivo);
            if (string.IsNullOrWhiteSpace(texto))
                throw new InvalidOperationException("Não foi possível extrair texto deste arquivo.");

            await _processarMaterialEstudoService.ProcessarTextoAsync(idMateria, nomeArquivo, texto, idProfessorLogado);
        }

        private static async Task ValidarAssinaturaAsync(Stream conteudo, string extensao)
        {
            if (extensao.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                var assinatura = new byte[4];
                await conteudo.ReadExactlyAsync(assinatura);
                conteudo.Position = 0;

                if (assinatura[0] != 0x25 || assinatura[1] != 0x50 || assinatura[2] != 0x44 || assinatura[3] != 0x46)
                    throw new InvalidOperationException("O arquivo não é um PDF válido.");
            }
            else if (extensao.Equals(".docx", StringComparison.OrdinalIgnoreCase))
            {
                var assinatura = new byte[2];
                await conteudo.ReadExactlyAsync(assinatura);
                conteudo.Position = 0;

                if (assinatura[0] != 0x50 || assinatura[1] != 0x4B)
                    throw new InvalidOperationException("O arquivo não é um DOCX válido.");

                VerificarTamanhoDescompactado(conteudo);
            }
        }

        private static void VerificarTamanhoDescompactado(Stream conteudo)
        {
            try
            {
                using var zip = new ZipArchive(conteudo, ZipArchiveMode.Read, leaveOpen: true);

                long total = 0;
                foreach (var entrada in zip.Entries)
                {
                    total += entrada.Length;
                    if (total > TamanhoMaximoDescompactadoBytes)
                        throw new InvalidOperationException("O arquivo DOCX descompactado é grande demais.");
                }
            }
            catch (InvalidDataException)
            {
                throw new InvalidOperationException("O arquivo não é um DOCX válido.");
            }
            finally
            {
                conteudo.Position = 0;
            }
        }
    }
}