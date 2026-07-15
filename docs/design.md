# Design — the investigation behind the mod

## The request

> "Unlock Open Door. Please make it so you can close doors that you have unlocked
> with a Switch or Key. It makes absolutely no sense for doors to be locked in an
> open position. This used to be a mod."

## What "locked in an open position" actually means (V3 investigation)

Investigated against the installed game (V3.x, `Assembly-CSharp.dll` dated 2026-07-14),
decompiled with ilspycmd: `TEFeatureDoor`, `TEFeatureLockable`, `TEFeatureLockPickable`,
`TileEntityComposite`, `BlockTrigger`, `BlockActivateSwitch`, `TraderArea`.

V3 doors are **composite tile entities** (`TileEntityComposite` on a
`BlockCompositeTileEntity`) built from feature modules. A lockable POI door has:

- `TEFeatureDoor` — open/close state, animation, the "open"/"close" activation commands.
- `TEFeatureLockable` — `locked` flag, owner, allowed users, keypad/password.
- `TEFeatureLockPickable` (some doors) — lockpicking; on success it *downgrades the
  block* to an unlocked variant, so lockpicked doors are not the problem.

### Why the door gets stuck "locked open"

There are two separate code paths into a door:

1. **Trigger path** (switches, keys, quest events) — `TEFeatureDoor.OnBlockTriggered`
   toggles the door open **ignoring the lock entirely**. It only clears the lock if
   the POI designer ticked the `Unlock` checkbox on that trigger
   (`BlockTrigger.Unlock`, trigger-data version 5+). Most POI switches don't have it
   ticked, so the door swings open with `locked == true` left behind.

2. **Player path** — `TEFeatureDoor.OnBlockActivated("close"/"open")` refuses and
   plays the locked sound whenever `lockFeature.IsLocked()` and the player is not an
   allowed user. The "close" prompt is still shown (`AllowBlockActivationCommand`
   only checks `isOpen`), which is exactly the user-facing absurdity: you get a
   Close prompt on a wide-open door and it buzzes "locked" at you.

Doors can also **spawn** open+locked directly from prefab tile-entity data, and a
**quest reset** re-copies prefab data, restoring the locked state.

### Trader doors (exploit check — safe)

`TraderArea.SetClosed` closes trader doors *first*, then locks them; on morning open
it unlocks them while open. So vanilla trader doors are never legitimately
open+locked, and a mod that unlocks only **open** doors cannot be abused to keep a
trader accessible at night. The policy's "closed doors are never unlocked" boundary
is the guarantee — no trader special-case needed.

### Player-placed doors

`TileEntityComposite.PlayerPlaced` is `Owner != null`; world-generated POI blocks
have no owner. That one flag cleanly separates "loot-route door in a POI" from
"someone's base door", so the policy excludes owned doors and base security stays
exactly vanilla.

## Prior art

