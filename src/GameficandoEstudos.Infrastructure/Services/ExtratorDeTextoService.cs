using System.Text;
using DocumentFormat.OpenXml.Packaging;
using GameficandoEstudos.Application.Interfaces;
using UglyToad.PdfPig;
using DocumentFormat.OpenXml.Wordprocessing;

namespace GameficandoEstudos.Infrastructure.Services;

public class ExtratorDeTextoService : IExtratorDeTextoService
{
    
    private const int LimiteTextoCaracteres = 500_000;
    private const string MensagemTextoMuitoLongo =
        "O texto do material excede o tamanho máximo permitido (500.000 caracteres).";

    private static readonly HashSet<string> ExtensoesTexto = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".md", ".csv", ".json", ".xml", ".yaml", ".yml", ".html", ".css", ".sql",
        ".cs", ".py", ".js", ".ts", ".java", ".c", ".cpp", ".h", ".go", ".rb", ".php"
    };

    public async Task<string> ExtrairTextoAsync(Stream conteudoArquivo, string nomeArquivo)
    {
        var extensao = Path.GetExtension(nomeArquivo);

        if (ExtensoesTexto.Contains(extensao))
            return await ExtrairDeTextoPuroAsync(conteudoArquivo);

        if (extensao.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            return ExtrairDePdf(conteudoArquivo);

        if (extensao.Equals(".docx", StringComparison.OrdinalIgnoreCase))
            return ExtrairDeDocx(conteudoArquivo);

        throw new NotSupportedException($"Tipo de arquivo '{extensao}' não é suportado.");
    }

    private static async Task<string> ExtrairDeTextoPuroAsync(Stream conteudo)
    {
        using var leitor = new StreamReader(conteudo, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        return await leitor.ReadToEndAsync();
    }

    private static string ExtrairDePdf(Stream conteudo)
    {
        using var documento = PdfDocument.Open(conteudo);
        var textoCompleto = new StringBuilder();

        foreach (var pagina in documento.GetPages())
        {
            textoCompleto.AppendLine(pagina.Text);

            if (textoCompleto.Length > LimiteTextoCaracteres)
                throw new InvalidOperationException(MensagemTextoMuitoLongo);
        }

        return textoCompleto.ToString();
    }

    private static string ExtrairDeDocx(Stream conteudo)
    {
        using var documento = WordprocessingDocument.Open(conteudo, isEditable: false);

        var corpo = documento.MainDocumentPart?.Document.Body;
        if (corpo is null)
            return string.Empty;

        var texto = new StringBuilder();
        foreach (var paragrafo in corpo.Descendants<Paragraph>())
        {
            var linha = paragrafo.InnerText;
            if (string.IsNullOrWhiteSpace(linha))
                continue;

            if (texto.Length > 0)
                texto.Append("\n\n");
            texto.Append(linha);

            if (texto.Length > LimiteTextoCaracteres)
                throw new InvalidOperationException(MensagemTextoMuitoLongo);
        }

        return texto.ToString();
    }
}