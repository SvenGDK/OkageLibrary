| | |
| ------------- | ------------- |
| <img width="128" alt="Logo" src="https://user-images.githubusercontent.com/84620/224557684-ad2cf053-123f-4244-9df9-3bed55c5214c.png"> | Windows tool to manage & send compatible mast1c0re ELFs & ISOs. </br> Includes a RetroArch PS2 ISO Repacker and a Format to exFAT MBR Tool. |

## Features of v1.2
- Store all your ELF files and PS2 ISO games at one place
  - To use with mast1c0re compatible ELF files
- Send ELFs to the mast1c0re Network ELF Loader
- Send Games to the mast1c0re Network Game Loader
  - Compatible with the [modified network game loader](https://github.com/SvenGDK/mast1c0re-mod-network-game-loader) to transfer your games faster
- PS2 Backup Manager
  - Shows game details and art from [PSXDatacenter](https://psxdatacenter.com) if the game ID is supported
- Emulator ISO Repacker
  - Allows re-packing RetroArch Emulator ISOs
- exFAT MBR USB Format Tool
  - Can backup data before the format process
  
## How-to send an ELF file
- Note down your console IP
- Open "Okage: Shadow King" and load the exploited save game with the "Network ELF Loader".
- Wait until you see the message "Waiting for ELF file"
- Open "OkageLibrary" and enter your console IP
- OPTIONAL: Add the "mast1c0re Network Game Loader" to the Homebrew list if not done yet
- Select the ELF on the list and select "Send selected ELF"
- The ELF should have been loaded on your console

## How-to send a game
- Send the "Network Game Loader" ELF to the console
- Open the PS2 Backup Manager and add a game if not done yet
- Select the game on the list and select "Send selected game to console"
- Wait until the game has been transferred to the console
- Enjoy (if not crashed... :P)

## How-to repack an emulator ISO file
- Get the "LICENSEA.DAT" file (You can find by looking for the "Psy-Q SDK") and copy it to "\Tools\LIC"
- Open the "Emulator ISO Repacker" tool
- Select a compatible RetroArch Emulator ISO like "PicoDrive_PS2_Megadrive.iso"
- INFO: The list will fill up if some roms have already been added
- Select "Add new game" and choose your game rom (INFO: A file extension filter will be applied if the core is known)
  - OR select "Delete selected game" to delete the game rom from the ISO
- Select an output folder by clicking on "Select output folder"
- Click on "Create ISO"

## How-to format an USB drive in exFAT & MBR
- Open the "Format USB in exFAT" tool
- Select your removable USB drive from the list
- OPTIONS: READ WITH CAUTION AND ALWAYS USE THE OPTION "Keep Data when formatting" :
  - "Convert to MBR" converts an GPT USB drive (This deletes all data IF THE SECOND OPTION IS NOT SELECTED!)
  - "Keep Data when formatting" TO KEEP ALL YOUR DATA from your USB drive (This requires the used space on your USB to be available on your C:\)
- Click on "Start Format"

## Tested PS2 games
* See [PS2 Games Compatibility](https://github.com/SvenGDK/OkageLibrary/blob/main/PS2GamesCompatibility.md)