- **"Unlock Open Doors" by IDizor** — the mod the commenter remembers.
  [Nexus #1832](https://www.nexusmods.com/7daystodie/mods/1832),
  [GitHub](https://github.com/IDizor/7D2D-UnlockOpenDoors) (GPL-3.0), actively
  maintained (v3.0.1 for game V3.0, July 2026). It patches five points including
  `TEFeatureLockable.SetLocked` and the `Prefab` tile-entity read paths.
- Alternatives are lockpick-based (Mazriq's Lockpick Doors #10870, Pick Locked
  Doors #2223) — a different design (spend lockpicks), not what the commenter asked.
- Vanilla V2/V3 patch notes show TFP has not fixed this.

**License note:** IDizor's source is GPL-3.0. STFD is an independent,
from-scratch implementation derived from our own decompilation of the game — no
code was taken from that repo, and none may be.

## Behaviour specification

**The rule (one sentence):** a door that is *open*, *locked*, and *not player-placed*
becomes unlocked.

| # | Behaviour |
|---|-----------|
| B1 | A POI door opened by a switch/trigger/key-event becomes unlocked the moment it opens; the player can close it (and reopen it) freely. |
| B2 | A POI door that loads from disk / spawns from a prefab already open+locked becomes unlocked when its tile entity is loaded. |
| B3 | A quest reset restores vanilla prefab state; if that state is open+locked, B1/B2 re-apply. Doors relocked *closed* by a reset stay locked (vanilla behaviour). |
| B4 | Player-placed doors (`Owner != null`) are **never** touched. |
| B5 | Closed doors are never unlocked, regardless of anything. Trader night-locking therefore works unchanged. |
| B6 | Applies to everything with both `TEFeatureDoor` + `TEFeatureLockable`: doors, hatches, garage doors, vault doors, drawbridges. |

Non-goals (v1): no config file, no XML changes, no UI, no localization (vanilla
tooltips become correct automatically once the lock state is fixed).

## Implementation decisions

Three Harmony hooks (see [architecture.md](architecture.md) for the layer they
delegate into):

| Hook | Kind | Covers |
|---|---|---|
| `TileEntityComposite.OnBlockTriggered` | postfix | B1 — runs after all feature modules processed the trigger, so the door's new open state is final |
| `TileEntity.OnReadComplete` | postfix | B2 — disk and network loads |
| `TEFeatureDoor.OnBlockActivated` | prefix | Catch-all — unlocks before vanilla's own locked check in the same method, so a single E-press closes the door |

Deliberately **not** patched:

- `TEFeatureLockable.SetLocked` (IDizor's guard) — a global "refuse to lock open
  doors" intercepts every caller, including vanilla systems that legitimately lock
  doors (trader areas lock doors immediately after force-closing them). Targeted
  postfixes achieve the outcome with less blast radius.
- The `Prefab` tile-entity read paths — the read-complete and activation hooks
  cover prefab-spawned doors; if in-game testing of quest resets (B3) finds a gap,
  a prefab-path hook is the slice-2 addition.

## Extension: lockpicking closed locked doors

**The ask:** unlock closed locked doors with lockpicks, difficulty by door
type (wood / iron / steel / vault), behaviour like picking a police car or
chest.

**Why not vanilla's `TEFeatureLockPickable`?** It looks purpose-built, but
it is per-block-type, not per-instance: adding it to `commercialDoorV2Grey`
via XML would make *every* such door in the world demand lockpicking —
including the thousands of unlocked ones — because its
`AllowBlockActivationCommand` disables all other commands (open/close!) for
non-owners until picked, and its success path *downgrades the block*
(right for safes, wrong for doors whose lock is tile-entity state). Fighting
those assumptions needs more patches than implementing the flow ourselves.

**What we reuse from vanilla instead:** the entire player-facing machinery —
`XUiC_Timer.OpenTimer` (the hold-to-pick modal), `resourceLockPick` items,
`PassiveEffects.LockPickTime`/`LockPickBreakChance` via `EffectManager`
(so the Lock Picking perk works unchanged), the vanilla sounds and
`ttLockpickMissing`/`ttLockpickBroken` tooltips, and the same
progress-survives-broken-picks model (`Completion` carried between
attempts; timer `AlternateTime` = pre-rolled snap moment). Base numbers
calibrated against vanilla safes (gun safe 15s / 0.75 break chance):
wood 10s, iron 15s, steel 20s, vault 30s.

Two retry rules came out of playtesting: re-rolling the full 0.75 on every
retry meant locks routinely snapped 3-7 times in their final seconds
(geometric distribution — P(≥3 consecutive snaps) ≈ 0.42), which felt
terrible. So (1) the break chance scales with the work still ahead
(`breakChance × (1 − completion)`) — a fresh attempt keeps the vanilla
odds, a retry at 90% risks a tenth of them — and (2) a mercy cap
(`maxBreaks`, default 4) guarantees the attempt after enough snaps on the
same door, the same idea as vanilla V3's 14-break cap on safes.

**Difficulty resolution** (Core, config-driven from
`STFDConfig.xml`): first matching door-name wildcard rule → first
matching material-id rule (`Mwood*`, `Msteel*`, …) → default tier.
Name rules exist because material alone can't separate a vault door from a
steel garage door (both `Msteel`).

**Noise (risk to balance the reward):** the unlock click carries 3m with a
50% per-zombie alert chance ("directly behind the door"); a snapping pick
carries 8m at 80% ("the room and nearby"). Alerted zombies wake
(`ConditionalTriggerSleeperWakeUp`) and investigate the door
(`SetInvestigatePosition`). Crouching scales both radius and chance by the
config sneak factor (0.6) times the game's `NoiseMultiplier` passive —
which is exactly what the From The Shadows perk reduces while crouched, so
"reduced based on sneaking skill" uses the perk players actually level.
Zombie AI is simulated by the authority, so the alert runs there: in
single-player / P2P the picking process is the authority and alerts
locally; a client on a dedicated server sends the pre-scaled noise via a
custom net package (`NetPackageStfdLockNoise` — package ids are negotiated
by type name at connect, so mod packages work whenever both sides run the
mod, which lockpicking already requires). The server never trusts the
client's numbers: `ValidEntityIdForSender` drops spoofed instigators, and
`LockNoisePlanner.ClampIncoming` caps radius at the server config's
loudest noise and chance at certainty — a hacked client can at worst make
the noise the mod could already make.

**Boundaries** (each pinned by a test or an explicit guard):
- Player-placed doors can never be picked — same `Owner == null` flag as
  the unlock rule.
- Open locked doors are never picked — the unlock rule owns them; no
  lockpick toll on a door the mod opens for free.
- **Trader doors and gates can never be picked** — any door inside a
  trader area (`World.GetTraderAreaAt`) is refused, so the night lock and
  the vanilla "come back at opening time" message are untouched.
- Keypad-allowed users are refused too: vanilla already opens the door
  for them.
- Editor sessions are excluded.

## Why this is a C# mod, not XML

A door's lock state is per-tile-entity instance data baked into prefab/world data,
not a `blocks.xml` property. XML can add lockpicking to doors, but nothing in XML
can clear a locked flag on an already-open door — that requires code at runtime.
