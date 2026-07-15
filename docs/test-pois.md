# Where to test: POIs with switch/trigger doors that stay locked

Found by parsing the game's own prefab data — every `.tts` in
`Data/Prefabs/POIs` has its trigger records read directly (block position,
trigger channels, and the per-trigger `Unlock` flag), and each trigger
position is resolved to its block name via the prefab's `.blocks.nim`.
The scanner lives in [`tools/TtsTriggerScanner.cs`](../tools/TtsTriggerScanner.cs);
run it from PowerShell with:

```powershell
Add-Type -Path tools/TtsTriggerScanner.cs
[TtsTriggerScanner]::Report("<game>\Data\Prefabs\POIs", "doors.txt", "switches.txt")
```

(`doors.txt` = block names with both `TEFeatureDoor` + `TEFeatureLockable`
resolved through `Extends` inheritance from `blocks.xml`; `switches.txt` =
`ActivateSwitch` blocks: `powerSwitch01/02`.)

## Headline numbers (V3.x, scanned 2026-07-15, 1105 POIs)

| | |
|---|---|
| Trigger-driven lockable doors across all POIs | **1604** |
| …that stay locked after the trigger opens them | **1338 (83%)** |
| …with the `Unlock` flag actually ticked (vanilla behaves) | 266 |
| Stuck doors driven by a player-usable **switch** | 370 |
| Stuck doors driven by other triggers (sensors, quest events, volumes) | 968 |

So the complaint is not an edge case: five of six triggered lockable doors
in the game ship without the Unlock flag.

## Recommended test targets

Small, common POIs where a wall switch opens a door that vanilla leaves
locked (`STAYS-LOCKED` + `switch` in the scan):

| POI | What to test | Why this one |
|---|---|---|
| `army_camp_02` / `army_camp_03` | Switch opens `vaultDoor01` bunker door | Tiny POI, the classic case — walk in, flip switch, try to close the vault door behind you |
| `utility_waterworks_01` | Switch opens `vaultDoor01` + `manholeHatch` | Two door types incl. a hatch from one switch |
| `house_modern_24` | Switch-driven `vaultDoor01`, `vaultHatch01`, garage doors (9 stuck doors) | Densest residential example |
| `roadside_checkpoint_05` | Switch opens `jailDoorGrey` / `jailDoorDouble*` | Jail doors, plus elevator doors on other triggers |
| `factory_02` | Switch-driven `vaultDoor01`, `vaultHatch01`, `commercialBulletproofGlassDoor` (13 stuck doors) | Big industrial mix, good soak test |
| `police_station_03` | Switch opens `jailDoorBrown` | Key/jail flavour |
| `store_grocery_02` | Switches open roll-up gates (`rollUpGate3x3/4x3/5x3`) | Multi-block roll-up doors specifically |
| `apartments_04` | Trigger door **with** `Unlock` ticked (`exteriorHouseDoorWhite`) | Regression control: vanilla already unlocks this one — mod must be a no-op |

Teleport there in a creative/dm game: `F1` → `dm`, then
`teleport <x> <y> <z>` after finding the POI with `prefab list`-style
tools, or simply start a Navezgane/pregen game and visit. Every door the
mod touches logs `[STFD] unlocked open door '<block>' at <pos>`.

## Reading the full scan

The raw per-door report (1604 rows: POI, door block, unlock flag, source
type) is reproducible with the scanner above. Rows marked `other-trigger`
are doors driven by motion sensors, trip volumes or quest events — the
mod fixes those too (same `OnBlockTriggered` path), but they are harder
to trigger deliberately in a test session.
