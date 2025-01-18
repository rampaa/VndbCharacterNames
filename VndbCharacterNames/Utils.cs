using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace VndbCharacterNames;
internal static partial class Utils
{
    public const string SurnameNameType = "Surname";
    public const string OtherNameType = "other";

    public static readonly JsonSerializerOptions Jso = new()
    {
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    [GeneratedRegex(@"^[\u30A0-\u30FF]+$", RegexOptions.CultureInvariant)]
    public static partial Regex KatakanaRegex { get; }

    [GeneratedRegex(@"^Full name:.*\n((Sex:.*\n)|)", RegexOptions.CultureInvariant)]
    public static partial Regex FullNameAndSexRegex { get; }

    [GeneratedRegex(@"[\u00D7\u2000-\u206F\u25A0-\u25FF\u2E80-\u319F\u31C0-\u4DBF\u4E00-\u9FFF\uF900-\uFAFF\uFE30-\uFE4F\uFF00-\uFFEF]|\uD82C[\uDC00-\uDD6F]|\uD83C[\uDE00-\uDEFF]|\uD840[\uDC00-\uDFFF]|[\uD841-\uD868][\uDC00-\uDFFF]|\uD869[\uDC00-\uDEDF]|\uD869[\uDF00-\uDFFF]|[\uD86A-\uD87A][\uDC00-\uDFFF]|\uD87B[\uDC00-\uDE5F]|\uD87E[\uDC00-\uDE1F]|\uD880[\uDC00-\uDFFF]|[\uD881-\uD887][\uDC00-\uDFFF]|\uD888[\uDC00-\uDFAF]", RegexOptions.CultureInvariant)]
    public static partial Regex JapaneseRegex { get; }

    [GeneratedRegex(@"[\u0000-\u024F\u1E00-\u1EFF\u2C60-\u2C7F]", RegexOptions.CultureInvariant)]
    public static partial Regex LatinRegex { get; }

    [GeneratedRegex(@"(\S+)\s*\(([^,]+?)\)", RegexOptions.CultureInvariant)]
    public static partial Regex NameInParentheses { get; }
}
