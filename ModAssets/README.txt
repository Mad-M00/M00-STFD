Shut the Frigging Door (STFD) - Doors locked open, fixed. Locked doors, pickable.
====================================================================

WHAT THIS MOD DOES

Two things, both about locked POI doors:

1. CLOSE THE OPEN ONES. You know the door: a switch or a key swings it
   open for you, and then it stands there, wide open, forever - press E
   and the game buzzes "locked" at you. Locked. In the OPEN position.
   With this mod any world door that ends up open-but-locked simply
   becomes unlocked. Close it behind you. Open it again. Sleep in the
   POI with the door shut, the way nature intended.

2. PICK THE CLOSED ONES. A closed locked world door can now be
   lockpicked, exactly like a safe or a police car: look at it, press
   E with lockpicks in your inventory, and hold on. Picks can snap
   (progress is kept, like safes). The Lock Picking perk helps, same
   as it does for safes.

   Difficulty scales with the door:

     wood   10s   house doors, closets, gates, wooden hatches
     iron   15s   iron doors, garage doors, roll-ups, chainlink
     steel  20s   jail doors, steel garage doors, bulletproof glass
     vault  30s   vault doors and vault hatches

   (Base times before perks; break chance 0.75 like vanilla safes.)

   AND IT MAKES NOISE. The click of the lock giving way can wake
   whatever is directly behind the door. A pick snapping rings out -
   the room and anything nearby may come looking. Crouch before you
   pick: sneaking muffles the noise, and the From The Shadows perk
   muffles it further, just like your footsteps.

WHAT IT NEVER TOUCHES

- Your own doors. Anything a player placed keeps its lock and its
  owner - it can't be closed by strangers and it can't be picked.
  Base security is exactly vanilla.
- Trader gates. They close and lock for the night exactly as before.

TUNING IT

Everything is configurable in STFDConfig.xml (next to this
file): both features have their own on/off switch, and the tiers,
per-door rules, mercy cap, noise loudness and sneak muffling are all
plain numbers with instructions inside the file. If you break it, the
mod falls back to its defaults and says so in the game log - you
can't damage your game with it.

Apply your edits WITHOUT restarting: press F1 and type

  stfd reload           load your changes and show the result
  stfd                  show the current settings
  stfd tier jailDoor01  show which difficulty a door gets

Don't want lockpicking at all? Set enabled="false" on the
<Lockpicking> line. Don't want the open-door fix? Set
enabled="false" on the <CloseOpenDoors> line.

INSTALLATION

1. Copy this mod folder into ...\7 Days To Die\Mods\M00-STFD\
2. Start the game. EAC must be set to allow mods (SkipWithAntiCheat
   is set, so with EAC on the mod simply stays inactive).

MULTIPLAYER

Install on BOTH server and clients. The server side covers switch-
and key-opened doors for everyone; the client side provides the
press-E catch-all and the lockpick minigame (it runs on the player's
own screen). Lock noise works everywhere: on dedicated servers the
noise is sent to the server so zombies genuinely wake and investigate.

IF SOMETHING DOES NOT WORK

The game log (F1 console) shows a line like
  [STFD] unlocked open door 'doorSecure...' at ...
  [STFD] lockpicked door 'jailDoorGrey' at ...
every time the mod acts. If the mod folder is present but nothing
happens, check that the 0_TFP_Harmony mod folder still exists in
your Mods directory - the game ships with it, and every DLL mod
needs it.

Answers to the usual questions (multiplayer installs, trader gates,
quest doors, sleeper wakes) are in the online FAQ:

  https://github.com/Mad-M00/M00-STFD/blob/main/docs/user/faq.md

The full player guide lives one folder up from there.
