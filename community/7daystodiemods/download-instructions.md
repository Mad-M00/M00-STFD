# Install — takes two minutes

1. **Download** the zip from this page.
2. **Open your Mods folder.** Either of these works:
   `...\Steam\steamapps\common\7 Days To Die\Mods`
   or press Windows+R and type `%APPDATA%\7DaysToDie\Mods`
   *(If there is no Mods folder yet, create one named exactly: `Mods`)*
3. **Extract the zip there.** You must end up with `ModInfo.xml` directly
   inside `Mods\M00-STFD`. If you see `Mods\M00-STFD\M00-STFD`, move the
   inner folder up one level — a folder inside a folder is the number
   one reason a mod "does not work".
4. **Turn EAC (anti-cheat) OFF** in the game launcher, like for any DLL
   mod. With EAC on, the mod safely skips itself and simply does nothing.
5. **Check that `0_TFP_Harmony` still exists** in the game install's Mods
   folder. It ships with the game and all DLL mods need it. If it is
   missing: in Steam right-click 7 Days to Die → Properties → Installed
   Files → Verify integrity of game files, and Steam puts it back.

**Check it works:** in a world, press F1 and type `stfd` — if the console
shows the settings, you are set. Now go find a switch-opened door and
close it behind you.

**Multiplayer:** install on the server **and** on each player. Server-only
already fixes switch/key doors for everyone; lockpicking needs the mod on
the player's machine too.

**Updating:** close the game first, keep a copy of `STFDConfig.xml` if you
edited it, replace the mod folder, put your config back.

**Uninstalling:** delete the `Mods\M00-STFD` folder. Your save is
untouched.

Questions? [The FAQ covers the usual suspects.](https://github.com/Mad-M00/M00-STFD/blob/main/docs/user/faq.md)
