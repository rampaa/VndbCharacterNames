# How to use it?

You need the character data from VNDB in the format that can be found in [this link](https://query.vndb.org/94589cbf0ce9d1d2/q). That query is for my personal use only, as it arbitrarily filters out characters based on the visual novels they appear in. So feel free to copy and edit it as you see fit. You should export the result as a JSON file by pressing EXPORT -> JSON.

After getting the necessary JSON file, you can simply run the file through Command Prompt like this:

`VndbCharacterNames.exe <Path of the VNDB character data file> <Output file path>`

e.g.,

`VndbCharacterNames.exe C:\Users\User\query-2025-01-15T102233.json C:\Users\User\Desktop\JL\Dicts\VndbCharacterNames.json`

Or you can double-click `VndbCharacterNames.exe` and give those paths when prompted.

The file is created in Nazeka EPWING Converter format, so you can use it with Nazeka and [JL](https://github.com/rampaa/JL). If you plan to use it with JL, make sure to select `Name Dictionary (Nazeka)` as your dictionary type.
