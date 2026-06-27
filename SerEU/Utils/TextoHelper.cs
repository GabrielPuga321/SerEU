using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SerEU.Utils;

/// <summary>
/// Utilitários de normalização de texto para pesquisa "tolerante":
/// ignora maiúsculas/minúsculas, acentos e espaços extra.
/// </summary>
public static partial class TextoHelper
{
    [GeneratedRegex(@"\s+")]
    private static partial Regex EspacosRegex();

    /// <summary>
    /// Normaliza um texto para comparação: minúsculas, sem acentos,
    /// sem espaços duplicados e sem espaços nas pontas.
    /// </summary>
    public static string Normalizar(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

        // Minúsculas + colapso de espaços
        var t = EspacosRegex().Replace(texto.Trim().ToLowerInvariant(), " ");

        // Remove diacríticos (acentos)
        var decomposto = t.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposto.Length);
        foreach (var c in decomposto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// Divide um termo de pesquisa em palavras já normalizadas.
    /// </summary>
    public static string[] Tokenizar(string? termo)
    {
        var normalizado = Normalizar(termo);
        return normalizado.Length == 0
            ? []
            : normalizado.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }
}
