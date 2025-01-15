namespace VndbCharacterNames;

internal sealed record class NameRecord(string Name, string NameInRomaji)
{
#pragma warning disable IDE0051
    // ReSharper disable once UnusedMember.Local
    private string Name { get; } = Name;
    // ReSharper disable once UnusedMember.Local
    private string NameInRomaji { get; } = NameInRomaji;
#pragma warning restore IDE0051
}
