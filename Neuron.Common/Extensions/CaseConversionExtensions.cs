using System.Text;

namespace Neuron.Common.Extensions;

public static class CaseConversionExtensions
{
    public static string ToSnakeCase(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;
        
        var textSpan = text.AsSpan();
        var inserts = 0;

        for (var i = 0; i < text.Length; i++)
        {
            if (i != 0 && char.IsUpper(textSpan[i]))
                inserts++;
        }
        
        var result = new Span<char>(new char[text.Length + inserts]);
        var resultIndex = 0;
        
        for (var i = 0; i < text.Length; i++)
        {
            if (i != 0 &&  char.IsUpper(textSpan[i]))
            {
                result[resultIndex] = '_';
                resultIndex++;
            }
            result[resultIndex] = char.ToLower(textSpan[i]);
            resultIndex++;
        }
        
        return result.ToString();
    }
}