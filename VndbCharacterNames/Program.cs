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
        foreach (string jsonFile in jsonFiles!)
        {
            using FileStream fileStream = File.OpenRead(jsonFile);
            List<VndbNameRecord>? vndbNameRecords;
            try
            {
                vndbNameRecords = JsonSerializer.Deserialize<List<VndbNameRecord>>(fileStream, s_jso)!;
            }
            catch (JsonException)
            {
                Console.WriteLine($"{jsonFile} doesn't appear to be in the expected format! Please consider putting the related JSON file(s) into ");
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
                                AddItemToDictionary(nameTypesDict, surnameAndRomaji, SurnameNameType);

                                if (vndbNameRecord.Sex is not null)
                                {
                                    NameRecord givenNameAndRomajiRecord = new(givenName, givenNameInRomaji);
                                    AddItemToDictionary(nameTypesDict, givenNameAndRomajiRecord, vndbNameRecord.Sex);
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
                                AddItemToDictionary(nameTypesDict, surnameAndRomaji, SurnameNameType);

                                if (vndbNameRecord.Sex is not null)
                                {
                                    NameRecord givenNameAndRomajiRecord = new(givenName, givenNameInRomaji);
                                    AddItemToDictionary(nameTypesDict, givenNameAndRomajiRecord, vndbNameRecord.Sex);
                                }

                                _ = convertedRecords.Add(new ConvertedNameRecord(surname, surnameInRomaji));
                                _ = convertedRecords.Add(new ConvertedNameRecord(givenName, givenNameInRomaji));
                            }
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
                string? nameType = nameTypesDict.TryGetValue(new NameRecord(record.PrimarySpelling, record.Reading), out List<string>? nameTypes)
                    ? string.Join(", ", nameTypes)
                : null;

                string? definitionForCustomNameFile = record.Definition is not null
                    ? FullNameAndSexRegex.Replace(record.Definition, "").Replace("\t", "  ", StringComparison.Ordinal).ReplaceLineEndings("\\n")
                    : null;

#pragma warning disable CA1308
                string line = $"{record.PrimarySpelling}\t{record.Reading}\t{nameType?.ToLowerInvariant() ?? OtherNameType}\t{definitionForCustomNameFile}";
#pragma warning restore CA1308

                lines.Add(line);

                string definitionForNazeka = record.Definition is not null
                    ? record.Definition
                    : nameType is not null
                        ? $"({nameType}) {record.Reading}"
                        : record.Reading;

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
        }
        else
        {
            Console.WriteLine("There is no file in the expected format in the folder specified!");
            Main(args);
        }
    }
}
