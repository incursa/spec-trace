using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpecTrace.Rfc.Core;

public static class RfcJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNamingPolicy = null,
        WriteIndented = true,
    };

    public static readonly JsonSerializerOptions JsonlOptions = new(Options)
    {
        WriteIndented = false,
    };
}
