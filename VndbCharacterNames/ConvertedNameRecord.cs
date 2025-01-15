namespace VndbCharacterNames;

internal sealed record class ConvertedNameRecord(string PrimarySpelling, string Reading, string? Definition = null)
{
    public string PrimarySpelling { get; } = PrimarySpelling;
    public string Reading { get; } = Reading;
    public string? Definition { get; set; } = Definition;
}
