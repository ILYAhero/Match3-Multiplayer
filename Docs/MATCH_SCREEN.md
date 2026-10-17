# Mono Orbit — match screen integration

## Purpose

This slice connects the tested deterministic 9x9 Gameplay engine to a fully interactive
portrait-oriented Unity screen. It does not claim network synchronization or release-ready art.

## Modules

- `MonoOrbit.Gameplay`: deterministic rules, immutable board snapshots, score deltas.
- `MonoOrbit.Application`: offline `PracticeSession`, committed score, revision and reset.
- `MonoOrbit.Presentation`: cached art, touch input, canvas layout and cascade playback.
- `MonoOrbit.Presentation.Editor`: opt-in Unity scene generator.

The scene generator deliberately does **not** run automatically during import.

## Open in Unity

1. In GitHub Desktop choose **feature/match-screen**, click Fetch origin and Pull origin.
2. Make sure Unity Hub opens the repository root (where Assets/Packages/ProjectSettings live).
3. Let Unity recompile; resolve red Console errors before any other step.
4. Use **Tools → Mono Orbit → Create Match Scene**. Save any existing open-scene changes
   first; the generator prompts before replacing the existing Mono Orbit match scene.
5. Open the generated `Assets/MonoOrbit/Scenes/MonoOrbitMatch.unity` if necessary.
6. In Game view choose portrait **9:16**, enter Play Mode.
7. Swipe a cell into an adjacent cell, or tap the first tile then the adjacent tile.
   Invalid swaps are rejected. Valid matches resolve and animate; New Constellation resets score.

## Test plan (not yet executed on this slice)

- EditMode Test Runner shows at least **11** tests, including 3 from PracticeSession.
- Every test is green; no import/assembly errors in Console.
- Board is 9 x 9, no starting matches; each generated board has a legal swap.
- Tap selection, valid and invalid swipes, score changes and reset work.
- After matching, no tile stays invisible and touch unlocks after cascades.
- The restart button works with mouse and touchscreen.
- Review portrait layout on at least 16:9, 19.5:9 and 20:9 aspect ratios.
- Profile texture allocation and frame time on physical Android devices.

## Known scope boundaries

- Weather sprites are generated at load and cached. These are initial in-engine
  visuals, not final authored high-resolution assets or weather abilities.
- Cascade removal/reveal is animated, but final gravity motion, sound,
  GPU-driven weather VFX, haptics and accessibility pass are pending.
- If the board has no available moves after resolution, Gameplay creates a new
  playable board rather than reshuffling the same elements.
- Multiplayer rooms, Relay, turn timers, per-player competitive scoring,
  reconnection and authoritative network commands are **not implemented**.
- Default Unity test packages are already in the project's manifest.

Do not merge this branch or mark the pull request ready until these checks pass.
