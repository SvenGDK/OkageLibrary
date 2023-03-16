## PS2 Emulator Configuration Files
You can find here some some already created config (.conf) files.</br>
Some are official and some user created - see folder names.</br>
An updated list of tested PS2 games WITH a config file will be available soon, including comments.

## How-To Use
If you see a file with "ENTERID_cli.conf" then it should be compatible with the EU & US version (NTSC-J probably too).
- Rename this file and enter the game ID of your ISO like "SLES-12345" and save to "SLES-12345_cli.conf".

If you see a file with "SCUS-ENTERID_cli.conf" then it's a config that has been tested only on the US version.
- Rename this file to the correct game ID like "SCUS-12345_cli.conf".

## The following list of commands have been identified to cause a crash:

```
Grand Theft Auto: Liberty City Stories (SLUS_21423)
--gs-h2l-accurate-hash=1
--gs-adaptive-frameskip=1

Wild Arms 3
--ee-static-block-links=JAL,COP2
```

## Thanks
Big thanks goes to the [PS Dev Wiki](https://www.psdevwiki.com/ps4/Talk:PS2_Classics_Emulator_Compatibility_List) and users who created the config files.</br>
@McCaulay for the already listed options that can cause a crash.
