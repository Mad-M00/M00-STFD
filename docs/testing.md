# Testing a game mod without the game

The tests run in milliseconds with `dotnet test` — no 7 Days To Die
installation needed on the machine running them.

## The trick: the tests compile Core's source, not the mod's DLL

A mod DLL cannot load in a normal test runner — it references
`Assembly-CSharp` and Unity, which only exist inside the game. So the
test project does not reference the mod assembly at all. It compiles
the `src/Core` **source files** directly into itself, against standard
.NET:

```xml
<!-- tests/M00-STFD.Tests.csproj -->
<ItemGroup>
  <Compile Include="..\..\src\Core\**\*.cs" LinkBase="Core" />
</ItemGroup>
```

Side effect: **if a game type leaks into Core, the test project stops
compiling.** The layering rule is enforced by the build, not by
convention.

## What the tests protect

Intent tests, not coverage tests — each pins a promise. The highlights:

| Test | The promise it pins |
|---|---|
| `A_world_door_standing_open_but_locked_is_unlocked` | The mod's reason to exist |
| `Closed_doors_are_never_unlocked` | No silent bypass; trader night-lock stays exploit-free |
| `Player_placed_doors_are_never_touched` / `_never_picked` | Base security is exactly vanilla, for both rules |
| `Open_doors_are_never_picked_the_unlock_rule_owns_them` | A door matches exactly one rule — no lockpick toll on a door the mod opens for free |
| `Vault_doors_resolve_by_name_before_their_steel_material` | Vaults stay harder than ordinary steel doors |
| `Default_tiers_escalate_wood_iron_steel_vault` | The difficulty ladder's ordering is game balance |
| `Malformed_xml_falls_back_to_complete_defaults` | A broken config edit can never half-apply |
| `Each_feature_can_be_switched_off_alone` | The two feature switches are independent |
| `Progress_from_a_broken_pick_shortens_the_next_attempt` | Snapped picks keep their progress |
| `Retry_break_chance_shrinks_with_progress` | The playtest fix: no snap-farming at the finish line |
| `The_mercy_cap_guarantees_the_attempt_after_enough_snaps` | Unlucky streaks end |
| `Incoming_network_noise_is_clamped_to_server_limits` | A hacked client can at worst make the noise the mod could already make |

Not tested: that Harmony patched a method, that `SetLocked` syncs, that
a trigger fires. That is the game's behaviour and the adapter layer's
job — verifying it requires the game, and mocking half the game API
produces tests that pass while the mod is broken. Core is tested
exhaustively; Game/Mod are kept thin and verified by playing.

## In-game verification checklist

One scenario per hook plus the boundaries. Concrete POIs for each are
in [test-pois.md](test-pois.md):

1. **Switch door** — flip the switch → tooltip reads *unlocked* → E
   closes it → E reopens it.
2. **Key / quest-event door** — same expectations.
3. **Spawned locked-open door** — unlocks when its chunk loads (watch
   the `[STFD] unlocked open door ...` log line).
4. **Quest reset** — restores vanilla prefab state; re-triggering the
   door unlocks it again.
5. **Player door regression** — a non-owner cannot close, unlock or
   pick a placed door.
6. **Trader regression** — gates lock at 22:00 as vanilla; E on the
   night gate shows "come back at opening time", not the pick timer.
7. **Lockpicking** — wood fast, jail slower, vault slowest; a snap
   resumes shorter; zero picks shows the vanilla hint; the perk
   visibly speeds things up; `enabled="false"` restores the vanilla
   buzzer.
8. **Noise** — a snap in a sleeper room stands most of it up; repeat
   crouched with From The Shadows ranks and watch the wake count drop
   (`[STFD] lock noise ... alerted N zombie(s)`).

## Workflow

```
dotnet test          # run everything (fast - no game needed)
dotnet build         # compile the mod DLL against the game's assemblies
```

When a rule changes, its test changes in the same commit.
