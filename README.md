| | |
| ------------- | ------------- |
| <img width="128" alt="Logo" src="https://user-images.githubusercontent.com/84620/224557684-ad2cf053-123f-4244-9df9-3bed55c5214c.png"> | Windows tool to manage and send your compatible mast1c0re ELFs &amp; ISOs at one place. |

## Features of v1
- Store all your ELF files and PS2 ISO games at one place
  - To use with mast1c0re compatible ELF files
- Send ELFs to the mast1c0re Network ELF Loader
- Send Games to the mast1c0re Network Game Loader
- PS2 Backup Manager
  - Shows game details and art from https://psxdatacenter.com/ if the game ID is supported
- Emulator ISO Repacker (Currently broken - the new ISO might not work)
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

### Some tested games on PS5 6.50 with "Network Game Loader v0.1.4"
| Game Title | Game ID | Works ? | Comments |
| ------------- | ------------- | ------------- | ------------- |
Canis Canem Edit | <img src='https://user-images.githubusercontent.com/84620/224665630-c8172062-4f02-4e40-b1c2-d07c7142cd56.png' width='21' height='15'> SLES-53561 | :yellow_square: Menu | Crashes after loading the menu + 1 trophy
Crash Twinsanity | <img src='https://user-images.githubusercontent.com/84620/224665633-5499d0ca-8d37-4bd4-b044-932f9f4f23f1.png' width='21' height='15'> SLUS-20909 | :green_square: Yes | Playable with minimal glitches
Def Jam - Fight for NY | <img src='https://user-images.githubusercontent.com/84620/224665630-c8172062-4f02-4e40-b1c2-d07c7142cd56.png' width='21' height='15'> SLES-52507 | :red_square: No | Crash after transfer
Devil May Cry | <img src='https://user-images.githubusercontent.com/84620/224665630-c8172062-4f02-4e40-b1c2-d07c7142cd56.png' width='21' height='15'> SLES-50358 | :yellow_square: Menu | Boots into menu -> Crash after language selection
Kingdom Hearts II | <img src='https://user-images.githubusercontent.com/84620/224665630-c8172062-4f02-4e40-b1c2-d07c7142cd56.png' width='21' height='15'> SLES-54114 | :yellow_square: Menu | Boots into game -> Crash after starting a new game
Max Payne 2 | <img src='https://user-images.githubusercontent.com/84620/224665630-c8172062-4f02-4e40-b1c2-d07c7142cd56.png' width='21' height='15'> SLES-52336 | :red_square: No | Crash after transfer
Metal Slug 3 | <img src='https://user-images.githubusercontent.com/84620/224665630-c8172062-4f02-4e40-b1c2-d07c7142cd56.png' width='21' height='15'> SLES-52599 | :red_square: No | Crash after transfer
Midnight Club 3 DUB Edition | <img src='https://user-images.githubusercontent.com/84620/224665630-c8172062-4f02-4e40-b1c2-d07c7142cd56.png' width='21' height='15'> SLES-52942 | :green_square: Yes | Playable with minimal glitches
Mortal Kombat Armageddon | <img src='https://user-images.githubusercontent.com/84620/224665630-c8172062-4f02-4e40-b1c2-d07c7142cd56.png' width='21' height='15'> SLES-54156 | :red_square: No | Triggers a trophy after transfer then crashes
Need for Speed Underground | <img src='https://user-images.githubusercontent.com/84620/224665630-c8172062-4f02-4e40-b1c2-d07c7142cd56.png' width='21' height='15'> SLES-51967 | :yellow_square: Menu | Menu works but crashes when starting a race, triggers 2 trophies 
Resident Evil 4 | <img src='https://user-images.githubusercontent.com/84620/224665630-c8172062-4f02-4e40-b1c2-d07c7142cd56.png' width='21' height='15'> SLES-53702 | :green_square: Yes | Boots into game and triggers 3 trophies + some graphical glitches
Scarface | <img src='https://user-images.githubusercontent.com/84620/224665630-c8172062-4f02-4e40-b1c2-d07c7142cd56.png' width='21' height='15'> SLES-54182 | :green_square: Yes | Boots into the game - stuttering in cut-scenes + some graphical glitches
The Simpsons Hit & Run | <img src='https://user-images.githubusercontent.com/84620/224665630-c8172062-4f02-4e40-b1c2-d07c7142cd56.png' width='21' height='15'> SLES-51897 | :red_square: No | Crash after transfer
Worms 4: Mayhem | <img src='https://user-images.githubusercontent.com/84620/224665630-c8172062-4f02-4e40-b1c2-d07c7142cd56.png' width='21' height='15'> SLES-53096 | :red_square: No | Crash after transfer


## How-to repack an emulator ISO file
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
