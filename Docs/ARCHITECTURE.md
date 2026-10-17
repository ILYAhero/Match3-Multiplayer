# Mono Orbit — architecture decision record (ADR-001)

**Status:** Accepted as initial project architecture. Production readiness will require Unity and device validation.

## Product boundaries

- Portrait-first, two-player match-3 on a shared **9 × 9** board.
- Modes: simultaneous commands and alternating turns (5-second turn time).
- Each player has an independent score; weather effects and game rules must be deterministic at the host.
- Goal: premium dark visual direction, clear input/feedback, smooth animation, responsive touch layout.
- Join-by-code and reconnect rules require network integration, not simulated local state.

## Layers and ownership

| Layer | Responsibility | Dependency direction |
| --- | --- | --- |
| Gameplay domain | Cells, board, rules, matches, cascades, points | Pure C#, independent of Unity |
| Application | Match lifecycle, players, timers, command processing | Depends on domain |
| Networking | Authentication, session/room, authority, transport, state messages | Depends on application; Unity transport isolated |
| Presentation | UI Toolkit/uGUI, sprites, VFX, audio, accessibility | Reads application state and animations |
| Tests | Rules, deterministic behavior, networking edge cases, UX test scenarios | Domain and integration tests independently |

Do not use a global singleton GameManager for every responsibility. Data-driven configuration comes after the hard invariants and schemas are agreed upon.

## Current domain contracts

- `BoardState` is an immutable row-major snapshot; coordinate `(0, 0)` is bottom-left.
- `MoveValidator.IsLegalSwap` only accepts orthogonally adjacent different, nonempty cells if the swap produces a match at one of the swapped positions.
- `MatchFinder.FindMatches` returns deduplicated coordinates for runs of 3+.
- `MatchEngine.TrySwap` rejects invalid moves without using random numbers. Valid moves produce `MoveResult`, an ordered list of `CascadeStep` events, score delta, and a stable playable final board.
- `BoardGenerator` uses seeded `DeterministicRandom`. Only the host creates the RNG and resolves authoritative outcomes. Xorshift32 is for replay determinism, **not** for security.
- Scoring v1: 10 points per removed piece multiplied by cascade depth (1, 2, ...). Special shapes, combos and weather abilities will have explicit rules and tests in later versions.
- If a stable board has no legal move, v1 **regenerates** it rather than preserving piece counts. This is surfaced via `WasRegenerated`.
- Six IDs: Sun (1), Rain (2), Snow (3), Wind (4), Thunder (5), Cloud (6). ID 0 means Empty.

## Authoritative multiplayer plan

1. Client sends `SwapCommand` with session ID, actor ID, expected revision, source/destination and unique command ID.
2. Host validates membership, timer/mode, revision, and legal swap. Host processes commands serially to prevent divergent states during simultaneous play.
3. Host increments revision, resolves cascades and updates scores exactly once; duplicate command IDs are idempotently rejected/replayed.
4. Host sends ordered outcome plus authoritative board snapshot and scores; clients animate but do not mutate the rules independently.
5. Join/reconnect receives a full snapshot and current revision. Expired turns and disconnections have explicit policies, tested with two actual phones.
6. Server-hosted infrastructure is the long-term competitive option; relay-hosted peer authority is easier initially but vulnerable to host cheating/host disconnect.

Networking technology is not installed by this slice; select packages after integration testing against the project's Unity version.

## Definition of done for each slice

- Core logic tested, including invalid input and edge cases.
- Assembly boundaries build in Unity, with no Console errors.
- Code reviewed by the second contributor through a pull request.
- No references to editor-only APIs in runtime assemblies.
- Performance measured on lower-end Android, not assumed.
- UI and effects tested for legibility, touch input and frame rate once implemented.
- Scene/assets committed with stable `.meta` GUIDs.

## Next slices

1. **Application orchestration:** revisioned moves, score ownership, game state, mode/time policies + tests.
2. **Presentation:** premium Mono Orbit board view, tile assets, responsive portrait layout, touch gestures, animated cascade playback.
3. **Multiplayer:** room code, matchmaking/Relay, command authorization, snapshots, disconnect/rejoin.
4. **Gameplay expansion:** weather abilities with explicit balance and deterministic tests.
5. **Polish & release:** performance profiles, VFX pooling, audio/haptics, accessibility, automated builds, device QA.

## Collaboration

Contributor A owns Gameplay/Application and tests. Contributor B owns Networking/Presentation and integration tests. Separate feature branches and review before merging into main; avoid simultaneous scene edits.
