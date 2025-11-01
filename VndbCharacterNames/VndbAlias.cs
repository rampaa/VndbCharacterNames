using System.Text.Json.Serialization;

namespace VndbCharacterNames;

internal sealed record class VndbAlias(string Name, VndbSpoilerLevel SpoilerLevel, string? Latin = null)
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = Name;

    [JsonPropertyName("Spoiler Level")]
    public VndbSpoilerLevel SpoilerLevel { get; set; } = SpoilerLevel;

    [JsonPropertyName("Latin")]
    public string? Latin { get; } = Latin;
}
