# FAQ

**The mod is installed but nothing happens.**
EAC (anti-cheat) must be set to off in the game launcher — this is a
DLL mod. Also check that the `0_TFP_Harmony` folder still exists in
your Mods directory; the game ships with it and every DLL mod needs it
(if it's missing, verify game files in Steam).

**How do I know it's working?**
The game log (F1) prints a line every time the mod acts:
`[STFD] unlocked open door ...`, `[STFD] lockpicked door ...`,
`[STFD] lock noise at ... alerted N zombie(s)`.

**Multiplayer — who installs what?**
Same folder on both sides. Server-only already gives every player
(modded or not) the door fix. Each player who wants the press-E fix
and lockpicking installs it too — the pick timer runs on your own
screen, and the noise is sent to the server so zombies wake for
everyone.

**Can someone pick my base door?**
No. Player-placed doors keep their owner and their lock — they can't
be closed by strangers, unlocked, or picked. Base security is exactly
vanilla.

**Can I pick my way into the trader at night?**
No. Doors and gates inside trader areas are excluded; you get the
vanilla "come back at opening time" message.

**Can quest doors be picked? Isn't that cheating?**
Yes, they can — the same doors you could always break down with a
sledgehammer. Picking is just quieter and leaves the door usable. It
never completes or skips a quest objective (an objective switch still
has to be flipped), and a quest reset restores every lock.

**A snapped pick woke the whole room. Working as intended?**
Yes. The snap rolls a chance against every spawned zombie within about
8 metres, sleeping or not — walls don't block it. It won't reach rooms
that haven't spawned in yet and it never attracts screamers (no heat
map). Crouch and level From The Shadows to pick quietly.

**Can I turn the noise off? Or the whole lockpicking feature?**
Yes — `<Noise>` radii of 0 make picking silent, and each feature has
its own `enabled` switch. See [Configuration](configuration.md).

**Is it safe to uninstall?**
Yes. The mod never edits game files. Doors it unlocked stay unlocked
in that save (that's normal world state); everything else returns to
vanilla behaviour immediately.

**Does it work with door mods / modded doors?**
Yes, automatically — the mod keys off the game's door and lock
features, not block name lists, so any door built the vanilla way is
covered. Difficulty for unknown doors falls back to material, then to
the default tier.
