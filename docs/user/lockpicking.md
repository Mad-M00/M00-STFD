# Lockpicking doors

## Difficulty

Pick time scales with the door. Base times, before perks:

| Tier | Time | Typical doors |
|---|---|---|
| wood | 10s | house doors, closets, gates, wooden hatches |
| iron | 15s | iron doors, garage doors, roll-ups, chainlink, manholes |
| steel | 20s | jail doors, steel garage doors, bulletproof glass, elevators |
| vault | 30s | vault doors and vault hatches |

Every attempt risks snapping the pick (75% base, like vanilla safes).
Two rules keep that fair:

- **Progress is kept.** A snapped pick does not reset the lock; the
  next attempt resumes where it broke.
- **Retries get safer.** The snap chance only covers the work still
  ahead — a lock at 90% almost never snaps another pick — and after 4
  snaps on the same door, the next attempt is guaranteed to finish.

## Perks

The **Lock Picking** perk applies exactly as it does to safes and
police cars: faster picking, fewer breaks. A high-level infiltrator
opens doors in a few seconds.

## Noise

Picking is not silent:

- **The unlock click** (success) can wake zombies directly behind the
  door — about 3 metres, 50% chance each.
- **A snapping pick** rings out — about 8 metres, 80% chance each.
  That can stand up a whole sleeper room.

**Crouch before you pick.** Sneaking cuts both the reach and the
chance, and the **From The Shadows** perk cuts them further — the same
perk that quiets your footsteps quiets your lockpicking.

Alerted zombies wake up and walk to the door to investigate. They heard
a noise; they haven't seen you. A crouched, perked picker usually gets
away with it. A standing fresh spawn snapping three picks outside the
sleeper dorm does not.

All the numbers on this page are yours to change — see
[Configuration](configuration.md).
