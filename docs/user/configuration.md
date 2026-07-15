# Configuration

Everything lives in one file in the mod folder:

```
...\Mods\M00-STFD\STFDConfig.xml
```

It is a normal text file with instructions inside, and it ships
identical to the built-in defaults. If you break it, the mod ignores
all of it, uses the defaults, and says so in the game log — an edit can
never half-apply.

## The switches

```xml
<CloseOpenDoors enabled="true" />
<Lockpicking enabled="true" ... maxBreaks="4">
```

Each feature turns off independently: keep the door fix without
lockpicking, or the other way round.

## The numbers

| Where | What you can change |
|---|---|
| `<Tier>` | Pick seconds and break chance per tier — add your own tiers |
| `<Door match="vault*" tier="vault"/>` | Which doors get which tier; `*` is a wildcard, top rule wins |
| `<Material>` | Fallback tier by block material when no door rule matches |
| `maxBreaks` | Snaps on one door before the next attempt is guaranteed |
| `<Noise>` | Radius and alert chance for the unlock click and the pick snap; `radius="0"` = silent picking |
| `sneakFactor` | How much quieter crouching makes you |
| `investigateSeconds` | How long alerted zombies search around the door |

## Apply changes without restarting

Press F1 and type:

```
stfd reload            load your edits and show the result
stfd                   show the current settings
stfd tier vaultDoor01  show which tier a door resolves to
```

The `tier` command is the quick way to check a rule you just wrote:
edit, reload, ask.
