namespace VndbCharacterNames;

internal sealed class ConvertedNameRecord(string primarySpelling, string reading, string? nameType, string? definition, string? characterImagePath) : IEquatable<ConvertedNameRecord>
{
    public string PrimarySpelling { get; } = primarySpelling;
    public string Reading { get; } = reading;
    public string? NameType { get; } = nameType;
    public string? Definition { get; } = definition;
    public string? CharacterImagePath { get; set; } = characterImagePath;

    public bool Equals(ConvertedNameRecord? other)
    {
        return other is not null
            && other.PrimarySpelling == PrimarySpelling
            && other.Reading == Reading
            && other.NameType == NameType
            && other.Definition == Definition;
    }

    public override bool Equals(object? obj)
    {
        return obj is ConvertedNameRecord other
            && other.PrimarySpelling == PrimarySpelling
            && other.Reading == Reading
            && other.NameType == NameType
            && other.Definition == Definition;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PrimarySpelling.GetHashCode(StringComparison.Ordinal),
            Reading.GetHashCode(StringComparison.Ordinal),
            NameType?.GetHashCode(StringComparison.Ordinal),
            Definition?.GetHashCode(StringComparison.Ordinal));
    }

    public static bool operator ==(ConvertedNameRecord? left, ConvertedNameRecord? right) => left?.Equals(right) ?? (right is null);
    public static bool operator !=(ConvertedNameRecord? left, ConvertedNameRecord? right) => !(left == right);
}
