using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace VndbCharacterNames;

internal static partial class Program
{
    private const string SurnameNameType = "Surname";
    private const string OtherNameType = "other";

    private static readonly JsonSerializerOptions s_jso = new()
    {
        RespectNullableAnnotations = true,
        RespectRequiredConstructorParameters = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    [GeneratedRegex(@"^[\u30A0-\u30FF]+$", RegexOptions.CultureInvariant)]
    private static partial Regex KatakanaRegex { get; }

    [GeneratedRegex(@"^Full name:.*\n((Sex:.*\n)|)", RegexOptions.CultureInvariant)]
    private static partial Regex FullNameAndSexRegex { get; }

    [GeneratedRegex(@"[\u00D7\u2000-\u206F\u25A0-\u25FF\u2E80-\u319F\u31C0-\u4DBF\u4E00-\u9FFF\uF900-\uFAFF\uFE30-\uFE4F\uFF00-\uFFEF]|\uD82C[\uDC00-\uDD6F]|\uD83C[\uDE00-\uDEFF]|\uD840[\uDC00-\uDFFF]|[\uD841-\uD868][\uDC00-\uDFFF]|\uD869[\uDC00-\uDEDF]|\uD869[\uDF00-\uDFFF]|[\uD86A-\uD87A][\uDC00-\uDFFF]|\uD87B[\uDC00-\uDE5F]|\uD87E[\uDC00-\uDE1F]|\uD880[\uDC00-\uDFFF]|[\uD881-\uD887][\uDC00-\uDFFF]|\uD888[\uDC00-\uDFAF]", RegexOptions.CultureInvariant)]
    public static partial Regex JapaneseRegex { get; }

    [GeneratedRegex(@"[\u0000-\u024F\u1E00-\u1EFF\u2C60-\u2C7F]", RegexOptions.CultureInvariant)]
    public static partial Regex LatinRegex { get; }

    private static void AddItemToDictionary(Dictionary<NameRecord, List<string>> dict, NameRecord nameRecord, string nameType)
    {
        if (dict.TryGetValue(nameRecord, out List<string>? nameTypes))
        {
            if (!nameTypes.Contains(nameType))
            {
                nameTypes.Add(nameType);
            }
        }
        else
        {
            dict[nameRecord] = [nameType];
        }
    }

    public static void Main(string[] args)
    {
        string? outputFilePath = null;
        bool validArgs = false;
        List<string>? jsonFiles = null;
        if (args.Length is 2)
        {
            bool valid = true;
            string jsonFolderPath = args[0].Trim('"', ' ');
            if (!Directory.Exists(jsonFolderPath))
            {
                Console.WriteLine("The folder specified does not exist!");
                valid = false;
            }
            else
            {
                jsonFiles = Directory.EnumerateFiles(jsonFolderPath, "*.json", SearchOption.TopDirectoryOnly).ToList();
                if (jsonFiles.Count is 0)
                {
                    Console.WriteLine("There's no JSON file in the specified folder!");
                    valid = false;
                }
            }

            outputFilePath = args[1].Trim('"', ' ');
            if (!Path.IsPathFullyQualified(outputFilePath))
            {
                Console.WriteLine("Invalid file path!");
                valid = false;
            }

            validArgs = valid;
            if (validArgs)
            {
                outputFilePath = Path.ChangeExtension(outputFilePath, "json");
            }
        }

        bool validInputs = false;
        while (!validArgs && !validInputs)
        {
            Console.WriteLine("Please enter the path of the folder where JSON file(s) are placed");
            string? jsonFolderPath = Console.ReadLine()?.Trim('"', ' ');
            if (!Directory.Exists(jsonFolderPath))
            {
                Console.WriteLine("The folder specified does not exist!");
            }
            else
            {
                jsonFiles = Directory.EnumerateFiles(jsonFolderPath, "*.json", SearchOption.TopDirectoryOnly).ToList();
                if (jsonFiles.Count is 0)
                {
                    Console.WriteLine("There's no JSON file in the specified folder!");
                }
                else
                {
                    validInputs = true;
                }
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

        Dictionary<NameRecord, List<string>> nameTypesDict = [];
        HashSet<ConvertedNameRecord> convertedRecords = [];

        int validJsonFileCount = 0;
        long totalVndbNameRecordCount = 0;
        foreach (string jsonFile in jsonFiles!)
        {
            using FileStream fileStream = File.OpenRead(jsonFile);
            List<VndbNameRecord>? vndbNameRecords;
            try
            {
                vndbNameRecords = JsonSerializer.Deserialize<List<VndbNameRecord>>(fileStream, s_jso)!;
                ++validJsonFileCount;
                totalVndbNameRecordCount += vndbNameRecords.Count;
            }
            catch (JsonException)
            {
                Console.WriteLine($"{jsonFile} doesn't appear to be in the expected format! Next time please consider putting the related JSON file(s) into an empty folder.");
                continue;
            }

            foreach (VndbNameRecord vndbNameRecord in vndbNameRecords)
            {
                string definition = vndbNameRecord.GetDefinition();
                string fullNameWithoutAnyWhiteSpace = string.Join("", vndbNameRecord.FullName.Split());

                if (vndbNameRecord.Sex is not null)
                {
                    AddItemToDictionary(nameTypesDict, new NameRecord(fullNameWithoutAnyWhiteSpace, vndbNameRecord.FullNameInRomaji), vndbNameRecord.Sex);
                }

                _ = convertedRecords.Add(new ConvertedNameRecord(fullNameWithoutAnyWhiteSpace, vndbNameRecord.FullNameInRomaji, vndbNameRecord.Sex, definition));
                string[] splitRomajiParts = vndbNameRecord.FullNameInRomaji.Split((string[]?)null, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

                string? surname = null;
                string? surnameInRomaji = null;
                string? givenName = null;
                string? givenNameInRomaji = null;
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
                                surname = splitFullNameParts[0];
                                surnameInRomaji = splitRomajiParts[0];
                                givenName = splitFullNameParts[1];
                                givenNameInRomaji = splitRomajiParts[1];

                                if (KatakanaRegex.IsMatch(surname))
                                {
                                    (surname, givenName) = (givenName, surname);
                                    (surnameInRomaji, givenNameInRomaji) = (givenNameInRomaji, surnameInRomaji);
                                }
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
                                givenName = splitFullNameParts[0];
                                givenNameInRomaji = splitRomajiParts[0];
                                surname = splitFullNameParts[1];
                                surnameInRomaji = splitRomajiParts[1];

                                if (!KatakanaRegex.IsMatch(surname) && !KatakanaRegex.IsMatch(givenName))
                                {
                                    (givenName, surname) = (surname, givenName);
                                    (givenNameInRomaji, surnameInRomaji) = (surnameInRomaji, givenNameInRomaji);
                                }
                            }
                        }
                    }
                }

                if (givenName is not null && givenNameInRomaji is not null && surname is not null && surnameInRomaji is not null)
                {
                    NameRecord surnameAndRomaji = new(surname, surnameInRomaji);
                    AddItemToDictionary(nameTypesDict, surnameAndRomaji, SurnameNameType);

                    if (vndbNameRecord.Sex is not null)
                    {
                        NameRecord givenNameAndRomajiRecord = new(givenName, givenNameInRomaji);
                        AddItemToDictionary(nameTypesDict, givenNameAndRomajiRecord, vndbNameRecord.Sex);
                    }

                    _ = convertedRecords.Add(new ConvertedNameRecord(surname, surnameInRomaji));
                    _ = convertedRecords.Add(new ConvertedNameRecord(givenName, givenNameInRomaji));

                    List<NameRecord>? aliasRecords = vndbNameRecord.GetAliasPairs();
                    if (aliasRecords is not null)
                    {
                        foreach (NameRecord aliasRecord in aliasRecords)
                        {
                            if (surname != aliasRecord.Name && givenName != aliasRecord.Name)
                            {
                                _ = convertedRecords.Add(new ConvertedNameRecord(aliasRecord.Name, aliasRecord.NameInRomaji, vndbNameRecord.Sex, definition));
                            }
                        }
                    }
                }
                else
                {
                    List<NameRecord>? aliasRecords = vndbNameRecord.GetAliasPairs();
                    if (aliasRecords is not null)
                    {
                        foreach (NameRecord aliasRecord in aliasRecords)
                        {
                            _ = convertedRecords.Add(new ConvertedNameRecord(aliasRecord.Name, aliasRecord.NameInRomaji, vndbNameRecord.Sex, definition));
                        }
                    }
                }
            }
        }

        if (convertedRecords.Count > 0)
        {
            string customNamePath = Path.Join(Path.GetDirectoryName(outputFilePath), $"{Path.GetFileNameWithoutExtension(outputFilePath)}_Custom_Names.txt");

            JsonArray nazekaJsonArray = ["VNDB Names"];
            List<string> lines = [];
            foreach (ConvertedNameRecord record in convertedRecords)
            {
#pragma warning disable CA1308
                string? nameType = record.NameType ?? (nameTypesDict.TryGetValue(new NameRecord(record.PrimarySpelling, record.Reading), out List<string>? nameTypes)
                    ? string.Join(", ", nameTypes).ToLowerInvariant()
                    : null);
#pragma warning restore CA1308

                string? definitionForCustomNameFile = record.Definition is not null
                    ? FullNameAndSexRegex.Replace(record.Definition, "").Replace("\t", "  ", StringComparison.Ordinal).ReplaceLineEndings("\\n")
                    : null;

                string line = $"{record.PrimarySpelling}\t{record.Reading}\t{nameType ?? OtherNameType}\t{definitionForCustomNameFile}";
                lines.Add(line);

                string definitionForNazeka = record.Definition ?? (nameType is not null
                    ? $"({nameType}) {record.Reading}"
                    : record.Reading);

                JsonArray nazekaSpellingsArray = [record.PrimarySpelling];
                JsonArray nazekaDefinitionsArray = [definitionForNazeka];
                nazekaJsonArray.Add(new JsonObject
                {
                    ["r"] = record.Reading,
                    ["s"] = nazekaSpellingsArray,
                    ["l"] = nazekaDefinitionsArray
                });
            }

            File.WriteAllLines(customNamePath, lines);
            File.WriteAllText(outputFilePath!, JsonSerializer.Serialize(nazekaJsonArray, s_jso));
            Console.WriteLine($"Successfully created {outputFilePath} and {customNamePath}!");

            if (validJsonFileCount is 1 && totalVndbNameRecordCount is 100_000)
            {
                Console.WriteLine("You provided a single valid JSON file with exactly 100000 records. This probably means that the result of your query was truncated by VNDB Query.");
            }

            Console.WriteLine("Press any key to exit...");
            _ = Console.ReadKey();
        }
        else
        {
            Console.WriteLine("There is no file in the expected format in the folder specified!");
            Main(args);
        }
    }
}
