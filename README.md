# How to use it?

You need the character data from VNDB in the format that can be found in [this link](https://query.vndb.org/94589cbf0ce9d1d2/q). That query is for my personal use only, as it arbitrarily filters out characters based on the visual novels they appear in. So feel free to copy and edit it as you see fit. You should export the result as a JSON file by pressing EXPORT -> JSON. Note that VNDB Query service will truncate the results to the first 100000 rows. So you may need to use OFFSET and export the data multiple times depending on how many rows your query returns.

After getting the necessary JSON files, create a new folder and put those files inside it. After that you can simply run this tool through Command Prompt like this:

`VndbCharacterNames.exe <Path of folder you just created> <Output file path>`

e.g.,

`VndbCharacterNames.exe C:\Users\User\VndbExports C:\Users\User\Desktop\JL\Dicts\VndbCharacterNames`

Or you can double-click `VndbCharacterNames.exe` and give those paths when prompted.

This will create two files, {SpecifiedFileName}_Custom_Name.txt and {SpecifiedFileName}.json.

The former is in JL Custom Name format. Using it for full VNDB character data is not adviced. It's mostly useful when you want to generate a custom name dictionary for characters from a specific visual novel. You should rename it to {JLProfileNameForTheVN}_Custom_Name.txt and place it under ..\JL\Profiles.

The latter is in Nazeka EPWING Converter format, so you can use it with Nazeka and [JL](https://github.com/rampaa/JL). If you plan to use it with JL, make sure to select `Name Dictionary (Nazeka)` as your dictionary type.

<img src="https://github.com/user-attachments/assets/45ec7c54-b9e1-4a4b-acc5-8530aa48c73f" width="50%" height="50%">
