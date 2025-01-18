using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace VndbCharacterNames;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable once ClassNeverInstantiated.Global
#pragma warning disable CA1812 // Internal class that is apparently never instantiated
internal sealed class VndbNameRecord(string fullName,
    string fullNameInRomaji,
    string[] visualNovelTitles,
    string[]? aliases,
    string? bloodType,
    string? cupSize,
    string? sex,
    int? bust,
    int? waist,
    int? hip,
    int? birthMonth,
    int? birthDay,
    int? height,
    int? weight,
    int? age)
#pragma warning restore CA1812 // Internal class that is apparently never instantiated
{
    [JsonPropertyName("Full Name")]
    public string FullName { get; } = fullName;

    [JsonPropertyName("Full Name in Romaji")]
    public string FullNameInRomaji { get; } = fullNameInRomaji;

    [JsonPropertyName("Visual Novel Titles")]
    public string[] VisualNovelTitles { get; } = visualNovelTitles;

    [JsonPropertyName("Aliases")]
    public string[]? Aliases { get; } = aliases;

    [JsonPropertyName("Blood Type")]
    public string? BloodType { get; } = bloodType;

    [JsonPropertyName("Cup Size")]
    public string? CupSize { get; } = cupSize;

    [JsonPropertyName("Sex")]
    public string? Sex { get; } = sex;

    [JsonPropertyName("Bust")]
    public int? Bust { get; } = bust;

    [JsonPropertyName("Waist")]
    public int? Waist { get; } = waist;

    [JsonPropertyName("Hip")]
    public int? Hip { get; } = hip;

    [JsonPropertyName("Birth Month")]
    public int? BirthMonth { get; } = birthMonth;

    [JsonPropertyName("Birth Day")]
    public int? BirthDay { get; } = birthDay;

    [JsonPropertyName("Height")]
    public int? Height { get; } = height;

    [JsonPropertyName("Weight")]
    public int? Weight { get; } = weight;

    [JsonPropertyName("Age")]
    public int? Age { get; } = age;

    private string? GetBirthday()
    {
        if (BirthMonth is null && BirthDay is null)
        {
            return null;
        }

        string birthMonth = BirthMonth is not null
            ? $"{BirthMonth}月"
            : "";

        string birthDay = BirthDay is not null
            ? $"{BirthDay}日"
            : "";

        return $"Birthday: {birthMonth}{birthDay}";
    }

    private string? GetThreeSizes()
    {
        return Bust is null && Waist is null && Hip is null
            ? null
            : $"B/W/H: {Bust?.ToString(CultureInfo.InvariantCulture) ?? "?"}/{Waist?.ToString(CultureInfo.InvariantCulture) ?? "?"}/{Hip?.ToString(CultureInfo.InvariantCulture) ?? "?"}";
    }

    public string GetDefinition()
    {
        StringBuilder definitionStringBuilder = new();
        _ = definitionStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Full name: {FullName} ({FullNameInRomaji})");

        if (Sex is not null)
        {
            _ = definitionStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Sex: {Sex}");
        }

        string? birthday = GetBirthday();
        if (Age is not null)
        {
            birthday = birthday is not null
                ? $", {birthday}"
                : "";

            _ = definitionStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Age: {Age}{birthday}");
        }
        else if (birthday is not null)
        {
            _ = definitionStringBuilder.AppendLine(birthday);
        }

        if (Height is not null)
        {
            string weight = Weight is not null
                ? $", Weight: {Weight}"
                : "";

            _ = definitionStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Height: {Height}{weight}");
        }
        else if (Weight is not null)
        {
            _ = definitionStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Weight: {Weight}");
        }

        string? threeSizes = GetThreeSizes();
        if (threeSizes is not null)
        {
            string cupSize = CupSize is not null
                ? $", Cup size: {CupSize}"
                : "";

            _ = definitionStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{threeSizes}{cupSize}");
        }
        else if (CupSize is not null)
        {
            _ = definitionStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Cup size: {CupSize}");
        }

        if (BloodType is not null)
        {
            _ = definitionStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"Blood type: {BloodType}");
        }

        if (Aliases is not null)
        {
            string aliases = Aliases.Length > 1
                ? $"Aliases: {string.Join(", ", Aliases)}"
                : $"Alias: {Aliases[0]}";

            _ = definitionStringBuilder.AppendLine(aliases);
        }

        string vns = VisualNovelTitles.Length > 1
            ? $"VNs: {string.Join(", ", VisualNovelTitles)}"
            : $"VN: {VisualNovelTitles[0]}";
        _ = definitionStringBuilder.AppendLine(vns);

        return definitionStringBuilder.Length > 0
            ? definitionStringBuilder.ToString(0, definitionStringBuilder.Length - Environment.NewLine.Length)
            : FullNameInRomaji;
    }

    public List<NameRecord>? GetAliasPairs()
    {
        if (Aliases is null)
        {
            return null;
        }

        List<NameRecord> aliasRecords = new(Aliases.Length / 2);
        foreach (string alias in Aliases)
        {
            Match match = Utils.NameInParentheses.Match(alias);
            if (match.Success)
            {
                string firstMatch = match.Groups[1].Value;
                string secondMatch = match.Groups[2].Value;
                if (Utils.JapaneseRegex.IsMatch(firstMatch) && Utils.LatinRegex.IsMatch(secondMatch))
                {
                    aliasRecords.Add(new NameRecord(firstMatch, secondMatch));
                }
                else if (Utils.JapaneseRegex.IsMatch(secondMatch) && Utils.LatinRegex.IsMatch(firstMatch))
                {
                    aliasRecords.Add(new NameRecord(secondMatch, firstMatch));
                }
            }
        }

        if (aliasRecords.Count > 0)
        {
            return aliasRecords;
        }

        if (Aliases.Length % 2 is not 0)
        {
            return null;
        }

        bool evenNumberedAliasesShouldBeJapanese = Utils.JapaneseRegex.IsMatch(Aliases[0]);
        bool hasOnlyValidAliasPairs = true;
        for (int i = 0; i < Aliases.Length; i += 2)
        {
            if ((evenNumberedAliasesShouldBeJapanese && Utils.JapaneseRegex.IsMatch(Aliases[i]) && Utils.LatinRegex.IsMatch(Aliases[i + 1]))
                || (!evenNumberedAliasesShouldBeJapanese && Utils.LatinRegex.IsMatch(Aliases[i]) && Utils.JapaneseRegex.IsMatch(Aliases[i + 1])))
            {
                string name;
                string nameInRomaji;
                if (evenNumberedAliasesShouldBeJapanese)
                {
                    name = Aliases[i];
                    nameInRomaji = Aliases[i + 1];
                }
                else
                {
                    nameInRomaji = Aliases[i];
                    name = Aliases[i + 1];
                }

                aliasRecords.Add(new NameRecord(name, nameInRomaji));
            }
            else
            {
                hasOnlyValidAliasPairs = false;
                break;
            }
        }

        return hasOnlyValidAliasPairs && aliasRecords.Count > 0
            ? aliasRecords
            : null;
    }
}
