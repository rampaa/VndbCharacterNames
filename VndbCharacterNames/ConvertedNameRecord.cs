namespace VndbCharacterNames;

internal sealed record class ConvertedNameRecord(string PrimarySpelling, string Reading, string? NameType = null, string? Definition = null)
{
    public string PrimarySpelling { get; } = PrimarySpelling;
    public string Reading { get; } = Reading;
    public string? NameType { get; } = NameType;
    public string? Definition { get; } = Definition;
}
