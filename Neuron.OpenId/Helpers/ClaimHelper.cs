using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Neuron.OpenId.Helpers;

public static class ClaimHelper
{
    public static Claim FromDictionary(string type, Dictionary<string, string> value, string issuer = ClaimsIdentity.DefaultIssuer)
    {
        if (string.IsNullOrEmpty(type))
            throw new ArgumentNullException(nameof(type));
        
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Indented = false
        });

        writer.WriteStartObject();

        foreach (var property in value)
        {
            writer.WritePropertyName(property.Key);
            writer.WriteStringValue(property.Value);
        }

        writer.WriteEndObject();
        writer.Flush();

        return new Claim(
            type: type,
            value: Encoding.UTF8.GetString(stream.ToArray()),
            valueType: "JSON",
            issuer: issuer,
            originalIssuer: issuer);

    }

    public static List<Claim> FromList(string type, IList<string> values, string issuer = ClaimsIdentity.DefaultIssuer)
    {
        if (string.IsNullOrEmpty(type))
            throw new ArgumentNullException(nameof(type));
        
        var result = new List<Claim>();
        foreach (var value in values.Distinct(StringComparer.Ordinal))
        {
            result.Add(new Claim(type: type, value: value, null, issuer));
        }

        return result;
    }
}