# How to use it

## 1. Get character data from VNDB

You need character data from VNDB in JSON format. You can obtain it using the VNDB Query service.

Example query (for reference):  
https://query.vndb.org/0e6f3eba652fb016/q

> ⚠️ This query is for personal use only. Feel free to copy and modify it as needed.  
> To retrieve characters from a specific visual novel, uncomment the `v.id = 'v7'` line and replace `v7` with the VNDB ID of your chosen visual novel.  

### Exporting the data

1. Open your query in the VNDB Query service.
2. Click **EXPORT → JSON**.
3. Download the resulting JSON file.

### VNDB row limit warning

- VNDB Query exports are limited to **100,000 rows per export**.
- If your query returns more than that:
  - Use the `OFFSET` parameter to skip already exported rows (e.g. `OFFSET = 100000`, `OFFSET = 200000`, etc.).
  - Export the query multiple times until an export returns fewer than 100,000 rows.

---

## 2. Prepare the input folder

1. Create a new folder anywhere on your system.
2. Put **all exported JSON files** into this folder.

This folder will be used as the tool’s input.

---

## 3. (Optional) Download character images from VNDB

If you want to include character images in the generated entries, you must download them from [VNDB](https://vndb.org/d14#6). VNDB distributes images via `rsync`, which is not available by default on Windows.

### 3.1 Install MSYS2

1. Go to https://www.msys2.org/
2. Download and run the installer.
3. Complete the installation using default options.
4. Open `MSYS2 UCRT64` from the Start Menu

### 3.2 Update MSYS2

In the MSYS2 window, run:

```sh
pacman -Syu
```

- If MSYS2 asks you to close the window, close it.
- Reopen **MSYS2 UCRT64** and run the command again.
- Repeat until no updates remain.

### 3.3 Install rsync

Still in the MSYS2 window, run:

```sh
pacman -S rsync
```

### 3.4 Create an image folder

Create a folder in like:

```text
C:\VNDB\ch
```

MSYS2 will access this path as:

```text
/c/VNDB/ch/
```

### 3.5 Download the images

Run the following command in MSYS2:

```sh
rsync -rtpv rsync://dl.vndb.org/vndb-img/ch/ /c/VNDB/ch/
```

- This downloads all VNDB character images.
- Files are saved to `C:\VNDB\ch`.
- The download is large and may take a long time.
- Re-running the command later only downloads new or updated files.

---

## 4. Run the tool

You can run the tool from `Command Prompt`:

```cmd
VndbCharacterNames.exe <InputFolderPath> <OutputFilePath> \
  --create-alias-entries=<true|false> \
  --max-spoiler-level-for-aliases=<0|1|2> \
  --add-character-details-to-full-names=<true|false> \
  --add-description-to-definition=<true|false> \
  --include-spoilers-in-description=<true|false> \
  --add-details-to-one-word-full-names=<true|false> \
  --add-details-to-given-names=<true|false> \
  --add-details-to-surnames=<true|false> \
  --path-of-character-images=<VNDB character image folder if you downloaded it, otherwise you must omit this parameter>
```

### Recommended settings:

If you did not uncomment the `v.id = 'v7'` line to retrieve the characters from a single visual novel, following settings are recommended to create a less cluttered dictionary:

```cmd
VndbCharacterNames.exe C:\Users\User\VndbExports C:\Users\User\Desktop\JL\Dicts\VndbCharacterNames \
  --create-alias-entries=true \
  --max-spoiler-level-for-aliases=1 \
  --add-character-details-to-full-names=true \
  --add-description-to-definition=false \
  --include-spoilers-in-description=false \
  --add-details-to-one-word-full-names=false \
  --add-details-to-given-names=false \
  --add-details-to-surnames=false \
  --path-of-character-images="C:\VNDB\ch"
```

If you did uncomment the `v.id = 'v7'` line to retrieve the characters from a single visual novel, following settings are recommended:

```cmd
VndbCharacterNames.exe C:\Users\User\VndbExports C:\Users\User\Desktop\JL\Dicts\VndbCharacterNames \
  --create-alias-entries=true \
  --max-spoiler-level-for-aliases=1 \
  --add-character-details-to-full-names=true \
  --add-description-to-definition=true \
  --include-spoilers-in-description=false \
  --add-details-to-one-word-full-names=true \
  --add-details-to-given-names=true \
  --add-details-to-surnames=true \
  --path-of-character-images="C:\VNDB\ch"
```

Alternatively, you can double-click `VndbCharacterNames.exe` and enter the parameters when prompted.

---

## 5. Output files

The tool generates two files:

1. `{SpecifiedFileName}_Custom_Name.txt`  
- Format: `JL Custom Name`
- Not recommended for full VNDB character data
- Best used when a custom character name dictionary is generated for a specific visual novel by uncommenting the `v.id = 'v7'` line in the query and replacing `v7` with the VNDB ID of the chosen visual novel.
- Rename it to: `{JLProfileNameForTheVN}_Custom_Name.txt`
- Place it under: `..\JL\Profiles`

2. `{SpecifiedFileName}.json`  
- Format: `Modified Nazeka EPWING Converter`
- Compatible with `Nazeka` (when images are omitted) and [JL](https://github.com/rampaa/JL)
- If you plan to use it with JL, select `Name Dictionary (Nazeka)` as the dictionary type when adding it to JL

---

<img src="https://github.com/user-attachments/assets/0261329f-0db6-4daf-9e2d-79282f584edf" width="50%" height="50%">
