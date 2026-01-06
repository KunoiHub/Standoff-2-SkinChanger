# SO2 Advanced SkinChanger

Skin changer for **Standoff 2**, working through memory manipulation of the emulator process (`hd-player.exe`).

This tool allows you to replace equipped skin IDs locally while playing Standoff 2 in an emulator environment. 

---

## Features

- Works with **Standoff 2 (emulator version)**
- Supports **HD-Player / LDPlayer processes**
- Signature-based memory scanning
- Multiple match detection
- Replace all found values or select a specific address
- No network interaction
- Console-based interface

---

## Supported Emulators

The program automatically searches for the following processes:

- `HD-Player`
- `LdVBoxHeadless`
- `Ld9BoxHeadless`

If none of them are found, the tool will not start.

---

## How It Works

1. The program attaches to the emulator process.
2. It scans committed read/write memory regions.
3. A signature is built using the current skin ID and a fixed suffix pattern.
4. All matching memory addresses are collected.
5. Selected values are replaced with a new skin ID.

---

## Usage

1. Launch the emulator.
2. Open **Standoff 2**.
3. Equip the skin you want to replace.
4. Build exe as Release | 64
5. Run the program **as Administrator**.
6. Enter the current skin ID.
7. Enter the new skin ID.
9. Choose skin ID by name in the file (skins.txt)
10. Choose one of the available replacement options.

If no matches are found, re-equip or unequip the skin in-game and try again.

---

## Requirements

- Windows 10 / 11
- 64 bit emulator
- .NET runtime compatible with C#
- Emulator running Standoff 2
- Administrator privileges

---
## Example usage (Skin ID 240014 is Karambit "Nebula")
![SO2 SkinChanger Preview](https://i.imgur.com/BJ56ZOc.png)


## Author

https://t.me/kunoi
