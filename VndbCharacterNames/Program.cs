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
                List<(NameRecord surnameRecord, NameRecord givenNameRecord)>? surnameAndNameRecords = GetSurnameAndNameRecords(vndbNameRecord.FullName, vndbNameRecord.FullNameInRomaji);
                if (surnameAndNameRecords is not null)
                {
                    foreach ((NameRecord surnameRecord, NameRecord givenNameRecord) in surnameAndNameRecords)
                    {
                        nameTypesDict.AddIfNotExists(surnameRecord, Utils.SurnameNameType);

                        if (vndbNameRecord.Sex is not null)
                        {
                            nameTypesDict.AddIfNotExists(givenNameRecord, vndbNameRecord.Sex);
                        }

                        _ = convertedRecords.Add(new ConvertedNameRecord(surnameRecord.Name, surnameRecord.NameInRomaji));
                        _ = convertedRecords.Add(new ConvertedNameRecord(givenNameRecord.Name, givenNameRecord.NameInRomaji));
                    }

                    List<NameRecord>? aliasRecords = vndbNameRecord.GetAliasPairs();
                    if (aliasRecords is not null)
                    {
                        string[] surnames = surnameAndNameRecords.Select(static r => r.surnameRecord.Name).ToArray();
                        string[] givenNames = surnameAndNameRecords.Select(static r => r.givenNameRecord.Name).ToArray();
                        foreach (NameRecord aliasRecord in aliasRecords)
                        {
                            if (!surnames.Contains(aliasRecord.Name) && !givenNames.Contains(aliasRecord.Name))
                            {
                                string fullAliasWithoutAnyWhiteSpace = string.Join("", aliasRecord.Name.Split());
                                if (vndbNameRecord.Sex is not null)
                                {
                                    nameTypesDict.AddIfNotExists(new NameRecord(fullAliasWithoutAnyWhiteSpace, aliasRecord.NameInRomaji), vndbNameRecord.Sex);
                                }

                                _ = convertedRecords.Add(new ConvertedNameRecord(fullAliasWithoutAnyWhiteSpace, aliasRecord.NameInRomaji, vndbNameRecord.Sex, definition));
                                List<(NameRecord surnameRecord, NameRecord givenNameRecord)>? aliasSurnameAndNameRecords = GetSurnameAndNameRecords(aliasRecord.Name, aliasRecord.NameInRomaji);
                                if (aliasSurnameAndNameRecords is not null)
                                {
                                    string[] aliasSurnames = aliasSurnameAndNameRecords.Select(static r => r.surnameRecord.Name).ToArray();
                                    string[] aliasGivenNames = aliasSurnameAndNameRecords.Select(static r => r.givenNameRecord.Name).ToArray();
                                    foreach ((NameRecord aliasSurnameRecord, NameRecord aliasGivenNameRecord) in aliasSurnameAndNameRecords)
                                    {
                                        if (!aliasSurnames.Contains(aliasSurnameRecord.Name))
                                        {
                                            nameTypesDict.AddIfNotExists(aliasSurnameRecord, Utils.SurnameNameType);
                                            _ = convertedRecords.Add(new ConvertedNameRecord(aliasSurnameRecord.Name, aliasSurnameRecord.NameInRomaji));
                                        }

                                        if (!aliasGivenNames.Contains(aliasGivenNameRecord.Name))
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
                            List<(NameRecord surnameRecord, NameRecord givenNameRecord)>? aliasSurnameAndNameRecords = GetSurnameAndNameRecords(aliasRecord.Name, aliasRecord.NameInRomaji);
                            if (aliasSurnameAndNameRecords is not null)
                            {
                                string[] aliasSurnames = aliasSurnameAndNameRecords.Select(static r => r.surnameRecord.Name).ToArray();
                                string[] aliasGivenNames = aliasSurnameAndNameRecords.Select(static r => r.givenNameRecord.Name).ToArray();
                                foreach ((NameRecord aliasSurnameRecord, NameRecord aliasGivenNameRecord) in aliasSurnameAndNameRecords)
                                {
                                    if (!aliasSurnames.Contains(aliasSurnameRecord.Name))
                                    {
                                        nameTypesDict.AddIfNotExists(aliasSurnameRecord, Utils.SurnameNameType);
                                        _ = convertedRecords.Add(new ConvertedNameRecord(aliasSurnameRecord.Name, aliasSurnameRecord.NameInRomaji));
                                    }

                                    if (!aliasGivenNames.Contains(aliasGivenNameRecord.Name))
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

    private static List<(NameRecord surnameRecord, NameRecord givenNameRecord)>? GetSurnameAndNameRecords(string fullName, string fullNameInRomaji)
    {
        if (!Utils.JapaneseRegex.IsMatch(fullName))
        {
            return null;
        }

        string[] splitFullNameInRomajiParts = fullNameInRomaji.Split((string[]?)null, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (splitFullNameInRomajiParts.Length <= 1)
        {
            return null;
        }

        string[] splitFullNameParts = fullName.Split((string[]?)null, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (splitFullNameParts.Length is 1)
        {
            splitFullNameParts = fullName.Split(['＝', '=', '・', '・', '･'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        }

        if (splitFullNameParts.Length <= 1)
        {
            return null;
        }

        if (splitFullNameInRomajiParts.Length != splitFullNameParts.Length)
        {
            return null;
        }

        string firstFullNamePart = splitFullNameParts[0];
        string lastFullNamePart = splitFullNameParts[^1];
        bool firstFullNamePartIsKanjiOrHiragana = Utils.KanjiRegex.IsMatch(firstFullNamePart) || Utils.HiraganaRegex.IsMatch(firstFullNamePart);

        if ((!splitFullNameParts.Any(Utils.KatakanaRegex.IsMatch) && !splitFullNameParts.Any(Utils.LatinRegex.IsMatch))
            || (firstFullNamePartIsKanjiOrHiragana && (Utils.KanjiRegex.IsMatch(lastFullNamePart) || Utils.HiraganaRegex.IsMatch(lastFullNamePart)))
            || (splitFullNameParts.Length is 2 && firstFullNamePartIsKanjiOrHiragana))
        {
            List<(NameRecord surnameRecord, NameRecord givenNameRecord)> surnameAndNameRecords = new(splitFullNameParts.Length - 1);
            NameRecord surnameRecord = new(splitFullNameParts[0], splitFullNameInRomajiParts[0]);
            for (int i = 1; i < splitFullNameParts.Length; i++)
            {
                surnameAndNameRecords.Add((surnameRecord, new NameRecord(splitFullNameParts[i], splitFullNameInRomajiParts[i])));
            }

            return surnameAndNameRecords;
        }
        else //if (!Utils.KanjiRegex.IsMatch(splitFullNameParts[0])
             //        && (Utils.KatakanaRegex.IsMatch(splitFullNameParts[0])
             //            || Utils.LatinRegex.IsMatch(splitFullNameParts[0])))
        {
            NameRecord surnameRecord = new(splitFullNameParts[^1], splitFullNameInRomajiParts[^1]);
            bool hasMoreThanTwoNameParts = splitFullNameParts.Length > 2;
            int surnameIndex = FindSurnameStartIndex(splitFullNameParts, splitFullNameInRomajiParts);
            List<(NameRecord surnameRecord, NameRecord givenNameRecord)> surnameAndNameRecords = new(hasMoreThanTwoNameParts ? surnameIndex : surnameIndex - 1);
            for (int i = 0; i < surnameIndex; i++)
            {
                surnameAndNameRecords.Add((surnameRecord, new NameRecord(splitFullNameParts[i], splitFullNameInRomajiParts[i])));
            }

            if (hasMoreThanTwoNameParts)
            {
                int surnameLength = 0;
                int surnameInRomajiLength = 0;
                for (int i = surnameIndex; i < splitFullNameParts.Length; i++)
                {
                    surnameLength += splitFullNameParts[i].Length;
                    surnameInRomajiLength += splitFullNameInRomajiParts[i].Length;
                }

                int offset = splitFullNameParts.Length - surnameIndex - 1;
                int surnameStartIndexForFullName = fullName.Length - surnameLength - offset;
                int surnameStartIndexForFullNameInRomaji = fullNameInRomaji.Length - surnameInRomajiLength - offset;

                NameRecord fullSurnameRecord = new(string.Join("", fullName[surnameStartIndexForFullName..].Split()), fullNameInRomaji[surnameStartIndexForFullNameInRomaji..]);
                NameRecord fullGivenNameRecord = new(string.Join("", fullName[..(surnameStartIndexForFullName - 1)].Split()), fullNameInRomaji[..(surnameStartIndexForFullNameInRomaji - 1)]);
                surnameAndNameRecords.Add((fullSurnameRecord, fullGivenNameRecord));
            }

            return surnameAndNameRecords;
        }
    }

    private static int FindSurnameStartIndex(string[] splitFullNameParts, string[] splitFullNameInRomajiParts)
    {
        KeyValuePair<string, string[]>[] surnamePrefixes =
        [
            new("von", ["フォン", "ファン"]),
            new("van", ["ヴァン", "ファン"]),
            new("de", ["ド", "デ", "ダ"]),
            new("du", ["デュ"]),
            new("di", ["ディ"]),
            new("le", ["ル"]),
            new("la", ["ラ"])
        ];

        int index = splitFullNameParts.Length - 1;
        foreach (KeyValuePair<string, string[]> surnamePerfix in surnamePrefixes)
        {
            KeyValuePair<string, string[]> perfix = surnamePerfix;
            int tempIndex = Array.FindIndex(splitFullNameInRomajiParts, 1, splitFullNameParts.Length - 2, r => r.Equals(perfix.Key, StringComparison.OrdinalIgnoreCase));
            if (tempIndex > 0 && index > tempIndex && surnamePerfix.Value.Contains(splitFullNameParts[tempIndex]))
            {
                index = tempIndex;
            }
        }

        return index;
    }
}
