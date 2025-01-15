using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace VndbCharacterNames;

internal static partial class Program
{
    private const string SurnameClassName = "surname";

    private static readonly JsonSerializerOptions s_jso = new()
    {
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    [GeneratedRegex(@"^[\u30A0-\u30FF]+$", RegexOptions.CultureInvariant)]
    private static partial Regex KatakanaRegex { get; }

    private static void AddItemToDictionary(Dictionary<NameRecord, List<string>> dict, NameRecord nameRecord, string nameClass)
    {
        if (dict.TryGetValue(nameRecord, out List<string>? nameClasses))
        {
            if (!nameClasses.Contains(nameClass))
            {
                nameClasses.Add(nameClass);
            }
        }
        else
        {
            dict[nameRecord] = [nameClass];
        }
    }

    public static void Main(string[] args)
    {
        string? jsonFilePath = null;
        string? outputFilePath = null;
        bool validArgs = false;
        if (args.Length is 2)
        {
            jsonFilePath = args[0].Trim('"', ' ');
            if (!File.Exists(jsonFilePath))
            {
                Console.WriteLine("The file specified does not exist!");
            }

            outputFilePath = args[1].Trim('"', ' ');
            if (!Path.IsPathFullyQualified(outputFilePath))
            {
                Console.WriteLine("Invalid file path!");
            }

            outputFilePath = Path.ChangeExtension(outputFilePath, "json");

            validArgs = true;
        }

        bool validInputs = false;
        while (!validArgs && !validInputs)
        {
            Console.WriteLine("Please enter the path of the json file!");
            jsonFilePath = Console.ReadLine()?.Trim('"', ' ');
            if (!File.Exists(jsonFilePath))
            {
                Console.WriteLine("The file specified does not exist!");
            }
            else
            {
                validInputs = true;
            }
        }

        validInputs = false;
        while (!validArgs && !validInputs)
        {
            Console.WriteLine("Please enter the path to save the generated file, it must include its name as well.");
            outputFilePath = Console.ReadLine()?.Trim('"', ' ');
            if (outputFilePath is null || !Path.IsPathFullyQualified(outputFilePath))
            {
                Console.WriteLine("Invalid file path!");
            }
            else
            {
                outputFilePath = Path.ChangeExtension(outputFilePath, "json");
                validArgs = true;
            }
        }

        using FileStream fileStream = File.OpenRead(jsonFilePath!);
        List<VndbNameRecord>? vndbNameRecords = JsonSerializer.Deserialize<List<VndbNameRecord>>(fileStream, s_jso);
        if (vndbNameRecords is null)
        {
#pragma warning disable CA1303 // Do not pass literals as localized parameters
            Console.WriteLine("Can't load the file. Make sure that you've specified the correct path!");
#pragma warning restore CA1303 // Do not pass literals as localized parameters
            return;
        }

        Dictionary<NameRecord, List<string>> nameClassesDict = [];
        HashSet<ConvertedNameRecord> convertedRecords = [];
        foreach (VndbNameRecord vndbNameRecord in vndbNameRecords)
        {
            string definition = vndbNameRecord.GetDefinition();
            string fullNameWithoutAnyWhiteSpace = string.Join("", vndbNameRecord.FullName.Split());
            _ = convertedRecords.Add(new ConvertedNameRecord(fullNameWithoutAnyWhiteSpace, vndbNameRecord.FullNameInRomaji, definition));
            string[] splitRomajiParts = vndbNameRecord.FullNameInRomaji.Split((string[]?)null, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (splitRomajiParts.Length > 1)
            {
                string[] splitFullNameParts = vndbNameRecord.FullName.Split((string[]?)null, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (splitFullNameParts.Length is 1)
                {
                    splitFullNameParts = vndbNameRecord.FullName.Split(['＝', '='], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                }

                if (splitFullNameParts.Length > 1)
                {
                    if (splitRomajiParts.Length == splitFullNameParts.Length)
                    {
                        if (splitFullNameParts.Length is 2)
                        {
                            string surname = splitFullNameParts[0];
                            string surnameInRomaji = splitRomajiParts[0];
                            string givenName = splitFullNameParts[1];
                            string givenNameInRomaji = splitRomajiParts[1];

                            if (KatakanaRegex.IsMatch(surname))
                            {
                                (surname, givenName) = (givenName, surname);
                                (surnameInRomaji, givenNameInRomaji) = (givenNameInRomaji, surnameInRomaji);
                            }

                            NameRecord surnameAndRomaji = new(surname, surnameInRomaji);
                            AddItemToDictionary(nameClassesDict, surnameAndRomaji, SurnameClassName);

                            if (vndbNameRecord.Sex is not null)
                            {
                                NameRecord givenNameAndRomajiRecord = new(givenName, givenNameInRomaji);
                                AddItemToDictionary(nameClassesDict, givenNameAndRomajiRecord, vndbNameRecord.Sex);
                            }

                            _ = convertedRecords.Add(new ConvertedNameRecord(surname, surnameInRomaji));
                            _ = convertedRecords.Add(new ConvertedNameRecord(givenName, givenNameInRomaji));
                        }
                    }
                }
                else
                {
                    splitFullNameParts = vndbNameRecord.FullName.Split(['・', '・', '･'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                    if (splitRomajiParts.Length == splitFullNameParts.Length)
                    {
                        if (splitFullNameParts.Length is 2)
                        {
                            string givenName = splitFullNameParts[0];
                            string givenNameInRomaji = splitRomajiParts[0];
                            string surname = splitFullNameParts[1];
                            string surnameInRomaji = splitRomajiParts[1];

                            if (!KatakanaRegex.IsMatch(surname) && !KatakanaRegex.IsMatch(givenName))
                            {
                                (givenName, surname) = (surname, givenName);
                                (givenNameInRomaji, surnameInRomaji) = (surnameInRomaji, givenNameInRomaji);
                            }

                            NameRecord surnameAndRomaji = new(surname, surnameInRomaji);
                            AddItemToDictionary(nameClassesDict, surnameAndRomaji, SurnameClassName);

                            if (vndbNameRecord.Sex is not null)
                            {
                                NameRecord givenNameAndRomajiRecord = new(givenName, givenNameInRomaji);
                                AddItemToDictionary(nameClassesDict, givenNameAndRomajiRecord, vndbNameRecord.Sex);
                            }

                            _ = convertedRecords.Add(new ConvertedNameRecord(surname, surnameInRomaji));
                            _ = convertedRecords.Add(new ConvertedNameRecord(givenName, givenNameInRomaji));
                        }
                    }
                }
            }
        }

        if (convertedRecords.Count > 0)
        {
            JsonArray nazekaJsonArray = ["VNDB Names"];
            foreach (ConvertedNameRecord record in convertedRecords.ToArray())
            {
                record.Definition ??= nameClassesDict.TryGetValue(new NameRecord(record.PrimarySpelling, record.Reading),
                    out List<string>? nameClasses)
                    ? $"({string.Join(", ", nameClasses)}) {record.Reading}"
                    : record.Reading;

                JsonArray nazekaSpellingsArray = [record.PrimarySpelling];
                JsonArray nazekaDefinitionsArray = [record.Definition];
                nazekaJsonArray.Add(new JsonObject
                {
                    ["r"] = record.Reading,
                    ["s"] = nazekaSpellingsArray,
                    ["l"] = nazekaDefinitionsArray
                });
            }

            File.WriteAllText(outputFilePath!, JsonSerializer.Serialize(nazekaJsonArray, s_jso));
            Console.WriteLine($"Successfully created {outputFilePath}!");
        }
    }
}
