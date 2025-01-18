using System.Text.Json;
using System.Text.Json.Nodes;

namespace VndbCharacterNames;

file static class Program
{
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
                vndbNameRecords = JsonSerializer.Deserialize<List<VndbNameRecord>>(fileStream, Utils.Jso)!;
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
                    nameTypesDict.AddIfNotExists(new NameRecord(fullNameWithoutAnyWhiteSpace, vndbNameRecord.FullNameInRomaji), vndbNameRecord.Sex);
                }

                _ = convertedRecords.Add(new ConvertedNameRecord(fullNameWithoutAnyWhiteSpace, vndbNameRecord.FullNameInRomaji, vndbNameRecord.Sex, definition));
                (NameRecord surnameRecord, NameRecord givenNameRecord)? surnameAndNameRecords = GetSurnameAndNameRecords(vndbNameRecord.FullName, vndbNameRecord.FullNameInRomaji);
                if (surnameAndNameRecords is not null)
                {
                    NameRecord surnameRecord = surnameAndNameRecords.Value.surnameRecord;
                    NameRecord givenNameRecord = surnameAndNameRecords.Value.surnameRecord;

                    nameTypesDict.AddIfNotExists(surnameRecord, Utils.SurnameNameType);

                    if (vndbNameRecord.Sex is not null)
                    {
                        nameTypesDict.AddIfNotExists(givenNameRecord, vndbNameRecord.Sex);
                    }

                    _ = convertedRecords.Add(new ConvertedNameRecord(surnameRecord.Name, surnameRecord.NameInRomaji));
                    _ = convertedRecords.Add(new ConvertedNameRecord(givenNameRecord.Name, givenNameRecord.NameInRomaji));

                    List<NameRecord>? aliasRecords = vndbNameRecord.GetAliasPairs();
                    if (aliasRecords is not null)
                    {
                        foreach (NameRecord aliasRecord in aliasRecords)
                        {
                            if (surnameRecord.Name != aliasRecord.Name && givenNameRecord.Name != aliasRecord.Name)
                            {
                                string fullAliasWithoutAnyWhiteSpace = string.Join("", aliasRecord.Name.Split());
                                if (vndbNameRecord.Sex is not null)
                                {
                                    nameTypesDict.AddIfNotExists(new NameRecord(fullAliasWithoutAnyWhiteSpace, aliasRecord.NameInRomaji), vndbNameRecord.Sex);
                                }

                                _ = convertedRecords.Add(new ConvertedNameRecord(fullAliasWithoutAnyWhiteSpace, aliasRecord.NameInRomaji, vndbNameRecord.Sex, definition));
                                (NameRecord surnameRecord, NameRecord givenNameRecord)? aliasSurnameAndNameRecords = GetSurnameAndNameRecords(aliasRecord.Name, aliasRecord.NameInRomaji);
                                if (aliasSurnameAndNameRecords is not null)
                                {
                                    NameRecord aliasSurnameRecord = aliasSurnameAndNameRecords.Value.surnameRecord;
                                    NameRecord aliasGivenNameRecord = aliasSurnameAndNameRecords.Value.surnameRecord;

                                    if (aliasSurnameRecord.Name != surnameRecord.Name)
                                    {
                                        nameTypesDict.AddIfNotExists(aliasSurnameRecord, Utils.SurnameNameType);
                                        _ = convertedRecords.Add(new ConvertedNameRecord(aliasSurnameRecord.Name, aliasSurnameRecord.NameInRomaji));
                                    }

                                    if (aliasGivenNameRecord.Name != givenNameRecord.Name)
                                    {
                                        if (vndbNameRecord.Sex is not null)
                                        {
                                            nameTypesDict.AddIfNotExists(aliasGivenNameRecord, vndbNameRecord.Sex);
                                        }

                                        _ = convertedRecords.Add(new ConvertedNameRecord(aliasGivenNameRecord.Name, aliasGivenNameRecord.NameInRomaji));
                                    }
                                }
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
                            string fullAliasWithoutAnyWhiteSpace = string.Join("", aliasRecord.Name.Split());
                            if (vndbNameRecord.Sex is not null)
                            {
                                nameTypesDict.AddIfNotExists(new NameRecord(fullAliasWithoutAnyWhiteSpace, aliasRecord.NameInRomaji), vndbNameRecord.Sex);
                            }

                            _ = convertedRecords.Add(new ConvertedNameRecord(fullAliasWithoutAnyWhiteSpace, aliasRecord.NameInRomaji, vndbNameRecord.Sex, definition));
                            (NameRecord surnameRecord, NameRecord givenNameRecord)? aliasSurnameAndNameRecords = GetSurnameAndNameRecords(aliasRecord.Name, aliasRecord.NameInRomaji);
                            if (aliasSurnameAndNameRecords is not null)
                            {
                                NameRecord aliasSurnameRecord = aliasSurnameAndNameRecords.Value.surnameRecord;
                                NameRecord aliasGivenNameRecord = aliasSurnameAndNameRecords.Value.surnameRecord;

                                nameTypesDict.AddIfNotExists(aliasSurnameRecord, Utils.SurnameNameType);
                                _ = convertedRecords.Add(new ConvertedNameRecord(aliasSurnameRecord.Name, aliasSurnameRecord.NameInRomaji));

                                if (vndbNameRecord.Sex is not null)
                                {
                                    nameTypesDict.AddIfNotExists(aliasGivenNameRecord, vndbNameRecord.Sex);
                                }
                                _ = convertedRecords.Add(new ConvertedNameRecord(aliasGivenNameRecord.Name, aliasGivenNameRecord.NameInRomaji));
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
#pragma warning disable CA1308
                string? nameType = record.NameType ?? (nameTypesDict.TryGetValue(new NameRecord(record.PrimarySpelling, record.Reading), out List<string>? nameTypes)
                    ? string.Join(", ", nameTypes).ToLowerInvariant()
                    : null);
#pragma warning restore CA1308

                string? definitionForCustomNameFile = record.Definition is not null
                    ? Utils.FullNameAndSexRegex.Replace(record.Definition, "").Replace("\t", "  ", StringComparison.Ordinal).ReplaceLineEndings("\\n")
                    : null;

                string line = $"{record.PrimarySpelling}\t{record.Reading}\t{nameType ?? Utils.OtherNameType}\t{definitionForCustomNameFile}";
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
            File.WriteAllText(outputFilePath!, JsonSerializer.Serialize(nazekaJsonArray, Utils.Jso));
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

    private static (NameRecord surnameRecord, NameRecord givenNameRecord)? GetSurnameAndNameRecords(string fullName, string fullNameInRomaji)
    {
        string[] splitRomajiParts = fullNameInRomaji.Split((string[]?)null, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        string? surname = null;
        string? surnameInRomaji = null;
        string? givenName = null;
        string? givenNameInRomaji = null;
        if (splitRomajiParts.Length > 1)
        {
            string[] splitFullNameParts = fullName.Split((string[]?)null, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (splitFullNameParts.Length is 1)
            {
                splitFullNameParts = fullName.Split(['＝', '='], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
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

                        if (Utils.KatakanaRegex.IsMatch(surname))
                        {
                            (surname, givenName) = (givenName, surname);
                            (surnameInRomaji, givenNameInRomaji) = (givenNameInRomaji, surnameInRomaji);
                        }
                    }
                }
            }
            else
            {
                splitFullNameParts = fullName.Split(['・', '・', '･'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (splitRomajiParts.Length == splitFullNameParts.Length)
                {
                    if (splitFullNameParts.Length is 2)
                    {
                        givenName = splitFullNameParts[0];
                        givenNameInRomaji = splitRomajiParts[0];
                        surname = splitFullNameParts[1];
                        surnameInRomaji = splitRomajiParts[1];

                        if (!Utils.KatakanaRegex.IsMatch(surname) && !Utils.KatakanaRegex.IsMatch(givenName))
                        {
                            (givenName, surname) = (surname, givenName);
                            (givenNameInRomaji, surnameInRomaji) = (surnameInRomaji, givenNameInRomaji);
                        }
                    }
                }
            }
        }

        return givenName is not null && givenNameInRomaji is not null && surname is not null && surnameInRomaji is not null
            ? (surnameRecord: new NameRecord(surname, surnameInRomaji), givenNameRecord: new NameRecord(givenName, givenNameInRomaji))
            : null;
    }
}
