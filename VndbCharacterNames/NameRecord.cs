namespace VndbCharacterNames;

internal sealed record class NameRecord(string Name, string NameInRomaji)
{
    public string Name { get; } = Name;
    public string NameInRomaji { get; } = NameInRomaji;
}
